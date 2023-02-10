using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LoggerLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LoggerTest
{
    internal class TestFileWriter : IFileWriter
    {
        internal int ServerPrintingDelay;

        internal readonly List<string> LogOutputList;

        public TestFileWriter(int serverPrintingDelay)
        {
            ServerPrintingDelay = serverPrintingDelay;
            LogOutputList = new List<string>();
        }


        public void Write(string line)
        {
            LogOutputList.Add(line);
        }
    }

    [TestClass]
    public class LoggerLibTest
    {
        [TestMethod]
        public void LogPrints_clientDelayEqualsServerDelay_PrintsCorrectOrder()
        {
            //Arrange
            var fileWriter = new TestFileWriter(10); // SPD = 10
            const int clientPrintingDelay = 10; // CPD = 10
            var logger = new LoggerLib.LoggerLib(fileWriter);
            var result = Enumerable.Range(0, 1000).Select(i => i.ToString()).ToList();

            //Act
            foreach (var number in result)
            {
                logger.PrintLogLine(number);
                Thread.Sleep(clientPrintingDelay);
            }
            
            logger.WaitForThePrintsToFinish();
            
            //Assert
            var actual = fileWriter.LogOutputList;
            CollectionAssert.AreEqual(result, actual);
        }

        [TestMethod]
        public void LogPrints_clientDelayGreaterThanServerDelay_PrintsCorrectOrder()
        {
            //Arrange
            var fileWriter = new TestFileWriter(10); // SPD = 10
            const int clientPrintingDelay = 30; // CPD = 30
            var logger = new LoggerLib.LoggerLib(fileWriter);
            var result = Enumerable.Range(0, 1000).Select(i => i.ToString()).ToList();

            //Act
            foreach (var number in result)
            {
                logger.PrintLogLine(number);
                Thread.Sleep(clientPrintingDelay);
            }
            
            logger.WaitForThePrintsToFinish();
            
            //Assert
            var actual = fileWriter.LogOutputList;
            CollectionAssert.AreEqual(result, actual);
        }
        [TestMethod]
        public void LogPrints_clientDelayLessThanServerDelay_PrintsCorrectOrder()
        {
            //Arrange
            var fileWriter = new TestFileWriter(30); // SPD = 30
            const int clientPrintingDelay = 10; // CPD = 10
            var logger = new LoggerLib.LoggerLib(fileWriter);
            var result = Enumerable.Range(0, 1000).Select(i => i.ToString()).ToList();

            //Act
            foreach (var number in result)
            {
                logger.PrintLogLine(number);
                Thread.Sleep(clientPrintingDelay);
            }
            
            logger.WaitForThePrintsToFinish();
            
            //Assert
            var actual = fileWriter.LogOutputList;
            CollectionAssert.AreEqual(result, actual);
        }
    }
}