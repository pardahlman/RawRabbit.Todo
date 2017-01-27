using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RawRabbit.Todo.Shared.Repo
{
	public interface ITodoRepository
	{
		Task<List<Todo>> GetAllAsync();
		Task<List<Todo>> QueryAsync(Predicate<Todo> predicate);
		Task<Todo> GetAsync(int id);
		Task<Todo> AddAsync(Todo todo);
	}
}