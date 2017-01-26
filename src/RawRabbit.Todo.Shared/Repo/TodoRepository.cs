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
			_todos = new ConcurrentDictionary<int, Todo>();
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

		public Task<Todo> CreateAsync(Todo todo)
		{
			var id = Interlocked.Increment(ref _count);
			todo.Id = id;
			_todos.TryAdd(id, todo);
			return Task.FromResult(todo);
		}
	}
}
