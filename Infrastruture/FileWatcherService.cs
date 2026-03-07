using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ltht_project.Infrastructure
{
    internal class FileWatcherService
    {
        private readonly List<FileSystemWatcher> watchers;
        private readonly ConcurrentQueue<string> fileQueue;
        private readonly FileRegistry fileRegistry;
        private bool isRunning;
        public event EventHandler<FileDetectedEventArgs> FileDetected;

        public FileWatcherService(FileRegistry registry)
        {
            watchers = new List<FileSystemWatcher>();
            fileQueue = new ConcurrentQueue<string>();
            fileRegistry = registry;
            isRunning = false;
        }

        public void AddWatchDirectory(string path, string filter = "*.json")
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var watcher = new FileSystemWatcher
            {
                Path = path,
                Filter = filter,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
                EnableRaisingEvents = false
            };

            watcher.Created += OnFileDetected;
            watcher.Renamed += OnFileRenamed;

            watchers.Add(watcher);
        }

        public void Start()
        {
            if (isRunning)
            {
                return;
            }

            isRunning = true;

            foreach (var watcher in watchers)
            {
                watcher.EnableRaisingEvents = true;

                ProcessExistingFiles(watcher.Path, watcher.Filter);
            }
        }

        public void Stop()
        {
            if (!isRunning)
            {
                return;
            }

            isRunning = false;

            foreach (var watcher in watchers)
            {
                watcher.EnableRaisingEvents = false;
            }
        }

        public bool TryDequeueFile(out string filePath)
        {
            return fileQueue.TryDequeue(out filePath);
        }

        public int GetQueueCount()
        {
            return fileQueue.Count;
        }

        private void ProcessExistingFiles(string path, string filter)
        {
            try
            {
                var files = Directory.GetFiles(path, filter);

                foreach (var file in files)
                {
                    if (!fileRegistry.IsFileProcessed(file))
                    {
                        EnqueueFile(file);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void OnFileDetected(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                WaitForFileReady(e.FullPath);

                if (!fileRegistry.IsFileProcessed(e.FullPath))
                {
                    EnqueueFile(e.FullPath);
                }
            }
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            WaitForFileReady(e.FullPath);

            if (!fileRegistry.IsFileProcessed(e.FullPath))
            {
                EnqueueFile(e.FullPath);
            }
        }

        private void EnqueueFile(string filePath)
        {
            fileQueue.Enqueue(filePath);

            OnFileDetected(new FileDetectedEventArgs(filePath));
        }

        private void WaitForFileReady(string filePath, int maxRetries = 5, int delayMs = 500)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        return;
                    }
                }
                catch (IOException)
                {
                    if (i < maxRetries - 1)
                    {
                        Thread.Sleep(delayMs);
                    }
                }
            }
        }

        protected virtual void OnFileDetected(FileDetectedEventArgs e)
        {
            FileDetected?.Invoke(this, e);
        }

        public void Dispose()
        {
            Stop();

            foreach (var watcher in watchers)
            {
                watcher.Dispose();
            }

            watchers.Clear();
        }
    }
}
