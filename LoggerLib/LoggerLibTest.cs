using System;
using System.Collections.Generic;
using System.Threading;
using LoggerLib.Filters;

namespace LoggerLib
{
    class LoggerLibTest
    {
        private static void Main(string[] args)
        {
            IFileWriter fileWriter = new FileWriter();
            var filters = new List<ILogFilter>
            {
                new RemoveSpecialCharactersFilter(),
                new AddDateFilter()
            };
            ILoggerLib logger = new LoggerLib(fileWriter, filters);

            for (int i = 0; i < 100; i++)
            {
                logger.PrintLogLine($"Number {i} \n");

                // Delay client printing
                Thread.Sleep(10);
            }

            logger.WaitForThePrintsToFinish();
            // Console.ReadLine();
        }
    }
}