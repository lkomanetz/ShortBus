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
		private IServiceBus _bus;
		private const string testGuid = "fd9da0bf-7d53-4fa0-87a3-e1ab96d26098";

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
		public void SerializationOfObjectSucceeds() {

			TestEvent msg = CreateEvent();
			TestCommand cmd = CreateCommand();
			ServiceBus bus = new ServiceBus(null, null, null);
			string serializedEvent = bus.SerializeToXml(msg);
			string serializedCommand = bus.SerializeToXml(cmd);

			string expectedEvent = ConstructXml(msg);
			string expectedCommand = ConstructXml(cmd);

			XmlComparer.XmlComparer comparer = new XmlComparer.XmlComparer();
			Assert.IsTrue(comparer.AreEqual(expectedEvent, serializedEvent));
			Assert.IsTrue(comparer.AreEqual(expectedCommand, serializedCommand));
		}

		[TestMethod]
		public void EventHandlerExecutionSucceeds() {
			TestEvent evt = CreateEvent();
			TestCommand cmd = CreateCommand();

			_bus.Publish(evt);
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
				Id = Guid.Parse(TestConstants.TestId)
			};
		}

		private string ConstructXml(IMessage msg) {
			string xsdString = @"http://www.w3.org/2001/XMLSchema";
			string xsiString = @"http://www.w3.org/2001/XMLSchema-instance";
			string className = msg.GetType().Name;

			string xmlStr = $@"<?xml version='1.0' encoding='utf-16'?>
				<{className} xmlns:xsi='{xsiString}'
					xmlns:xsd='{xsdString}'>
					propValuePairs
				</{className}>
			";

			IList<Tuple<string, object>> propValuePairs = GetPropertiesAndValues(msg);
			string propValuePairString = String.Empty;

			for (short i = 0; i < propValuePairs.Count; i++) {
				string propertyName = propValuePairs[i].Item1;
				object value = propValuePairs[i].Item2;
				propValuePairString += $"<{propertyName}>{value}</{propertyName}>\n";
			}

			xmlStr = xmlStr.Replace("propValuePairs", propValuePairString);
			return xmlStr;
		}

		private IList<Tuple<string, object>> GetPropertiesAndValues(IMessage msg) {
			PropertyInfo[] properties = msg.GetType().GetProperties();
			IList<Tuple<string, object>> pairs = new List<Tuple<string, object>>();

			for (short i = 0; i < properties.Length; i++) {
				var valuePair = Tuple.Create<string, object>(
					properties[i].Name,
					properties[i].GetValue(msg)
				);
				pairs.Add(valuePair);
			}

			return pairs;
		}

	}

}
