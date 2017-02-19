using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RawRabbit.Operations.MessageSequence;
using RawRabbit.Todo.Shared;
using RawRabbit.Todo.Shared.Messages;

namespace RawRabbit.Todo.Web.Controllers
{
	public class TodoController : Controller
	{
		private readonly IBusClient _busClient;

		public TodoController(IBusClient busClient)
		{
			_busClient = busClient;
		}

		[HttpGet]
		[Route("api/todos/")]
		public async Task<IActionResult> GetAllTodos()
		{
			await _busClient.PublishAsync(new CreateTodoList {Count = int.MaxValue});
			return Ok(new {success = true});
		}

		[HttpGet]
		[Route("api/todos/{id}")]
		public async Task<IActionResult> GetTodo(int id)
		{
			var todo = await _busClient.RequestAsync<TodoRequest, TodoResponse>(new TodoRequest {Id = id});
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
			var removeSequence = _busClient.ExecuteSequence(s => s
				.PublishAsync(new RemoveTodo { Id = id})
				.When<RemoveTodoFailed, TodoContext>((failed, ctx) =>
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
			await _busClient.PublishAsync(new CreateTodo {Todo = todo});
			return Ok(new {success = true});
		}
	}
}
