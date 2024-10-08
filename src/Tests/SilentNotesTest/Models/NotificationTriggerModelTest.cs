﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Models;

namespace SilentNotesTest.Models
{
    [TestClass]
    public class NotificationTriggerModelTest
    {
        [TestMethod]
        public void IsDue_TriggersWhenExpired()
        {
            DateTime createdAt = new DateTime(2023, 05, 10);
            TimeSpan queueTime = TimeSpan.FromDays(10);

            NotificationTriggerModel model = new NotificationTriggerModel { CreatedAt = createdAt };
            Assert.IsTrue(model.IsDue(new DateTime(2023, 05, 20), queueTime));
            Assert.IsTrue(model.IsDue(new DateTime(2023, 05, 21), queueTime));
        }

        [TestMethod]
        public void IsDue_RefusesWhenTooEarly()
        {
            DateTime createdAt = new DateTime(2023, 05, 10);
            TimeSpan queueTime = TimeSpan.FromDays(10);

            NotificationTriggerModel model = new NotificationTriggerModel { CreatedAt = createdAt };
            Assert.IsFalse(model.IsDue(new DateTime(2023, 05, 15), queueTime));
            Assert.IsFalse(model.IsDue(new DateTime(2023, 05, 08), queueTime)); // even before createdAt
        }

        [TestMethod]
        public void IsDue_RefusesWhenAlreadyShown()
        {
            DateTime createdAt = new DateTime(2023, 05, 10);
            TimeSpan queueTime = TimeSpan.FromDays(10);

            NotificationTriggerModel model = new NotificationTriggerModel { CreatedAt = createdAt, ShownAt = createdAt.AddDays(1) };
            Assert.IsFalse(model.IsDue(new DateTime(2023, 05, 21), queueTime));
        }
    }
}
