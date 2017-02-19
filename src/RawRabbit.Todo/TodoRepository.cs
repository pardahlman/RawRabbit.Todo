using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RawRabbit.Todo
{
	public class TodoRepository
	{
		private readonly List<Shared.Todo> _todos;
		private int _count;

		public TodoRepository()
		{
			_todos = new List<Shared.Todo>
			{
				new Shared.Todo
				{
					Id = 0,
					Task = "Documentation for RawRabbit 2.0",
					Owner = "pardahlman"
				},
				new Shared.Todo
				{
					Id = 1,
					Task = "Release RawRabbit 2.0",
					Owner = "pardahlman"
				}
			};
			_count = _todos.Count - 1;
		}

		public Task<List<Shared.Todo>> GetAllAsync()
		{
			return Task.FromResult(_todos);
		}

		public Task<Shared.Todo> GetAsync(int id)
		{
			var todo = _todos.FirstOrDefault(t => t.Id == id);
			return Task.FromResult(todo);
		}

		public Task<Shared.Todo> RemoveAsync(int id)
		{
			var todo = _todos.FirstOrDefault(t => t.Id == id);
			if (todo != null)
			{
				_todos.Remove(todo);
			}
			return Task.FromResult(todo);
		}

		public Task<Shared.Todo> AddAsync(Shared.Todo todo)
		{
			var id = Interlocked.Increment(ref _count);
			todo.Id = id;
			_todos.Add(todo);
			return Task.FromResult(todo);
		}

		public Task<List<Shared.Todo>> QueryAsync(Predicate<Shared.Todo> predicate)
		{
			var result = _todos
				.Where(t => predicate(t))
				.ToList();
			return Task.FromResult(result);
		}
	}
}
