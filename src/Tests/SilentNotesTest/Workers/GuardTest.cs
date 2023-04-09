using System;
using NUnit.Framework;
using SilentNotes.Workers;

namespace SilentNotesTest.Workers
{
    [TestFixture]
    public class GuardTest
    {
        [Test]
        public void GuardExecutesActionWhenDisposed()
        {
            string value = "start";
            using (var testGuard = new Guard(() => value = "end"))
            {
                value = "between";
            }
            Assert.AreEqual("end", value);
        }

        [Test]
        public void GuardExecutesActionIfExceptionIsThrown()
        {
            string value = "start";
            try
            {
                using (var testGuard = new Guard(() => value = "end"))
                {
                    value = "between";
                    throw new Exception("just a test");
                }
            }
            catch (Exception)
            {
            }
            Assert.AreEqual("end", value);
        }

        [Test]
        public void GuardExecutesActionIfMethodReturnsPrematurely()
        {
            State = "start";
            ReturnPrematurely();
            Assert.AreEqual("end", State);
        }

        private string State { get; set; }

        private void ReturnPrematurely()
        {
            using (var testGuard = new Guard(() => State = "end"))
            {
                return;
                State = "between";
            }
        }
    }
}
