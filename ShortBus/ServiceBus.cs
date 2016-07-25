using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using ShortBus.Contracts;
using System.Reflection;
using System.Threading;
using System.IO;
using ShortBus.Contracts.MessageHandlers;

namespace ShortBus {

	public class ServiceBus : IServiceBus {

		private Type[] _messageHandlers; 
		private Type[] _messageTypes;
		private string[] _outputQueuePaths;
		private string _inputQueuePath;
		private MessageQueue _inputQueue;
		private const string MESSAGE_HANDLER_INTERFACE_NAME = "IMessageHandler";

		public ServiceBus(
			Type[] handlerTypes,
			string[] outputQueuePaths,
			string inputQueuePath
		) {
			_messageHandlers = handlerTypes;
			_outputQueuePaths = outputQueuePaths;
			_inputQueuePath = inputQueuePath;
			_inputQueue = null;

			_messageTypes = this.FindImplementedMessageTypes(_messageHandlers.ToArray());
			this.InitializeInputQueue(inputQueuePath, _messageTypes);
		}

		public string[] OutputQueuePaths {
			get { return _outputQueuePaths; }
			set { _outputQueuePaths = value; }
		}

		public string InputQueuePath { get { return _inputQueuePath; } }

		public void Publish(IList<IEvent> events) {
			for (short i = 0; i < events.Count; i++) {
				Publish(events[i]);
			}
		}

		public void Publish(IEvent @event) {
			for (short i = 0; i < _outputQueuePaths.Length; i++) {
				SendToMsmq(_outputQueuePaths[i], @event);
			}
		}

		public void Send(IList<ICommand> commands) {
			for (short i = 0; i < commands.Count; i++) {
				Send(commands[i]);
			}
		}

		public void Send(ICommand command) {
			for (short i = 0; i < _outputQueuePaths.Length; i++) {
				SendToMsmq(_outputQueuePaths[i], command);
			}
		}

		private void InitializeInputQueue(string inputQueue, IList<Type> serializedTypes) {
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
			_inputQueue.ReceiveCompleted += _inputQueue_ReceiveCompleted;
			_inputQueue.BeginReceive();
		}

		private void _inputQueue_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e) {
			Message msg = _inputQueue.EndReceive(e.AsyncResult);
			msg.Formatter = new XmlMessageFormatter(_messageTypes);
			ExecuteHandlers((IMessage)msg.Body);
			_inputQueue.BeginReceive();
		}

		private void ExecuteHandlers(IMessage msg) {
			BusMessageType msgType = GetBusMessageType(msg);

			Type[] handlerTypes = FindHandlersFor(msgType);
			if (handlerTypes == null || handlerTypes.Length == 0) {
				return;
			}

			int threadCount = handlerTypes.Length;
			Thread[] handlerThreads = new Thread[threadCount];
			for (short i = 0; i < threadCount; i++) {
				/*
				 * I'm passing in the loop index because the number of threads being created
				 * is equal to the number of handlers that need to be executed.  So each iteration
				 * of the loop needs to correlate to the next handler.
				 */
				handlerThreads[i] = new Thread((index) => {
					int arrayIndex = Convert.ToInt32(index);
					Type handlerType = handlerTypes[arrayIndex];
					object handler = Activator.CreateInstance(handlerType);

					Type handlerInterfaceType = null;
					string methodName = String.Empty;

					if (msgType == BusMessageType.Event) {
						handlerInterfaceType = typeof(IEventHandler<>).MakeGenericType(msg.GetType());
						methodName = "Handle";
					}
					else {
						handlerInterfaceType = typeof(ICommandHandler<>).MakeGenericType(msg.GetType());
						methodName = "Execute";
					}

					handlerInterfaceType.InvokeMember(
						methodName,
						BindingFlags.InvokeMethod,
						null,
						handler,
						new object[] { msg }
					);
				});
				handlerThreads[i].Start(i);
			}

			foreach (Thread thread in handlerThreads) {
				thread.Join();
			}
		}

		private Type[] FindHandlersFor(BusMessageType type) {
			string searchString = String.Empty;
			switch (type) {
				case BusMessageType.Event:
					searchString = "Event";
					break;
				case BusMessageType.Command:
					searchString = "Command";
					break;
			}

			return _messageHandlers
				.Where(x => {
					return x.GetInterfaces().Any(y => y.Name.Contains(searchString));
				})
				.ToArray<Type>();
		}

		private BusMessageType GetBusMessageType(IMessage msg) {
			Type type = msg.GetType();
			Type[] interfaces = type.GetInterfaces();

			return (interfaces.Any(x => x.Name.Contains("Event"))) ?
				BusMessageType.Event :
				BusMessageType.Command;
		}

		private Type[] FindImplementedMessageTypes(Type[] handlerTypes) {
			List<Type> serializedTypes = new List<Type>();

			for (short i = 0; i < handlerTypes.Length; i++) {
				Type[] interfaces = handlerTypes[i].GetInterfaces()
					.Where(x => x.Name.Contains(MESSAGE_HANDLER_INTERFACE_NAME) == false)
					.ToArray<Type>();

				for (short j = 0; j < interfaces.Length; j++) {
					Type[] genericArguments = interfaces[j].GetGenericArguments();

					if (genericArguments.Length == 1) {
						serializedTypes.Add(interfaces[j].GetGenericArguments()[0]);
					}
				}
			}

			return serializedTypes.ToArray<Type>();
		}

		private void SendToMsmq(string queue, IMessage msg) {
			string messageName = msg.GetType().Name;
			Message msmqMsg = new Message();
			msmqMsg.Body = msg;
			msmqMsg.Label = messageName;

			_inputQueue.Send(msmqMsg);
		}
	}

}
