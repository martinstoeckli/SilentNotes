using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SilentNotes;
using SilentNotes.Services;

namespace SilentNotesTest.Services
{
    [TestClass]
    public class SynchronizationStateTest
    {
        [TestMethod]
        public void InitializesWithUnknownLastSync()
        {
            SynchronizationState state = new SynchronizationState(null);
            Assert.IsFalse(state.IsSynchronizationRunning);
            Assert.IsNull(state.LastFinishedSynchronization);
        }

        [TestMethod]
        public void TryStartSynchronizationState_SetsCorrectState()
        {
            SynchronizationState state = new SynchronizationState(null);
            bool res = state.TryStartSynchronizationState(SynchronizationType.AtStartup);
            Assert.IsTrue(res);
            Assert.IsTrue(state.IsSynchronizationRunning);
            Assert.IsNull(state.LastFinishedSynchronization);
        }

        [TestMethod]
        public void TryStartSynchronizationState_SendsMessage()
        {
            var messengerMock = new Mock<IMessengerService>();
            SynchronizationState state = new SynchronizationState(messengerMock.Object);

            // Message is sent when state changed to running
            bool res = state.TryStartSynchronizationState(SynchronizationType.AtStartup);
            Assert.IsTrue(res);
            messengerMock.Verify(m => m.Send(It.IsAny<SynchronizationIsRunningChangedMessage>()), Times.Once);

            // No other message sent when state didn't change
            res = state.TryStartSynchronizationState(SynchronizationType.AtStartup);
            Assert.IsFalse(res);
            messengerMock.Verify(m => m.Send(It.IsAny<SynchronizationIsRunningChangedMessage>()), Times.Once); // still 1
        }

        [TestMethod]
        public void TryStopSynchronizationState_SetsCorrectState()
        {
            SynchronizationState state = new SynchronizationState(null);
            state.TryStartSynchronizationState(SynchronizationType.AtStartup);
            state.UpdateLastFinishedSynchronization();
            state.StopSynchronizationState();
            Assert.IsFalse(state.IsSynchronizationRunning);
            Assert.IsNotNull(state.LastFinishedSynchronization);
        }

        [TestMethod]
        public void TryStopSynchronizationState_SendsMessage()
        {
            var messengerMock = new Mock<IMessengerService>();
            SynchronizationState state = new SynchronizationState(messengerMock.Object);

            // No message when state didn't change (no sync was running)
            state.StopSynchronizationState();
            messengerMock.Verify(m => m.Send(It.IsAny<SynchronizationIsRunningChangedMessage>()), Times.Never);

            // Message is sent when state changed to running and not running
            state.TryStartSynchronizationState(SynchronizationType.AtStartup);
            state.StopSynchronizationState();
            messengerMock.Verify(m => m.Send(It.IsAny<SynchronizationIsRunningChangedMessage>()), Times.Exactly(2));

            // No other messages sent
            state.StopSynchronizationState();
            messengerMock.Verify(m => m.Send(It.IsAny<SynchronizationIsRunningChangedMessage>()), Times.Exactly(2));
        }
    }
}
