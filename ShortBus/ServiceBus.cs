using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using ShortBus.Contracts;
using System.Reflection;
using System.Threading;
using System.Xml.Serialization;
using System.IO;

namespace ShortBus {

	public class ServiceBus : IServiceBus {
		private Type[] _handlerTypes;
		private string[] _outputQueuePaths;
		private string _inputQueuePath;
		private MessageQueue _inputQueue;

		public ServiceBus(
			Type[] handlerTypes,
			string[] outputQueuePaths,
			string inputQueuePath
		) {
			_handlerTypes = handlerTypes;
			_outputQueuePaths = outputQueuePaths;
			_inputQueuePath = inputQueuePath;
			_inputQueue = null;

			this.InitializeInputQueue(inputQueuePath);
		}

		public string[] OutputQueuePaths {
			get { return _outputQueuePaths; }
			set { _outputQueuePaths = value; }
		}

		public string InputQueuePath { get { return _inputQueuePath; } }

		public void Publish(IList<IEvent> evts) {
			throw new NotImplementedException();
		}

		public void Publish(IEvent evt) {
			throw new NotImplementedException();
		}

		public void Send(IList<ICommand> commands) {
			throw new NotImplementedException();
		}

		public void Send(ICommand command) {
			throw new NotImplementedException();
		}

		public void InitializeInputQueue(string inputQueue) {
			if (String.IsNullOrEmpty(inputQueue)) {
				return;
			}

			if (MessageQueue.Exists(inputQueue)) {
				_inputQueue = new MessageQueue(inputQueue);
			}
			else {
				MessageQueue.Create(inputQueue);
				_inputQueue = new MessageQueue(inputQueue);
			}
		}

		internal Type[] FindMessageInstances() {
			List<Type> handlerTypes = new List<Type>();

			for (short i = 0; i < _handlerTypes.Length; i++) {
				Type[] handlers = _handlerTypes[i].GetInterfaces()
					.Where(ifc => ifc.IsGenericType)
					.Select(ifc => ifc.GetGenericArguments()[0])
					.ToArray<Type>();

				handlerTypes.AddRange(handlers);
			}

			return handlerTypes.ToArray<Type>();
		}

		private void ReadQueueAsync() {
			Thread readThread = new Thread(() => {
			});
			readThread.Start();
		}

		private void SendToOutputQueues(IMessage package) {
			Message msg = new Message();
			msg.Body = SerializeToXml(package);
		}

		internal string SerializeToXml(IMessage msg) {
			StringWriter sw = new StringWriter();
			XmlSerializer serializer = new XmlSerializer(msg.GetType());
			serializer.Serialize(sw, msg);

			return sw.ToString();
		}
	}

}
