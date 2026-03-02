using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ltht_project.Core;
using ltht_project.Infrastructure;

namespace ltht_project.Gui
{
    internal class GuiManager
    {
        private readonly KPIEngine kpiEngine;
        private readonly FileRegistry fileRegistry;
        private readonly BackgroundWorker backgroundWorker;
        private readonly FileWatcherService fileWatcher;
        private readonly List<string> liveLogs;
        private readonly object logLock = new object();
        private bool isRunning;
        private DateTime startTime;
        private const int CONSOLE_WIDTH = 120;
        private const int MAX_LOGS = 5;
        private const int INDENT = 10;

        public GuiManager(KPIEngine engine, FileRegistry registry, BackgroundWorker worker, FileWatcherService watcher)
        {
            kpiEngine = engine;
            fileRegistry = registry;
            backgroundWorker = worker;
            fileWatcher = watcher;
            liveLogs = new List<string>();
            isRunning = true;
            startTime = DateTime.Now;

            fileWatcher.FileDetected += OnFileDetected;
        }

        private void OnFileDetected(object sender, FileDetectedEventArgs e)
        {
            AddLog($"Phát hiện file mới: {System.IO.Path.GetFileName(e.FilePath)}");
        }

        public void AddLog(string message)
        {
            lock (logLock)
            {
                string logEntry = $"[{DateTime.Now:HH:mm:ss}] {message}";
                liveLogs.Add(logEntry);
                if (liveLogs.Count > MAX_LOGS)
                {
                    liveLogs.RemoveAt(0);
                }
            }
        }

        public async Task RunAsync()
        {
            Console.CursorVisible = false;

            while (isRunning)
            {
                Console.Clear();
                ShowDashboard();

                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    await HandleKeyPress(key);
                }

                await Task.Delay(500);
            }

            Console.CursorVisible = true;
        }

        private async Task HandleKeyPress(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.F1:
                    break;

                case ConsoleKey.F2:
                    ShowDetailsPage();
                    break;

                case ConsoleKey.F3:
                    ShowSystemLogPage();
                    break;

                case ConsoleKey.E:
                    await ShowExportPage();
                    break;

                case ConsoleKey.Q:
                    await ShowQuitPage();
                    break;
            }
        }

        public void ShowDashboard()
        {
            DrawHeader("DASHBOARD");

            Console.WriteLine();
            WriteMidBold("Bảng KPI Tổng hợp:");
            Console.WriteLine();

            int totalSKUs = kpiEngine.CalculateTotalSKUs();
            decimal stockValue = kpiEngine.CalculateStockValue();
            int outOfStock = kpiEngine.CalculateOutOfStock();
            double avgDailySales = kpiEngine.CalculateAvgDailySales();
            double avgInventoryAge = kpiEngine.CalculateAvgInventoryAge();

            WriteMidText($"Tổng số SKU: {totalSKUs}");
            WriteMidText($"Tổng giá trị tồn kho: {stockValue:N2}");
            WriteMidText($"Sản phẩm hết hàng: {outOfStock}");
            WriteMidText($"Doanh số TB/Ngày: {avgDailySales:F1}");
            WriteMidText($"Tuổi kho trung bình: {avgInventoryAge:F1} ngày");

            Console.WriteLine();
            WriteMidBold("Vùng Live Logs (5 dòng cuối):");
            Console.WriteLine();

            lock (logLock)
            {
                if (liveLogs.Count == 0)
                {
                    WriteMidText("[Chưa có hoạt động nào]");
                }
                else
                {
                    foreach (var log in liveLogs)
                    {
                        WriteMidText(log);
                    }
                }
            }

            Console.WriteLine();
            DrawFooter("| [F1] Dashboard | [F2] Chi Tiết | [F3] Nhật Ký Hệ Thống | [E] Xuất Report | [Q] Thoát |");
        }

        public void ShowDetailsPage()
        {
            Console.Clear();
            DrawHeader("BẢNG CHI TIẾT THEO MÃ SKU");

            Console.WriteLine();

            var stats = kpiEngine.GetProductStats().OrderBy(p => p.Key).ToList();

            if (stats.Count == 0)
            {
                WriteMidText("Chưa có dữ liệu sản phẩm");
            }
            else
            {
                string headerRow = String.Format("{0,-20} {1,15} {2,15} {3,20} {4,15}",
                    "Product ID", "Tồn kho", "Giá vốn TB", "Giá trị tồn", "Tuổi kho");
                WriteMidText(headerRow);
                WriteMidText(new string('-', headerRow.Length));

                int displayCount = 0;
                int pageSize = 15;
                int currentPage = 0;

                for (int i = currentPage * pageSize; i < Math.Min((currentPage + 1) * pageSize, stats.Count); i++)
                {
                    var product = stats[i];
                    var ps = product.Value;

                    decimal avgCost = 0;
                    decimal stockValue = 0;
                    double avgAge = 0;

                    if (ps.TotalPurchasedQuantity > 0)
                    {
                        avgCost = ps.TotalPurchaseCost / ps.TotalPurchasedQuantity;
                        if (ps.CurrentStock > 0)
                        {
                            stockValue = ps.CurrentStock * avgCost;

                            if (ps.PurchaseHistory.Any())
                            {
                                var oldestPurchase = ps.PurchaseHistory.OrderBy(p => p.PurchaseDate).First();
                                avgAge = (DateTime.Now - oldestPurchase.PurchaseDate).TotalDays;
                            }
                        }
                    }

                    string row = String.Format("{0,-20} {1,15} {2,15:N2} {3,20:N2} {4,15}",
                        ps.ProductId.Length > 20 ? ps.ProductId.Substring(0, 17) + "..." : ps.ProductId,
                        ps.CurrentStock,
                        avgCost,
                        stockValue,
                        ps.CurrentStock > 0 ? $"{avgAge:F0} ngày" : "0 ngày");

                    WriteMidText(row);
                    displayCount++;
                }

                Console.WriteLine();
                WriteMidText($"Hiển thị {displayCount} / {stats.Count} sản phẩm");
            }

            Console.WriteLine();
            DrawFooter("| [B] Quay lại |");

            WaitForBackKey();
        }

        public void ShowSystemLogPage()
        {
            Console.Clear();
            DrawHeader("NHẬT KÝ HỆ THỐNG");

            Console.WriteLine();

            int queueSize = backgroundWorker.GetQueueSize();
            WriteMidText($"Trạng thái hàng đợi (Queue): {queueSize} tệp đang chờ");

            Console.WriteLine();
            WriteMidBold("Danh sách tệp đã xử lý (Registry)");
            Console.WriteLine();

            var successFiles = fileRegistry.GetSuccessfulFiles().Take(10).ToList();
            var failedFiles = fileRegistry.GetFailedFiles().ToList();

            if (successFiles.Count == 0 && failedFiles.Count == 0)
            {
                WriteMidText("Chưa xử lý file nào");
            }
            else
            {
                WriteMidText("--- Thành công ---");
                foreach (var file in successFiles)
                {
                    string checksum = file.Checksum.Length > 8 ? file.Checksum.Substring(0, 8) + "..." : file.Checksum;
                    WriteMidText($"{file.FileName} - MD5: {checksum} - Thành công - {file.ProcessedDate:HH:mm:ss}");
                }

                if (failedFiles.Count > 0)
                {
                    Console.WriteLine();
                    WriteMidText("--- Thất bại ---");
                    foreach (var file in failedFiles)
                    {
                        WriteMidText($"{file.FileName} - Lỗi: {file.ErrorMessage}");
                    }
                }
            }

            Console.WriteLine();
            TimeSpan uptime = DateTime.Now - startTime;
            WriteMidText($"Uptime: {uptime.Hours:D2}:{uptime.Minutes:D2}:{uptime.Seconds:D2}");

            Console.WriteLine();
            DrawFooter("| [B] Quay lại |");

            WaitForBackKey();
        }

        public async Task ShowExportPage()
        {
            Console.Clear();
            DrawHeader("XUẤT BÁO CÁO (JSON REPORT)");

            Console.WriteLine();

            var stats = kpiEngine.GetProductStats();
            int totalSKUs = stats.Count;

            WriteMidText($"[{DateTime.Now:HH:mm:ss}] Đang tổng hợp dữ liệu từ {totalSKUs} SKUs...");
            await Task.Delay(500);

            WriteMidText($"[{DateTime.Now:HH:mm:ss}] Đang Serialize cấu trúc JSON...");
            await Task.Delay(500);

            try
            {
                string projectRoot = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\"));
                string outputDir = System.IO.Path.Combine(projectRoot, "output");

                if (!System.IO.Directory.Exists(outputDir))
                {
                    System.IO.Directory.CreateDirectory(outputDir);
                }

                string fileName = $"kpi_report_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                string filePath = System.IO.Path.Combine(outputDir, fileName);

                var report = new
                {
                    GeneratedAt = DateTime.Now,
                    Summary = new
                    {
                        TotalSKUs = kpiEngine.CalculateTotalSKUs(),
                        StockValue = kpiEngine.CalculateStockValue(),
                        OutOfStock = kpiEngine.CalculateOutOfStock(),
                        AvgDailySales = kpiEngine.CalculateAvgDailySales(),
                        AvgInventoryAge = kpiEngine.CalculateAvgInventoryAge()
                    },
                    Products = stats.Select(p => new
                    {
                        ProductId = p.Value.ProductId,
                        TotalPurchased = p.Value.TotalPurchasedQuantity,
                        TotalSold = p.Value.TotalSoldQuantity,
                        CurrentStock = p.Value.CurrentStock,
                        TotalCost = p.Value.TotalPurchaseCost,
                        AvgUnitCost = p.Value.TotalPurchasedQuantity > 0 ? p.Value.TotalPurchaseCost / p.Value.TotalPurchasedQuantity : 0,
                        StockValue = p.Value.CurrentStock > 0 && p.Value.TotalPurchasedQuantity > 0
                            ? p.Value.CurrentStock * (p.Value.TotalPurchaseCost / p.Value.TotalPurchasedQuantity)
                            : 0,
                        SalesDays = p.Value.SalesDates.Count
                    }).ToList()
                };

                string json = System.Text.Json.JsonSerializer.Serialize(report, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await Task.Run(() => System.IO.File.WriteAllText(filePath, json));

                WriteMidText($"[{DateTime.Now:HH:mm:ss}] Đã lưu file thành công tại: {filePath}");
                AddLog($"Đã xuất report: {fileName}");
            }
            catch (Exception ex)
            {
                WriteMidText($"[{DateTime.Now:HH:mm:ss}] Lỗi: {ex.Message}");
            }

            Console.WriteLine();
            DrawFooter("| [B] Quay lại |");

            WaitForBackKey();
        }

        public async Task ShowQuitPage()
        {
            Console.Clear();
            DrawHeader("THOÁT ỨNG DỤNG");

            Console.WriteLine();
            WriteMidText("Đang thực hiện tắt máy an toàn...");
            Console.WriteLine();

            WriteMidText($"[{DateTime.Now:HH:mm:ss}] 1. Dừng tiếp nhận file...");
            fileWatcher.Stop();
            await Task.Delay(500);

            WriteMidText($"[{DateTime.Now:HH:mm:ss}] 2. Đợi xử lý nốt hàng đợi ({backgroundWorker.GetQueueSize()} files)...");
            await Task.Delay(2000);

            WriteMidText($"[{DateTime.Now:HH:mm:ss}] 3. Dừng background workers...");
            await backgroundWorker.StopAsync();
            await Task.Delay(500);

            WriteMidText($"[{DateTime.Now:HH:mm:ss}] 4. Lưu Registry (đã xử lý {fileRegistry.GetSuccessfulFiles().Count()} files)...");
            await Task.Delay(500);

            WriteMidText($"[{DateTime.Now:HH:mm:ss}] 5. Giải phóng tài nguyên...");
            fileWatcher.Dispose();
            await Task.Delay(500);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            WriteMidText("Hoàn Tất");
            Console.ResetColor();
            Console.WriteLine();

            WriteMidText("Tự động thoát sau 2 giây...");

            isRunning = false;

            await Task.Delay(2000);
            Environment.Exit(0);
        }

        private void DrawHeader(string title)
        {
            string line = new string('=', CONSOLE_WIDTH);
            Console.WriteLine(line);
            Console.WriteLine(CenterText(title));
            Console.WriteLine(line);
        }

        private void DrawFooter(string text)
        {
            string line = new string('=', CONSOLE_WIDTH);
            Console.WriteLine(line);
            Console.WriteLine(CenterText(text));
            Console.WriteLine(line);
        }

        private string CenterText(string text)
        {
            if (text.Length >= CONSOLE_WIDTH)
                return text;

            int padding = (CONSOLE_WIDTH - text.Length) / 2;
            return new string(' ', padding) + text;
        }

        private void WriteMidText(string text)
        {
            Console.WriteLine(new string(' ', INDENT) + text);
        }

        private void WriteMidBold(string text)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(new string(' ', INDENT) + "** " + text + " **");
            Console.ResetColor();
        }

        private void WaitForBackKey()
        {
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.B || key.Key == ConsoleKey.Escape)
                {
                    break;
                }
            }
        }
    }
}
