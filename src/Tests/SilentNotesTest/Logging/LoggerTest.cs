using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
//using SilentNotes.Logging;

namespace SilentNotesTest.Logging
{
    [TestFixture]
    public class LoggerTest
    {
        // todo: reactivate tests
        //[Test]
        //public void AddsLogsWithCorrectLevel()
        //{
        //    Logger logger = new Logger(TimeSpan.FromHours(1));
        //    logger.Log(LogLevel.Information, "message0");
        //    logger.Log(LogLevel.Warning, "message1");
        //    logger.Log(LogLevel.Error, "message2");

        //    var allLogEntries = logger.GetLastLogEntries(LogLevel.Information).ToList();
        //    Assert.AreEqual(LogLevel.Information, allLogEntries[0].Level);
        //    Assert.AreEqual(LogLevel.Warning, allLogEntries[1].Level);
        //    Assert.AreEqual(LogLevel.Error, allLogEntries[2].Level);
        //    Assert.AreEqual("message0", allLogEntries[0].Message);
        //    Assert.AreEqual("message1", allLogEntries[1].Message);
        //    Assert.AreEqual("message2", allLogEntries[2].Message);
        //}

        //[Test]
        //public void FormatsMessagesCorrectly()
        //{
        //    Logger logger = new Logger(TimeSpan.FromHours(1));
        //    logger.LogFormat(LogLevel.Warning, "Be warned from the {0} {1}", 12, "bears");

        //    var allLogEntries = logger.GetLastLogEntries(LogLevel.Information).ToList();
        //    Assert.AreEqual("Be warned from the 12 bears", allLogEntries[0].Message);
        //}

        //[Test]
        //public void WrongFormatDoesntThrow()
        //{
        //    Logger logger = new Logger(TimeSpan.FromHours(1));
        //    logger.LogFormat(LogLevel.Warning, "Be warned from the {0} {1}", 12);

        //    var allLogEntries = logger.GetLastLogEntries(LogLevel.Information).ToList();
        //    Assert.IsTrue(allLogEntries[0].Message.StartsWith("Exception formatting the log message"));
        //}

        //[Test]
        //public void ExpiredLogEntriesAreRemoved()
        //{
        //    Logger logger = new Logger(TimeSpan.FromMilliseconds(5));
        //    logger.Log(LogLevel.Information, "message0");
        //    Thread.Sleep(6);
        //    logger.Log(LogLevel.Information, "message1");

        //    var allLogEntries = logger.GetLastLogEntries(LogLevel.Information).ToList();
        //    Assert.AreEqual(1, allLogEntries.Count);
        //    Assert.AreEqual("message1", allLogEntries[0].Message);
        //}

        //[Test]
        //public void ExceptionLoggingLogsStackTrace()
        //{
        //    Logger logger = new Logger(TimeSpan.FromHours(1));
        //    try
        //    {
        //        Exception innerException =  new Exception("inner text");
        //        throw new Exception("outer text", innerException);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogException(LogLevel.Error, ex);
        //    }

        //    var logEntry = logger.GetLastLogEntries(LogLevel.Information).First();
        //    Assert.IsTrue(logEntry.Message.Contains("inner text"));
        //    Assert.IsTrue(logEntry.Message.Contains("outer text"));
        //    Assert.IsTrue(logEntry.Message.Contains("LoggerTest.cs:"));
        //}

        //[Test]
        //public void GetLastLogEntriesRespectsMinLevel()
        //{
        //    Logger logger = new Logger(TimeSpan.FromHours(1));
        //    logger.Log(LogLevel.Information, "message0");
        //    logger.Log(LogLevel.Warning, "message1");
        //    logger.Log(LogLevel.Error, "message2");

        //    Assert.AreEqual(3, logger.GetLastLogEntries(LogLevel.Information).Count());
        //    Assert.AreEqual(2, logger.GetLastLogEntries(LogLevel.Warning).Count());
        //    Assert.AreEqual(1, logger.GetLastLogEntries(LogLevel.Error).Count());
        //    Assert.AreEqual("message2", logger.GetLastLogEntries(LogLevel.Error).First().Message);
        //}
    }
}
