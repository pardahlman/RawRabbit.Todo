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
		private readonly List<Todo> _todos;
		private int _count;

		public TodoRepository()
		{
			_todos = new List<Todo>
			{
				new Todo
				{
					Id = 0,
					Task = "Documentation for RawRabbit 2.0",
					Owner = "pardahlman"
				},
				new Todo
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
			return Task.FromResult(_todos);
		}

		public Task<Todo> GetAsync(int id)
		{
			var todo = _todos.FirstOrDefault(t => t.Id == id);
			return Task.FromResult(todo);
		}

		public Task<Todo> RemoveAsync(int id)
		{
			var todo = _todos.FirstOrDefault(t => t.Id == id);
			if (todo != null)
			{
				_todos.Remove(todo);
			}
			return Task.FromResult(todo);
		}

		public Task<Todo> AddAsync(Todo todo)
		{
			var id = Interlocked.Increment(ref _count);
			todo.Id = id;
			_todos.Add(todo);
			return Task.FromResult(todo);
		}

		public Task<List<Todo>> QueryAsync(Predicate<Todo> predicate)
		{
			var result = _todos
				.Where(t => predicate(t))
				.ToList();
			return Task.FromResult(result);
		}
	}
}
