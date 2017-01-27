using System;

namespace RawRabbit.Todo.Shared.Messages
{
	public class NotifyClients
	{
		public DateTime Time { get; set; }
		public string Message { get; set; }
	}
}
