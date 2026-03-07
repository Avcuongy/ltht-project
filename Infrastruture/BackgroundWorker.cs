using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ltht_project.Core;
using ltht_project.Model;
using ltht_project.Infrastructure;

namespace ltht_project.Infrastructure
{
    internal class BackgroundWorker
    {
        private readonly FileWatcherService fileWatcher;
        private readonly FileRegistry fileRegistry;
        private readonly KPIEngine kpiEngine;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly List<Task> workerTasks;
        private readonly int workerCount;
        private bool isRunning;
        public event EventHandler<FileProcessedEventArgs> FileProcessed;

        public BackgroundWorker(FileWatcherService watcher, FileRegistry registry, KPIEngine engine, int workerCount = 2)
        {
            fileWatcher = watcher;
            fileRegistry = registry;
            kpiEngine = engine;
            this.workerCount = workerCount;
            cancellationTokenSource = new CancellationTokenSource();
            workerTasks = new List<Task>();
            isRunning = false;
        }

        public void Start()
        {
            if (isRunning)
            {
                return;
            }

            isRunning = true;

            for (int i = 0; i < workerCount; i++)
            {
                var task = Task.Run(() => ProcessFilesAsync(cancellationTokenSource.Token), cancellationTokenSource.Token);
                workerTasks.Add(task);
            }
        }

        public async Task StopAsync()
        {
            if (!isRunning)
            {
                return;
            }

            isRunning = false;
            cancellationTokenSource.Cancel();

            try
            {
                await Task.WhenAll(workerTasks);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                workerTasks.Clear();
            }
        }

        private async Task ProcessFilesAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (fileWatcher.TryDequeueFile(out string filePath))
                    {
                        await ProcessFileAsync(filePath);
                    }
                    else
                    {
                        await Task.Delay(1000, cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                }
            }
        }

        private async Task ProcessFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            if (fileRegistry.IsFileProcessed(filePath))
            {
                return;
            }

            if (!fileRegistry.MarkAsProcessing(filePath))
            {
                return;
            }

            try
            {
                string json = await ReadFileAsync(filePath);

                if (string.IsNullOrWhiteSpace(json))
                {
                    fileRegistry.MarkAsFailed(filePath, "Empty file");
                    OnFileProcessed(new FileProcessedEventArgs(filePath, false, 0, "Empty file"));
                    return;
                }

                int recordCount = 0;

                if (filePath.Contains("invoice"))
                {
                    recordCount = await ProcessInvoicesAsync(json, filePath);
                }
                else if (filePath.Contains("purchase-order"))
                {
                    recordCount = await ProcessPurchaseOrdersAsync(json, filePath);
                }
                else
                {
                    fileRegistry.MarkAsFailed(filePath, "Unknown file type");
                    OnFileProcessed(new FileProcessedEventArgs(filePath, false, 0, "Unknown file type"));
                    return;
                }

                fileRegistry.MarkAsSuccess(filePath);
                OnFileProcessed(new FileProcessedEventArgs(filePath, true, recordCount, null));
            }
            catch (Exception ex)
            {
                fileRegistry.MarkAsFailed(filePath, ex.Message);
                OnFileProcessed(new FileProcessedEventArgs(filePath, false, 0, ex.Message));
            }
        }

        private async Task<string> ReadFileAsync(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            using (var reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        private async Task<int> ProcessInvoicesAsync(string json, string filePath)
        {
            return await Task.Run(() =>
            {
                var invoices = JsonSerializer.Deserialize<List<Invoice>>(json);

                if (invoices == null || invoices.Count == 0)
                {
                    throw new Exception("No invoices found in file");
                }

                foreach (var invoice in invoices)
                {
                    if (invoice != null)
                    {
                        kpiEngine.ProcessInvoice(invoice);
                    }
                }

                return invoices.Count;
            });
        }

        private async Task<int> ProcessPurchaseOrdersAsync(string json, string filePath)
        {
            return await Task.Run(() =>
            {
                var orders = JsonSerializer.Deserialize<List<PurchaseOrder>>(json);

                if (orders == null || orders.Count == 0)
                {
                    throw new Exception("No purchase orders found in file");
                }

                foreach (var order in orders)
                {
                    if (order != null)
                    {
                        kpiEngine.ProcessPurchaseOrder(order);
                    }
                }

                return orders.Count;
            });
        }

        protected virtual void OnFileProcessed(FileProcessedEventArgs e)
        {
            FileProcessed?.Invoke(this, e);
        }

        public int GetQueueSize()
        {
            return fileWatcher.GetQueueCount();
        }

        public bool IsRunning => isRunning;
    }
}