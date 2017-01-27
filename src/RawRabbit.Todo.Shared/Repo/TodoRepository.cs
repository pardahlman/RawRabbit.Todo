using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RawRabbit.Todo.Shared.Repo
{
	public class TodoRepository : ITodoRepository
	{
		private readonly ConcurrentDictionary<int, Todo> _todos;
		private int _count;

		public TodoRepository()
		{
			_todos = new ConcurrentDictionary<int, Todo>
			{
				[0] = new Todo
				{
					Id = 0,
					Task = "Documentation for RawRabbit 2.0",
					Owner = "pardahlman"
				},
				[1] = new Todo
				{
					Id = 1,
					Task = "Release RawRabbit 2.0",
					Owner = "pardahlman"
				}
			};
			_count = _todos.Count - 1;
		}

		public Task<List<Todo>> GetAllAsync()
		{
			return Task.FromResult(_todos.Values.ToList());
		}

		public Task<Todo> GetAsync(int id)
		{
			var todo = _todos.Values.FirstOrDefault(t => t.Id == id);
			return Task.FromResult(todo);
		}

		public Task<Todo> AddAsync(Todo todo)
		{
			var id = Interlocked.Increment(ref _count);
			todo.Id = id;
			_todos.TryAdd(id, todo);
			return Task.FromResult(todo);
		}

		public Task<List<Todo>> QueryAsync(Predicate<Todo> predicate)
		{
			var result = _todos
				.Where(t => predicate(t.Value))
				.Select(kvp => kvp.Value)
				.ToList();
			return Task.FromResult(result);
		}
	}
}
