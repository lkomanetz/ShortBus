using ShortBus.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShortBus.Tests.TestEvents {

	public class TestEvent : IEvent {
		public Guid Id { get; set; }
		public int IntegerProperty { get; set; }
		public string StringProperty { get; set; }
	}

}
