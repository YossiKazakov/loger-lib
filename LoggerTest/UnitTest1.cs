using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LoggerLib;
using LoggerLib.Filters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LoggerTest
{
    internal class TestFileWriter : IFileWriter
    //Fake FileWriter class that enables to store the given messages
    {
        private readonly int _serverPrintingDelay;

        internal readonly List<string> LogOutputList;

        public TestFileWriter(int serverPrintingDelay = 0)
        {
            _serverPrintingDelay = serverPrintingDelay;
            LogOutputList = new List<string>();
        }
        
        public void Write(string line)
        {
            LogOutputList.Add(line);
            Thread.Sleep(_serverPrintingDelay);
        }
    }

    [TestClass]
    public class LoggerLibTest
    {
        private const int NumberOfLogs = 1000;

        private static readonly List<string> DefaultIntegerMessages =
            Enumerable.Range(0, NumberOfLogs).Select(i => i.ToString()).ToList();


        private static List<string> SetupTestAndReturnResults(int serverPrintingDelay = 0,
            int clientPrintingDelay = 0, List<string> messagesToActOn = null,
            List<ILogFilter> filters = null)
        {
            //Arrange
            var fileWriter = new TestFileWriter(serverPrintingDelay);
            var logger = new LoggerLib.LoggerLib(fileWriter, filters);
            messagesToActOn ??= DefaultIntegerMessages;

            //Act
            foreach (var number in messagesToActOn)
            {
                logger.PrintLogLine(number);
                Thread.Sleep(clientPrintingDelay);
            }

            logger.WaitForThePrintsToFinish();

            //Return actual result
            return fileWriter.LogOutputList;
        }
        

        #region DifferentDelayRatioTests

        [TestMethod]
        public void CorrectOrderLogPrints_BothHaveNoDelay_PrintsCorrectOrder()
        {
            //Arrange + Act
            var actual = SetupTestAndReturnResults(); // CPD = SPD = 0

            //Assert
            CollectionAssert.AreEqual(DefaultIntegerMessages, actual);
        }

        [TestMethod]
        public void CorrectOrderLogPrints_ClientDelayEqualsServerDelay_PrintsCorrectOrder()
        {
            //Arrange + Act
            var actual = SetupTestAndReturnResults(10, 10);

            //Assert
            CollectionAssert.AreEqual(DefaultIntegerMessages, actual);
        }

        [TestMethod]
        public void CorrectOrderLogPrints_ClientDelayGreaterThanServerDelay_PrintsCorrectOrder()
        {
            //Arrange + Act
            var actual = SetupTestAndReturnResults(10, 30);

            //Assert
            CollectionAssert.AreEqual(DefaultIntegerMessages, actual);
        }

        [TestMethod]
        public void CorrectOrderLogPrints_ClientDelayLessThanServerDelay_PrintsCorrectOrder()
        {
            //Arrange + Act
            var actual = SetupTestAndReturnResults(30, 10);

            //Assert
            CollectionAssert.AreEqual(DefaultIntegerMessages, actual);
        }

        #endregion

        #region FilterTests

        [TestMethod]
        public void FilterApplying_RemoveSpecialCharactersFilterApplied_FiltersAppliedCorrectly()
        {
            //Arrange - Construct the messages to be filtered
            const string specialCharacters = "!@#$%^&*()_+=/?.,`~";
            var random = new Random();
            var messagesToBeFiltered = new List<string>();

            //For each i in 0 to NumberOfLogs add some special characters randomly
            for (int i = 0; i < NumberOfLogs; i++)
            {
                //Generating a string of some random special character with a random length of 1 to 5
                var specialCharactersAddition = string.Join("",
                    Enumerable.Range(1, random.Next(1, 5))
                        .Select(_ => specialCharacters[random.Next(specialCharacters.Length)]));

                var singleMessage = i + specialCharactersAddition;
                messagesToBeFiltered.Add(singleMessage);
            }

            //Act
            var actual = SetupTestAndReturnResults(
                serverPrintingDelay: 0,
                clientPrintingDelay: 0,
                messagesToActOn: messagesToBeFiltered,
                filters: new List<ILogFilter> { new RemoveSpecialCharactersFilter() }
            );

            //Assert
            CollectionAssert.AreEqual(DefaultIntegerMessages, actual);
        }

        #endregion
    }
}

// internal class LoggerBuilder
// {
//     private TestFileWriter _fileWriter;
//     private List<ILogFilter> _filters = null;
//
//     internal LoggerBuilder WithServerPrintingDelayOf(int serverPrintingDelay)
//     {
//         _fileWriter = new TestFileWriter(serverPrintingDelay);
//         return this;
//     }
//
//     internal LoggerBuilder WithOutServerPrintingDelay()
//     {
//         _fileWriter = new TestFileWriter();
//         return this;
//     }
//
//     internal LoggerBuilder WithFilters(List<ILogFilter> filters)
//     {
//         _filters = filters;
//         return this;
//     }
//
//     internal LoggerLib.LoggerLib Build()
//     {
//         return new LoggerLib.LoggerLib(_fileWriter, _filters);
//     }
// }
