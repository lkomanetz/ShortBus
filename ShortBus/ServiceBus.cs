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
using ShortBus.Contracts.MessageHandlers;

namespace ShortBus {

	public class ServiceBus : IServiceBus {

		private Type[] _handlerTypes;
		private Type[] _serializedTypes;
		private string[] _outputQueuePaths;
		private string _inputQueuePath;
		private MessageQueue _inputQueue;
		private const string MESSAGE_HANDLER_CLASS_NAME = "IMessageHandler";

		public ServiceBus(
			Type[] handlerTypes,
			string[] outputQueuePaths,
			string inputQueuePath
		) {
			_handlerTypes = handlerTypes;
			_outputQueuePaths = outputQueuePaths;
			_inputQueuePath = inputQueuePath;
			_inputQueue = null;

			_serializedTypes = this.FindSerializedTypes(_handlerTypes.ToArray());
			this.InitializeInputQueue(inputQueuePath, _serializedTypes);
		}

		public string[] OutputQueuePaths {
			get { return _outputQueuePaths; }
			set { _outputQueuePaths = value; }
		}

		public string InputQueuePath { get { return _inputQueuePath; } }

		public void Publish(IList<IMessage> evts) {
			for (short i = 0; i < evts.Count; i++) {
				Publish(evts[i]);
			}
		}

		public void Publish(IMessage evt) {
			for (short i = 0; i < _outputQueuePaths.Length; i++) {
				SendToMsmq(_outputQueuePaths[i], evt);
			}
		}

		//public void Send(IList<ICommand> commands) {
		//	throw new NotImplementedException();
		//}

		//public void Send(ICommand command) {
		//	throw new NotImplementedException();
		//}

		public void InitializeInputQueue(string inputQueue, IList<Type> serializedTypes) {
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

			_inputQueue.Formatter = new XmlMessageFormatter(serializedTypes.ToArray());
			_inputQueue.BeginReceive();
			_inputQueue.ReceiveCompleted += _inputQueue_ReceiveCompleted;
		}

		private void _inputQueue_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e) {
			Message msg = _inputQueue.EndReceive(e.AsyncResult);
			msg.Formatter = new XmlMessageFormatter(_serializedTypes);
			IMessage test = msg.Body as IMessage;
			ExecuteHandlers(test);
			_inputQueue.BeginReceive();
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

		private void ExecuteHandlers(IMessage msg) {
			Type passedInType = msg.GetType();
			Type handlerType = _handlerTypes
				.Where(x => {
					return x.GetInterfaces().Any(y => y.Name.Contains(MESSAGE_HANDLER_CLASS_NAME)); })
				.FirstOrDefault();

			if (handlerType == null) {
				return;
			}

			IMessageHandler<IMessage> handler = (IMessageHandler<IMessage>)Activator.CreateInstance(handlerType);
			handler.Handle(msg);
		}

		private void SendToOutputQueues(IMessage package) {
			Message msg = new Message();
			msg.Body = SerializeToXml(package);
		}

		private Type[] FindSerializedTypes(Type[] handlerTypes) {
			List<Type> serializedTypes = new List<Type>();

			for (short i = 0; i < handlerTypes.Length; i++) {
				Type type = handlerTypes[i];
				Type[] interfaces = type.GetInterfaces();

				for (short j = 0; j < interfaces.Length; j++) {
					Type[] genericArguments = interfaces[j].GetGenericArguments();

					if (genericArguments.Length == 1) {
						serializedTypes.Add(interfaces[j].GetGenericArguments()[0]);
					}
				}
			}

			return serializedTypes.ToArray<Type>();
		}

		internal string SerializeToXml(IMessage msg) {
			StringWriter sw = new StringWriter();
			XmlSerializer serializer = new XmlSerializer(msg.GetType());
			serializer.Serialize(sw, msg);

			return sw.ToString();
		}

		internal void SendToMsmq(string queue, IMessage msg) {
			MessageQueue msmq = new MessageQueue(queue);
			Message msmqMsg = new Message();
			msmqMsg.Body = msg;
			msmq.Send(msmqMsg);
		}
	}

}
