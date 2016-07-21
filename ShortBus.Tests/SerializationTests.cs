using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShortBus.Contracts;
using ShortBus.Tests.TestEvents;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using XmlComparer;
using ShortBus.Tests.TestCommands;

namespace ShortBus.Tests {

	[TestClass]
	public class SerializationTests {
		private const string MSMQ_QUEUE = @".\private$\unit_test";
		private const string testGuid = "fd9da0bf-7d53-4fa0-87a3-e1ab96d26098";
		private ServiceBus _bus;

		[TestInitialize]
		public void Initialize() {
			Type[] handlers = new Type[1] {
				typeof(TestHandler)
			};

			_bus = new ServiceBus(
				handlers,
				new string[] { MSMQ_QUEUE },
				MSMQ_QUEUE
			);
		}

		[TestMethod]
		public void EventHandlerExecutionSucceeds() {
			TestEvent evt = CreateEvent();
			_bus.Publish(evt);

			int arraySize = 5;
			TestEvent[] events = new TestEvent[arraySize];
			for (short i = 0; i < arraySize; i++) {
				events[i] = CreateEvent();
			}

			_bus.Publish(events);
		}

		[TestMethod]
		public void CommandHandlerExecutionSucceeds() {
			TestCommand cmd = CreateCommand();
			_bus.Send(cmd);


			int arraySize = 5;
			TestCommand[] events = new TestCommand[arraySize];
			for (short i = 0; i < arraySize; i++) {
				events[i] = CreateCommand();
			}

			_bus.Send(events);
		}

		private TestEvent CreateEvent() {
			return new TestEvent() {
				Id = Guid.Parse(TestConstants.TestId),
				IntegerProperty = 1,
				StringProperty = "Test"
			};
		}

		private TestCommand CreateCommand() {
			return new TestCommand() {
				Id = Guid.Parse(TestConstants.TestId),
				StringListProperty = new List<string>() {
					"Test 1",
					"Test 2",
					"Test 3"
				}
			};
		}

	}

}
