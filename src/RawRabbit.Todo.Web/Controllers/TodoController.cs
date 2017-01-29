using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using RawRabbit.Todo.Shared.Messages;
using RawRabbit.Todo.Shared.Repo;

namespace RawRabbit.Todo.Web.Controllers
{
	public class TodoController : BaseController
	{

		public TodoController(IBusClient busClient) : base(busClient)
		{
		}

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

		[HttpPost]
		[Route("api/todos")]
		public async Task<IActionResult> CreateTodo(Shared.Todo todo)
		{
			await PublishAsync(new CreateTodo {Todo = todo});
			return Ok(new {success = true});
		}
	}
}
