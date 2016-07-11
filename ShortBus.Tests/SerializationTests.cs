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
		private const string XSD_STRING = @"http://www.w3.org/2001/XMLSchema";
		private const string XSI_STRING = @"http://www.w3.org/2001/XMLSchema-instance";

		[TestMethod]
		public void SerializationOfObjectSucceeds() {

			TestEvent msg = new TestEvent() {
				IntegerProperty = 1,
				StringProperty = "Test"
			};

			TestCommand cmd = new TestCommand() {
				Id = Guid.NewGuid()
			};

			ServiceBus bus = new ServiceBus(null, null, null);
			string serializedEvent = bus.SerializeToXml(msg);
			string serializedCommand = bus.SerializeToXml(cmd);

			string expectedEvent = ConstructXml(msg);
			string expectedCommand = ConstructXml(cmd);

			XmlComparer.XmlComparer comparer = new XmlComparer.XmlComparer();
			Assert.IsTrue(comparer.AreEqual(expectedEvent, serializedEvent));
			Assert.IsTrue(comparer.AreEqual(expectedCommand, serializedCommand));
		}

		private string ConstructXml(IMessage msg) {
			IList<Tuple<string, object>> propValuePairs = GetPropertyAndValues(msg);
			string className = msg.GetType().Name;

			string xmlStr = $@"<?xml version='1.0' encoding='utf-16'?>
				<{className} xmlns:xsi='{XSI_STRING}'
					xmlns:xsd='{XSD_STRING}'>
					propValuePairs
				</{className}>
			";

			string propValuePairString = String.Empty;
			for (short i = 0; i < propValuePairs.Count; i++) {
				string propertyName = propValuePairs[i].Item1;
				object value = propValuePairs[i].Item2;
				propValuePairString += $"<{propertyName}>{value}</{propertyName}>\n";
			}

			xmlStr = xmlStr.Replace("propValuePairs", propValuePairString);
			return xmlStr;
		}

		private IList<Tuple<string, object>> GetPropertyAndValues(IMessage msg) {
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
