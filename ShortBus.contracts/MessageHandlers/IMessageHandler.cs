using ShortBus.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShortBus.contracts.MessageHandlers {

	public interface IMessageHandler {

		void Handle(IMessage msg);

	}

}
