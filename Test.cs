using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ltht_project.Core;
using ltht_project.Infrastructure;
using ltht_project.Model;

namespace ltht_project
{
    internal class Test
    {
        public static async Task RunTestAsync()
        {
            Console.WriteLine("=== Inventory KPI Calculation System Test ===\n");

            // Lấy đường dẫn project root (lên 2 cấp từ bin\Debug)
            string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\"));
            string invoicesPath = Path.Combine(projectRoot, "data", "invoices");
            string purchaseOrdersPath = Path.Combine(projectRoot, "data", "purchase-orders");

            Console.WriteLine($"Project root: {projectRoot}");
            Console.WriteLine($"Invoices path: {invoicesPath}");
            Console.WriteLine($"Purchase orders path: {purchaseOrdersPath}");
            Console.WriteLine($"Registry will be saved at: {Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test-registry.json")}\n");

            var fileRegistry = new FileRegistry("test-registry.json");
            var kpiEngine = new KPIEngine();
            var fileWatcher = new FileWatcherService(fileRegistry);
            var backgroundWorker = new BackgroundWorker(fileWatcher, fileRegistry, kpiEngine, workerCount: 2);

            fileWatcher.AddWatchDirectory(invoicesPath, "*.json");
            fileWatcher.AddWatchDirectory(purchaseOrdersPath, "*.json");

            fileWatcher.FileDetected += (sender, e) =>
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] File detected: {Path.GetFileName(e.FilePath)}");
            };

            Console.WriteLine("Starting file watcher and background worker...\n");
            fileWatcher.Start();
            backgroundWorker.Start();

            Console.WriteLine("Processing existing files...\n");
            await Task.Delay(3000);

            Console.WriteLine("\n=== File Processing Summary ===");
            Console.WriteLine($"Queue size: {backgroundWorker.GetQueueSize()}");

            var successFiles = fileRegistry.GetSuccessfulFiles().ToList();
            var failedFiles = fileRegistry.GetFailedFiles().ToList();

            Console.WriteLine($"Successfully processed: {successFiles.Count} files");
            foreach (var file in successFiles)
            {
                Console.WriteLine($"  ✓ {file.FileName} - {file.ProcessedDate:yyyy-MM-dd HH:mm:ss}");
            }

            if (failedFiles.Any())
            {
                Console.WriteLine($"\nFailed: {failedFiles.Count} files");
                foreach (var file in failedFiles)
                {
                    Console.WriteLine($"  ✗ {file.FileName} - {file.ErrorMessage}");
                }
            }

            Console.WriteLine("\n=== KPI Calculation Results ===\n");

            Console.WriteLine("1. Total SKUs");
            Console.WriteLine($"   Count: {kpiEngine.CalculateTotalSKUs()}\n");

            Console.WriteLine("2. Cost of Inventory (Stock Value)");
            Console.WriteLine($"   Total: {kpiEngine.CalculateStockValue():C}\n");

            Console.WriteLine("3. Out-of-Stock Items");
            Console.WriteLine($"   Count: {kpiEngine.CalculateOutOfStock()}\n");

            Console.WriteLine("4. Average Daily Sales");
            Console.WriteLine($"   Average: {kpiEngine.CalculateAvgDailySales():F2} units/day\n");

            Console.WriteLine("5. Average Inventory Age");
            Console.WriteLine($"   Average: {kpiEngine.CalculateAvgInventoryAge():F2} days\n");

            Console.WriteLine("=== Product-Level Details ===\n");
            var stats = kpiEngine.GetProductStats();

            foreach (var product in stats.OrderBy(p => p.Key))
            {
                var ps = product.Value;
                Console.WriteLine($"Product: {ps.ProductId}");
                Console.WriteLine($"  Purchased: {ps.TotalPurchasedQuantity}");
                Console.WriteLine($"  Sold: {ps.TotalSoldQuantity}");
                Console.WriteLine($"  Current Stock: {ps.CurrentStock}");

                if (ps.PurchaseHistory.Any())
                {
                    decimal avgCost = ps.TotalPurchaseCost / ps.TotalPurchasedQuantity;
                    Console.WriteLine($"  Avg Unit Cost: {avgCost:C}");

                    if (ps.CurrentStock > 0)
                    {
                        Console.WriteLine($"  Stock Value: {(ps.CurrentStock * avgCost):C}");
                    }
                }

                if (ps.SalesDates.Any())
                {
                    Console.WriteLine($"  Sales Days: {ps.SalesDates.Count}");
                }

                Console.WriteLine();
            }

            Console.WriteLine("\nPress any key to test live monitoring (waiting 30 seconds)...");
            Console.ReadKey();

            Console.WriteLine("\nMonitoring for new files for 30 seconds...");
            Console.WriteLine("You can copy new JSON files to the data folders to test live processing.\n");

            await Task.Delay(30000);

            Console.WriteLine("\n\nStopping background worker...");
            await backgroundWorker.StopAsync();

            Console.WriteLine("Stopping file watcher...");
            fileWatcher.Stop();
            fileWatcher.Dispose();

            Console.WriteLine("\n=== Test Complete ===");
        }
    }
}