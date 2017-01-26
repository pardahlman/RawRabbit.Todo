using System.Collections.Generic;
using System.Threading.Tasks;

namespace RawRabbit.Todo.Shared.Repo
{
	public interface ITodoRepository
	{
		Task<List<Todo>> GetAllAsync();
		Task<Todo> GetAsync(int id);
		Task<Todo> CreateAsync(Todo todo);
	}
}