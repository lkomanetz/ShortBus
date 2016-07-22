using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShortBus.Contracts.MessageHandlers;
using ShortBus.Tests.TestCommands;
using ShortBus.Tests.TestEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShortBus.Tests {
	public class SecondTestHandler :
		ICommandHandler<TestCommand> {

		public void Execute(TestCommand evt) {
			Assert.IsTrue(
				evt.Id == Guid.Parse(TestConstants.TestId),
				"The event Id parameter was not sent through the Service Bus"
			);
		}

	}

}
