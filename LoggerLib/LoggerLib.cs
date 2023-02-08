#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LoggerLib.Filters;

namespace LoggerLib
{
    public class LoggerLib : ILoggerLib
    {
        //Producer-Consumer queue of logs
        private readonly ConcurrentQueue<string> _messages = new();
        
        //Dummy object to lock when performing writing
        private readonly object _writingLock = new();
        
        //Signal when a log has been enqueued
        private readonly AutoResetEvent _readyToWriteSignal = new(false);
        
        //Signal when the queue is empty 
        private readonly AutoResetEvent _workDoneSignal = new(false);

        //Writing will be performed by this object
        private readonly IFileWriter _fileWriter;

        //Log filters to apply to each log
        private readonly List<ILogFilter>? _logFilters;

        public LoggerLib(IFileWriter fileWriter, List<ILogFilter>? logFilters)
        {
            _fileWriter = fileWriter;
            _logFilters = logFilters;
            Task.Run(DoWork);
        }

        private string ApplyFilters(string message)
        {
            if (_logFilters == null)
                return message;

            foreach (var filter in _logFilters)
            {
                message = filter.Filter(message);
            }

            return message;
        }

        
        public void PrintLogLine(string message)
        {
            //Filter the log
            message = ApplyFilters(message);

            // Lock is not needed here, since nothing can get overriden, and the main thread is calling
            // the method synchronously
            _messages.Enqueue(message);

            //Signal the waiting thread(s) that there is a new message to process
            _readyToWriteSignal.Set();
        }

        private void DequeueAndWriteMessages()
        {
            while (_messages.Any())
            {
                //Get oldest message in the queue
                _messages.TryDequeue(out var messageToSend);

                //Write through the Filter Writer
                _fileWriter.Write($"{Environment.CurrentManagedThreadId} {messageToSend}");
            }
        }

        private void SignalIfWorkIsDone()
        {
            if (!_messages.Any())
                _workDoneSignal.Set();
        }

        private void DoWork()
        {
            while (true)
            {
                //Thread(s) waiting for the signal to be set
                _readyToWriteSignal.WaitOne();
                
                //Has to be locked, otherwise logs can be printed in the wrong order
                lock (_writingLock)
                {
                    DequeueAndWriteMessages();
                }
                SignalIfWorkIsDone();
            }
        }

        public void WaitForThePrintsToFinish()
        {
            _workDoneSignal.WaitOne();
        }
    }
}

