using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace InstanceThreadPool
{
    public class InstanceThreadPool
    {
        private readonly ThreadPriority _priority;
        private readonly string? _name;
        private readonly Thread[] _threads;

        public InstanceThreadPool(int maxThreadCount, ThreadPriority priority = ThreadPriority.Normal, string? Name = null)
        {
            if(maxThreadCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxThreadCount), maxThreadCount, "Thread count must be equal or more then 1");

            this._priority = priority;
            this._name = Name;

            _threads = new Thread[maxThreadCount];
            InitializeThreads();
        }

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


        public void Run(Action work) => Run(null!, _ => work());

        public void Run(object parameter, Action<object> work)
        {

        }

        #endregion

        #region [Worker]


        private void ThreadWork()
        {

            throw new NotImplementedException();
        }

        #endregion

    }
}
