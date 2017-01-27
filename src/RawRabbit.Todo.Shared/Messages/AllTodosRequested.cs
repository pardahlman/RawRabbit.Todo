using System.Collections.Generic;

namespace RawRabbit.Todo.Shared.Messages
{
	public class CreateTodoList
	{
		public int Count { get; set; }
	}

	public class TodoListCreated
	{
		public List<Todo> Todos { get; set; }
	}
}
