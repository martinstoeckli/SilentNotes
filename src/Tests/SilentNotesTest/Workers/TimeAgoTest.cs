using System;
using NUnit.Framework;
using SilentNotes.Workers;

namespace SilentNotesTest.Workers
{
    [TestFixture]
    public class TimeAgoTest
    {
        [Test]
        public void PrettyPrintStepsAtCorrectThresholds()
        {
            TimeAgo timeAgo = new TimeAgo(CreateTestLocalization());
            Assert.AreEqual("today", timeAgo.PrettyPrint(new DateTime(2011, 11, 11, 11, 11, 11), new DateTime(2011, 11, 11, 11, 11, 11)));
            Assert.AreEqual("today", timeAgo.PrettyPrint(new DateTime(2011, 11, 11, 00, 00, 00), new DateTime(2011, 11, 11, 23, 59, 59)));
            Assert.AreEqual("yesterday", timeAgo.PrettyPrint(new DateTime(2011, 10, 01), new DateTime(2011, 10, 02)));
            Assert.AreEqual("yesterday", timeAgo.PrettyPrint(new DateTime(2011, 10, 31), new DateTime(2011, 11, 01)));
            Assert.AreEqual("2 days", timeAgo.PrettyPrint(new DateTime(2011, 10, 01), new DateTime(2011, 10, 03)));
            Assert.AreEqual("2 days", timeAgo.PrettyPrint(new DateTime(2011, 10, 31), new DateTime(2011, 11, 02)));

            Assert.AreEqual("13 days", timeAgo.PrettyPrint(new DateTime(2011, 10, 01), new DateTime(2011, 10, 14)));
            Assert.AreEqual("2 weeks", timeAgo.PrettyPrint(new DateTime(2011, 10, 01), new DateTime(2011, 10, 15)));
            Assert.AreEqual("2 weeks", timeAgo.PrettyPrint(new DateTime(2011, 10, 31), new DateTime(2011, 11, 14)));

            Assert.AreEqual("8 weeks", timeAgo.PrettyPrint(new DateTime(2011, 10, 01), new DateTime(2011, 11, 30)));
            Assert.AreEqual("2 months", timeAgo.PrettyPrint(new DateTime(2011, 10, 01), new DateTime(2011, 12, 01)));
            Assert.AreEqual("2 months", timeAgo.PrettyPrint(new DateTime(2011, 10, 31), new DateTime(2011, 12, 31)));

            Assert.AreEqual("23 months", timeAgo.PrettyPrint(new DateTime(2011, 10, 01), new DateTime(2013, 09, 30)));
            Assert.AreEqual("23 months", timeAgo.PrettyPrint(new DateTime(2011, 10, 11), new DateTime(2013, 10, 10)));
            Assert.AreEqual("2 years", timeAgo.PrettyPrint(new DateTime(2011, 10, 01), new DateTime(2013, 10, 01)));
            Assert.AreEqual("27 years", timeAgo.PrettyPrint(new DateTime(1984, 10, 01), new DateTime(2011, 10, 01)));
        }

        [Test]
        public void PrettyPrintIgnoresTime()
        {
            TimeAgo timeAgo = new TimeAgo(CreateTestLocalization());

            // The timespan is not a whole day, nevertheless it should be yesterday
            Assert.AreEqual("yesterday", timeAgo.PrettyPrint(new DateTime(2011, 11, 11, 11, 11, 11), new DateTime(2011, 11, 12, 01, 01, 01)));
        }

        [Test]
        public void PrettyPrintCalculatesCorrectWeeksAtLeapYear()
        {
            TimeAgo timeAgo = new TimeAgo(CreateTestLocalization());

            // The timespan is not a whole day, nevertheless it should be yesterday
            Assert.AreEqual("3 weeks", timeAgo.PrettyPrint(new DateTime(2000, 02, 09), new DateTime(2000, 03, 01)));
            Assert.AreEqual("2 weeks", timeAgo.PrettyPrint(new DateTime(2000, 02, 10), new DateTime(2000, 03, 01)));
        }

        [Test]
        public void AgeInYearsIgnoresTime()
        {
            TimeAgo timeAgo = new TimeAgo(CreateTestLocalization());
            Assert.AreEqual(0, timeAgo.AgeInYears(new DateTime(2011, 11, 11, 11, 11, 11), new DateTime(2011, 11, 11, 11, 11, 11)));
            Assert.AreEqual(0, timeAgo.AgeInYears(new DateTime(2011, 11, 11, 11, 11, 11), new DateTime(2011, 11, 11, 23, 11, 11)));
            Assert.AreEqual(0, timeAgo.AgeInYears(new DateTime(2011, 11, 11, 11, 11, 11), new DateTime(2011, 11, 11, 01, 11, 11)));

            Assert.AreEqual(1, timeAgo.AgeInYears(new DateTime(2011, 11, 11, 11, 11, 11), new DateTime(2012, 11, 11, 01, 01, 01)));
        }

        [Test]
        public void AgeInYearsIsNeverNegative()
        {
            TimeAgo timeAgo = new TimeAgo(CreateTestLocalization());
            Assert.AreEqual(0, timeAgo.AgeInYears(new DateTime(2011, 11, 11), new DateTime(2011, 11, 08)));
        }

        [Test]
        public void AgeInYearsReturnsCorrectNumberOfYears()
        {
            TimeAgo timeAgo = new TimeAgo(CreateTestLocalization());
            Assert.AreEqual(0, timeAgo.AgeInYears(new DateTime(2011, 11, 11), new DateTime(2012, 11, 10)));
            Assert.AreEqual(1, timeAgo.AgeInYears(new DateTime(2011, 11, 11), new DateTime(2012, 11, 11)));
            Assert.AreEqual(1, timeAgo.AgeInYears(new DateTime(2011, 11, 11), new DateTime(2012, 11, 12)));

            Assert.AreEqual(100, timeAgo.AgeInYears(new DateTime(1911, 11, 11), new DateTime(2011, 11, 11)));
        }

        [Test]
        public void AgeInMonthsIgnoresTime()
        {
            TimeAgo timeAgo = new TimeAgo(CreateTestLocalization());
            Assert.AreEqual(0, timeAgo.AgeInMonths(new DateTime(2011, 11, 11, 11, 11, 11), new DateTime(2011, 11, 11, 11, 11, 11)));
            Assert.AreEqual(0, timeAgo.AgeInMonths(new DateTime(2011, 11, 11, 11, 11, 11), new DateTime(2011, 11, 11, 23, 11, 11)));
            Assert.AreEqual(0, timeAgo.AgeInMonths(new DateTime(2011, 11, 11, 11, 11, 11), new DateTime(2011, 11, 11, 01, 11, 11)));

            Assert.AreEqual(1, timeAgo.AgeInMonths(new DateTime(2011, 11, 11, 11, 11, 11), new DateTime(2011, 12, 11, 01, 01, 01)));
        }

        [Test]
        public void AgeInMonthsIsNeverNegative()
        {
            TimeAgo timeAgo = new TimeAgo(CreateTestLocalization());
            Assert.AreEqual(0, timeAgo.AgeInMonths(new DateTime(2011, 11, 11), new DateTime(2011, 10, 11)));
        }

        [Test]
        public void AgeInMonthsReturnsCorrectNumberOfYears()
        {
            TimeAgo timeAgo = new TimeAgo(CreateTestLocalization());
            Assert.AreEqual(0, timeAgo.AgeInMonths(new DateTime(2011, 11, 11), new DateTime(2011, 12, 10)));
            Assert.AreEqual(1, timeAgo.AgeInMonths(new DateTime(2011, 11, 11), new DateTime(2011, 12, 11)));
            Assert.AreEqual(1, timeAgo.AgeInMonths(new DateTime(2011, 11, 11), new DateTime(2011, 12, 12)));

            Assert.AreEqual(1200, timeAgo.AgeInMonths(new DateTime(1911, 11, 11), new DateTime(2011, 11, 11)));
        }

        [Test]
        public void AgeInWeeksRoundsCorrectly()
        {
            TimeAgo timeAgo = new TimeAgo(CreateTestLocalization());
            Assert.AreEqual(0, timeAgo.AgeInWeeks(0.0));
            Assert.AreEqual(0, timeAgo.AgeInWeeks(6.999999));
            Assert.AreEqual(1, timeAgo.AgeInWeeks(7.0));
            Assert.AreEqual(1, timeAgo.AgeInWeeks(7.000001));
            Assert.AreEqual(1, timeAgo.AgeInWeeks(13.999999));
            Assert.AreEqual(2, timeAgo.AgeInWeeks(14.000));
            Assert.AreEqual(2, timeAgo.AgeInWeeks(14.000001));
        }

        [Test]
        public void AgeInMonthsHandlesIrregularMonths()
        {
            TimeAgo timeAgo = new TimeAgo(CreateTestLocalization());
            Assert.AreEqual(1, timeAgo.AgeInMonths(new DateTime(2000, 01, 27), new DateTime(2000, 02, 28)));
            Assert.AreEqual(1, timeAgo.AgeInMonths(new DateTime(2000, 01, 28), new DateTime(2000, 02, 28)));
            Assert.AreEqual(0, timeAgo.AgeInMonths(new DateTime(2000, 01, 29), new DateTime(2000, 02, 28)));

            Assert.AreEqual(0, timeAgo.AgeInMonths(new DateTime(2000, 02, 28), new DateTime(2000, 03, 27)));
            Assert.AreEqual(1, timeAgo.AgeInMonths(new DateTime(2000, 02, 28), new DateTime(2000, 03, 28)));
            Assert.AreEqual(1, timeAgo.AgeInMonths(new DateTime(2000, 02, 28), new DateTime(2000, 03, 29)));
        }


        private TimeAgo.Localization CreateTestLocalization()
        {
            return new TimeAgo.Localization
            {
                Today = "today",
                Yesterday = "yesterday",
                NumberOfDaysAgo = "{0} days",
                NumberOfWeeksAgo = "{0} weeks",
                NumberOfMonthsAgo = "{0} months",
                NumberOfYearsAgo = "{0} years",
            };
        }
    }
}
