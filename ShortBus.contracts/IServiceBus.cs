﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShortBus.Contracts {

	public interface IServiceBus {

		string[] OutputQueuePaths { get; }
		string InputQueuePath { get; }
		void Publish(IList<IEvent> evts);
		void Publish(IEvent evt);
		void Send(IList<ICommand> commands);
		void Send(ICommand command);
	}

}
