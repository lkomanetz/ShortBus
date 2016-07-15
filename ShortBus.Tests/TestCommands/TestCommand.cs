using ShortBus.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShortBus.Tests.TestCommands {

	public class TestCommand : IMessage {

		public Guid Id { get; set; }

	}

}
