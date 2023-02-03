#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LoggerLib.Filters;

namespace LoggerLib
{
    public class LoggerLib : ILoggerLib
    {
        private readonly Queue<string> _messages = new();
        private readonly object _writingLock = new();
        private readonly AutoResetEvent _mSignal = new(false);
        private readonly AutoResetEvent _workDoneSignal = new(false);


        private readonly IFileWriter _mFileWriter;

        //Filters
        private readonly List<ILogFilter>? _logFilters;

        public LoggerLib(IFileWriter fileWriter, List<ILogFilter>? logFilters)
        {
            _mFileWriter = fileWriter;
            _logFilters = logFilters;
            Task.Run(DoWork);
            // new Thread(DoWork) { IsBackground = true }.Start();
            // new Thread(DoWork) { IsBackground = true }.Start();
            // new Thread(DoWork) { IsBackground = true }.Start();
        }

        private string ApplyFilters(string message)
        {
            if (_logFilters == null)
                return message;

            foreach (var filter in _logFilters) //TODO Could be null
            {
                message = filter.Filter(message);
            }

            return message;
        }

        public void PrintLogLine(string message)
        {
            //Filter the log
            message = ApplyFilters(message);

            // Lock is not needed here, since nothing can get overriden
            _messages.Enqueue(message);

            //Signal the waiting thread(s) that there is a new message to process
            _mSignal.Set();
        }

        private void DequeueAndWriteMessages()
        {
            while (_messages.Any())
            {
                //Get oldest message in the queue
                var messageToSend = _messages.Dequeue();

                //Write through the Filter Writer
                _mFileWriter.Write($"{Environment.CurrentManagedThreadId} {messageToSend}");
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
                _mSignal.WaitOne();

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


//////////////////////////////////////////////////////////////////////////////
// using System;
// using System.Collections.Generic;
// using System.Threading;
// using System.Threading.Tasks;
// using LoggerLib.Filters;
//
// namespace LoggerLib
// {
//     public class LoggerLib : ILoggerLib
//     {
//         private readonly Queue<string> _mMessages = new();
//         private readonly object _queueLock = new();
//         private readonly AutoResetEvent _mSignal = new(false);
//         private readonly IFileWriter _mFileWriter;
//
//         //Filters
//         private readonly List<ILogFilter> _logFilters;
//
//         public LoggerLib(IFileWriter fileWriter, List<ILogFilter> logFilters = null)
//         {
//             _mFileWriter = fileWriter;
//             _logFilters = logFilters;
//             Task.Run(DoWork); 
//             // new Thread(DoWork)
//             // {
//             //     IsBackground = true
//             // }.Start();
//         }
//
//         private string ApplyFilters(string message)
//         {
//             if (_logFilters == null)
//                 return message;
//
//             foreach (var filter in _logFilters) //TODO Could be null
//             {
//                 message = filter.Filter(message);
//             }
//
//             return message;
//         }
//
//         public void PrintLogLine(string message)
//         {
//             //Filter the log
//             message = ApplyFilters(message);
//             // Lock is not needed here, since nothing can get overriden
//             {
//                 _mMessages.Enqueue(message);
//             }
//
//             //Signal the threads that waiting in WaitOne() to continue (Only one gets in)
//             _mSignal.Set();
//         }
//
//         private void DoWork()
//         {
//             Console.WriteLine("doing work...");
//             while (true)
//             {
//                 //No need to Reset since it is now AutoResetEvent
//                 _mSignal.WaitOne();
//
//                 //Has to be locked. A reason for raising an error can be that thread is trying to dequeue 
//                 //an empty queue
//                 lock (_queueLock)
//                 {
//                     string messageToSend;
//                     while (_mMessages.Count > 0)
//                     {
//                         //Write through the Filer Writer
//                         messageToSend = _mMessages.Dequeue();
//
//                         _mFileWriter.Write(Thread.CurrentThread.ManagedThreadId +
//                                            " " + messageToSend);
//                     }
//                 }
//
//                 // Delay backend printing (simulate IO delay - DO NOT CHANGE)
//                 Thread.Sleep(20);
//             }
//         }
//     }
// }