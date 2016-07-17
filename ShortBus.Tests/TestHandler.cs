using ShortBus.Tests.TestEvents;
using ShortBus.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShortBus.Contracts.MessageHandlers;
using ShortBus.Tests.TestCommands;

namespace ShortBus.Tests {
	public class TestHandler :
		IEventHandler<TestEvent>, 
		ICommandHandler<TestCommand> {

		public void Handle(TestEvent evt) {
			if (evt.Id != Guid.Parse(TestConstants.TestId)) {
				throw new Exception("Id is not what was published.");
			}
		}
		public void Execute(TestCommand cmd) {
			if (cmd.Id != Guid.Parse(TestConstants.TestId)) {
				throw new Exception("Id is not what was sent.");
			}
		}

	}
}
