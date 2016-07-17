using ShortBus.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShortBus.Contracts.MessageHandlers {

	public interface IMessageHandler<T> where T : IMessage {

	}

}
