using ShortBus.Tests.TestEvents;
using ShortBus.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShortBus.Contracts.MessageHandlers;
using ShortBus.Tests.TestCommands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ShortBus.Tests.TestHandlers {
	public class TestHandler :
		IEventHandler<TestEvent>, 
		ICommandHandler<TestCommand> {

		public void Handle(TestEvent evt) {
			Assert.IsTrue(
				evt.Id == Guid.Parse(TestConstants.TestId),
				"The event Id parameter was not sent through the Service Bus"
			);
		}
		public void Execute(TestCommand cmd) {
			Assert.IsTrue(
				cmd.Id == Guid.Parse(TestConstants.TestId),
				"The command Id parameter was not sent through the Service Bus"
			);
		}

	}
}
