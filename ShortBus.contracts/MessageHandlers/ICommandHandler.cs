using ShortBus.Contracts;
using ShortBus.Contracts.MessageHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShortBus.Contracts.MessageHandlers {

	public interface ICommandHandler<T> where T : ICommand {

		void Handle(T cmd);
	}

}
