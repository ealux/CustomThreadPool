﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace InstanceThreadPool
{
    public class InstanceThreadPool
    {
        #region [Fields]

        // Fields
        private readonly ThreadPriority _priority;

        private readonly string? _name;
        private readonly Thread[] _threads;

        // Works queue
        private readonly Queue<(Action<object?> Work, object? Parameter)> _works = new();

        // Sync primitivies
        private readonly AutoResetEvent _WorkingEvent = new(false);

        private readonly AutoResetEvent _QueueLockEvent = new(true);

        // flag

        #endregion [Fields]

        /// <summary>
        /// Create InstanceThreadPool instance
        /// </summary>
        /// <param name="maxThreadCount">Count of threads in pool</param>
        /// <param name="priority">Priority for threads in pool</param>
        /// <param name="Name">Thread pool name</param>
        /// <exception cref="ArgumentOutOfRangeException">Throw if <paramref name="maxThreadCount"/> less or equal zero</exception>
        public InstanceThreadPool(
            int maxThreadCount,
            ThreadPriority priority = ThreadPriority.Normal,
            string? Name = null)
        {
            if (maxThreadCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxThreadCount), maxThreadCount, "Thread count must be equal or more then 1");

            this._priority = priority;
            this._name = Name;

            _threads = new Thread[maxThreadCount];
            InitializeThreads();
        }

        /// <summary>
        /// Create threads in pool
        /// </summary>
        private void InitializeThreads()
        {
            for (int i = 0; i < _threads.Length; i++)
            {
                var name = $"{nameof(InstanceThreadPool)}[{_name ?? GetHashCode().ToString("x")}]-Thread[{i}]";
                var thread = new Thread(ThreadWork)
                {
                    Name = name,
                    IsBackground = true,
                    Priority = _priority,
                };
                _threads[i] = thread;
                thread.Start();
            }
        }

        #region [Execute section (Public API)]

        /// <summary>
        /// Set work to the thread pool (parameter is null)
        /// </summary>
        /// <param name="work">Action to do</param>
        public void Run(Action work) => Run(null!, _ => work());

        /// <summary>
        /// Set work to the thread pool
        /// </summary>
        /// <param name="parameter">Method parameter</param>
        /// <param name="work">Action to do (with parameter)</param>
        public void Run(object parameter, Action<object?> work)
        {
            // Add work
            _QueueLockEvent.WaitOne();          // Request queue access
            _works.Enqueue((work, parameter));  // Add new work to queue
            _QueueLockEvent.Set();              // Release queue access

            // Access ThreadWork operation
            _WorkingEvent.Set();
        }

        #endregion [Execute section (Public API)]

        #region [Worker]

        /// <summary>
        /// Set current work to thread from pool
        /// </summary>
        private void ThreadWork()
        {
            var thread_name = Thread.CurrentThread.Name; // Handle current thread name
            Trace.TraceInformation($"Thread {thread_name} started with id {Environment.CurrentManagedThreadId}");   // Tracing on start

            // Waiting for work access
            while (true)
            {
                // Waiting event to allow work
                _WorkingEvent.WaitOne();
                // Request queue access
                _QueueLockEvent.WaitOne();

                // Check queue state (is any work here)
                while (_works.Count == 0)                   // If no work ...
                {
                    _QueueLockEvent.Set();                  // Release queue access
                    _WorkingEvent.WaitOne();                // Waiting event to allow work
                    _QueueLockEvent.WaitOne();              // Block queue access and wait one again
                }
                var (work, parameter) = _works.Dequeue();   // Take work

                // If taken work was last
                if (_works.Count > 0)
                    _WorkingEvent.Set();                    // Accept next thread to wait a new job in cycle
                _QueueLockEvent.Set();                      // Release queue access

                // Tracing on working...
                Trace.TraceInformation($"Thread {thread_name}[id{Environment.CurrentManagedThreadId}] is running...");
                try
                {
                    var timer = Stopwatch.StartNew();   // Create timer

                    // Start work
                    work(parameter);

                    // Stop timer
                    timer.Stop();
                    // Tracing on end work
                    Trace.TraceInformation($"Thread {thread_name}[id{Environment.CurrentManagedThreadId}] " +
                        $"complete work with {timer.ElapsedMilliseconds} ms");
                }
                catch (Exception e)
                {
                    Trace.TraceError($"Error occuried on thread {thread_name}: {e}");
                }
            }

            Trace.TraceInformation($"Thread {thread_name} completed");   // Tracing on end
        }

        #endregion [Worker]
    }
}