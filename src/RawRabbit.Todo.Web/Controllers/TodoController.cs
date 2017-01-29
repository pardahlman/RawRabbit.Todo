using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RawRabbit.Operations.MessageSequence;
using RawRabbit.Todo.Shared;
using RawRabbit.Todo.Shared.Messages;

namespace RawRabbit.Todo.Web.Controllers
{
	public class TodoController : BaseController
	{
		public TodoController(IBusClient busClient) : base(busClient)
		{ }

		[HttpGet]
		[Route("api/todos/")]
		public async Task<IActionResult> GetAllTodos()
		{
			await PublishAsync(new CreateTodoList {Count = int.MaxValue});
			return Ok(new {success = true});
		}

		[HttpGet]
		[Route("api/todos/{id}")]
		public async Task<IActionResult> GetTodo(int id)
		{
			var todo = await BusClient.RequestAsync<TodoRequest, TodoResponse>(new TodoRequest {Id = id});
			if (todo == null)
			{
				return NotFound();
			}
			return Ok(todo);
		}

		[HttpDelete]
		[Route("api/todos/{id}")]
		public async Task<IActionResult> RemoveTodo(int id)
		{
			var msg = string.Empty;
			var removeSequence = BusClient.ExecuteSequence<TodoContext, TodoRemoved>(s => s
				.PublishAsync(new RemoveTodo { Id = id})
				.When<RemoveTodoFailed>((failed, ctx) =>
				{
					msg = failed.Message;
					return Task.CompletedTask;
				}, it => it.AbortsExecution())
				.Complete<TodoRemoved>()
			);
			await removeSequence.Task;
			if (removeSequence.Aborted)
			{
				return BadRequest(msg);
			}
			return Ok();
		}

		[HttpPost]
		[Route("api/todos")]
		public async Task<IActionResult> CreateTodo(Shared.Todo todo)
		{
			await PublishAsync(new CreateTodo {Todo = todo});
			return Ok(new {success = true});
		}
	}
}
