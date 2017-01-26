using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using RawRabbit.Todo.Shared.Repo;
using RawRabbit.Todo.Web.Hubs;

namespace RawRabbit.Todo.Web.Controllers
{
	public class TodoController : Controller
	{
		private readonly ITodoRepository _todoRepo;
		private readonly IConnectionManager _connectionMgmt;

		public TodoController(ITodoRepository todoRepo, IConnectionManager connectionMgmt)
		{
			_todoRepo = todoRepo;
			_connectionMgmt = connectionMgmt;
		}

		[HttpGet]
		[Route("api/todos/")]
		public async Task<IActionResult> GetAllTodos()
		{
			var todos = await _todoRepo.GetAllAsync();
			return Ok(todos);
		}

		[HttpGet]
		[Route("api/todos/{id}")]
		public async Task<IActionResult> GetTodo(int id)
		{
			var todo = await _todoRepo.GetAsync(id);
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
			var created = await _todoRepo.CreateAsync(todo);
			_connectionMgmt.GetHubContext<TodoHub>().Clients.All.publishTodo(todo);
			return Created(new Uri($"/api/todo/{created.Id}"), todo);
		}
	}
}
