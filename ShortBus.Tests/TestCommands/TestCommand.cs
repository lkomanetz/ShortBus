using ShortBus.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShortBus.Tests.TestCommands {

	public class TestCommand : ICommand {

		public Guid Id { get; set; }
		public List<string> StringListProperty { get; set; }

	}

}
