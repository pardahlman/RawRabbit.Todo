using System;

namespace RawRabbit.Todo.Shared
{
	public class TodoContext
	{
		public string SessionId { get; set; }
		public string Source { get; set; }
		public string ExecutionId { get; set; }
	}
}
