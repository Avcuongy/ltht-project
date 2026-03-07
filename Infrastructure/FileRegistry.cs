using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

namespace ltht_project.Infrastructure
{
    internal enum ProcessingStatus
    {
        Processing,
        Success,
        Failed
    }
    internal class FileRegistry
    {
        private readonly string registryFilePath;
        private readonly ConcurrentDictionary<string, FileProcessingRecord> processedFiles;
        private readonly object fileLock = new object();

        public FileRegistry(string registryPath = "file-registry.json")
        {
            registryFilePath = registryPath;
            processedFiles = new ConcurrentDictionary<string, FileProcessingRecord>();
            LoadRegistry();
        }

        public bool IsFileProcessed(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            string checksum = CalculateChecksum(filePath);

            if (processedFiles.TryGetValue(fileName, out var record))
            {
                return record.Checksum == checksum && record.Status == ProcessingStatus.Success;
            }

            return false;
        }

        public bool MarkAsProcessing(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            string checksum = CalculateChecksum(filePath);

            var record = new FileProcessingRecord
            {
                FileName = fileName,
                FilePath = filePath,
                Checksum = checksum,
                ProcessedDate = DateTime.Now,
                Status = ProcessingStatus.Processing
            };

            return processedFiles.TryAdd(fileName, record);
        }
        public void MarkAsSuccess(string filePath)
        {
            string fileName = Path.GetFileName(filePath);

            if (processedFiles.TryGetValue(fileName, out var record))
            {
                record.Status = ProcessingStatus.Success;
                record.ProcessedDate = DateTime.Now;
                SaveRegistry();
            }
        }
        public void MarkAsFailed(string filePath, string errorMessage = null)
        {
            string fileName = Path.GetFileName(filePath);

            if (processedFiles.TryGetValue(fileName, out var record))
            {
                record.Status = ProcessingStatus.Failed;
                record.ErrorMessage = errorMessage;
                record.ProcessedDate = DateTime.Now;
                SaveRegistry();
            }
        }
        public IEnumerable<FileProcessingRecord> GetFailedFiles()
        {
            return processedFiles.Values.Where(r => r.Status == ProcessingStatus.Failed);
        }

        public IEnumerable<FileProcessingRecord> GetSuccessfulFiles()
        {
            return processedFiles.Values.Where(r => r.Status == ProcessingStatus.Success);
        }
        private string CalculateChecksum(string filePath)
        {
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
        private void LoadRegistry()
        {
            if (!File.Exists(registryFilePath))
            {
                return;
            }

            try
            {
                string json = File.ReadAllText(registryFilePath);
                var records = JsonSerializer.Deserialize<List<FileProcessingRecord>>(json);

                if (records != null)
                {
                    foreach (var record in records)
                    {
                        processedFiles.TryAdd(record.FileName, record);
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        private void SaveRegistry()
        {
            lock (fileLock)
            {
                try
                {
                    var records = processedFiles.Values.ToList();
                    string json = JsonSerializer.Serialize(records, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                    File.WriteAllText(registryFilePath, json);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
