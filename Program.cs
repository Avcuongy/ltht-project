using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ltht_project.Engine;
using ltht_project.Gui;
using ltht_project.Infrastructure;
using ltht_project.Model;

namespace ltht_project
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.SetWindowSize(Math.Min(120, Console.LargestWindowWidth), Math.Min(40, Console.LargestWindowHeight));
            Console.Title = "Inventory KPI System";

            // Khởi tạo paths
            string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\"));
            string invoicesPath = Path.Combine(projectRoot, "data", "invoices");
            string purchaseOrdersPath = Path.Combine(projectRoot, "data", "purchase-orders");

            var fileRegistry = new FileRegistry("file-registry.json");
            var kpiEngine = new KPIEngine();

            // Load data có sẵn
            LoadExistingData(kpiEngine, invoicesPath, purchaseOrdersPath);

            var fileWatcher = new FileWatcherService(fileRegistry);
            var backgroundWorker = new BackgroundWorker(fileWatcher, fileRegistry, kpiEngine, workerCount: 2);

            // Setup FileWatcher
            fileWatcher.AddWatchDirectory(invoicesPath, "*.json");
            fileWatcher.AddWatchDirectory(purchaseOrdersPath, "*.json");

            // GUI manager
            var guiManager = new GuiManager(kpiEngine, fileRegistry, backgroundWorker, fileWatcher);

            // Event: File detected
            fileWatcher.FileDetected += (sender, e) =>
            {
                guiManager.AddLog($"Phát hiện file mới: {Path.GetFileName(e.FilePath)}");
            };

            // Event: File has been processed
            backgroundWorker.FileProcessed += (sender, e) =>
            {
                if (e.Success)
                {
                    guiManager.AddLog($"Đã xử lý: {Path.GetFileName(e.FilePath)} (+{e.RecordCount} records)");
                }
                else
                {
                    guiManager.AddLog($"Lỗi: {Path.GetFileName(e.FilePath)} - {e.ErrorMessage}");
                }
            };

            // Start file watcher and background worker
            fileWatcher.Start();
            backgroundWorker.Start();

            // Logs
            guiManager.AddLog("Hệ thống đã khởi động");
            guiManager.AddLog($"Đã load {kpiEngine.CalculateTotalSKUs()} SKUs từ dữ liệu có sẵn");
            guiManager.AddLog($"Giám sát: data/invoices/");
            guiManager.AddLog($"Giám sát: data/purchase-orders/");

            await Task.Delay(1000);

            // Run GUI
            await guiManager.RunAsync();
        }

        private static void LoadExistingData(KPIEngine kpiEngine, string invoicesPath, string purchaseOrdersPath)
        {
            // Configure JsonSerializerOptions to be case-insensitive
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true  // Accept any case
            };

            try
            {
                // Load Purchase Orders
                if (Directory.Exists(purchaseOrdersPath))
                {
                    var poFiles = Directory.GetFiles(purchaseOrdersPath, "*.json");
                    foreach (var file in poFiles)
                    {
                        try
                        {
                            string json = File.ReadAllText(file);
                            var orders = JsonSerializer.Deserialize<List<PurchaseOrder>>(json, options);
                            if (orders != null)
                            {
                                foreach (var order in orders)
                                {
                                    if (order != null && !string.IsNullOrWhiteSpace(order.ProductId))
                                    {
                                        kpiEngine.ProcessPurchaseOrder(order);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[WARNING] Failed to load {Path.GetFileName(file)}: {ex.Message}");
                        }
                    }
                }

                // Load Invoices
                if (Directory.Exists(invoicesPath))
                {
                    var invFiles = Directory.GetFiles(invoicesPath, "*.json");
                    foreach (var file in invFiles)
                    {
                        try
                        {
                            string json = File.ReadAllText(file);
                            var invoices = JsonSerializer.Deserialize<List<Invoice>>(json, options);
                            if (invoices != null)
                            {
                                foreach (var invoice in invoices)
                                {
                                    if (invoice != null && !string.IsNullOrWhiteSpace(invoice.ProductId))
                                    {
                                        kpiEngine.ProcessInvoice(invoice);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[WARNING] Failed to load {Path.GetFileName(file)}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to load existing data: {ex.Message}");
            }
        }
    }
}
