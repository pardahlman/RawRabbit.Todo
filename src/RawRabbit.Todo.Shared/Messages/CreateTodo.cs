using RawRabbit.Configuration.Exchange;
using RawRabbit.Enrichers.Attributes;

namespace RawRabbit.Todo.Shared.Messages
{
	[Exchange(Name = "todo", Type = ExchangeType.Topic)]
	[Queue(Name = "attributed_create_todo", MessageTtl = 3000, AutoDelete = false)]
	[Routing(RoutingKey = "create_the_todo", AutoAck = true, PrefetchCount = 50)]
	public class CreateTodo
	{
		public Todo Todo { get; set; }
	}
}
