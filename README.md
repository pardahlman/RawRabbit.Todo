# RawRabbit Todo

This is a small sample application that demonstrates some of the features of forthcoming release of RawRabbit 2.0.

## Installation

1. Clone the repo `git clone https://github.com/pardahlman/RawRabbit.Todo.git`
2. Restore packages `dotnet restore`
3. Run `RawRabbit.Todo.Web` and `RawRabbit.Todo` is two seperate terminals `dotnet run`
4. Surf to `localhost:5000`

## Feature Highlights

### Event driven requeset

Listing all todos as well as create a new todo is fully event driven. The controller publishes a message and returnsa `200 OK`. The front end is updated through a SignalR push.

```csharp
[HttpGet]
[Route("api/todos/")]
public async Task<IActionResult> GetAllTodos()
{
	await PublishAsync(new CreateTodoList {Count = int.MaxValue});
	return Ok(new {success = true});
}
```
Messages are directed to the right (SignalR) client but using _Message Context Forwarding_.

### Publish/Subscribe with explicit Ack/Nack

The service uses the new way to Acknowledge messages. If not defined, the messages are auto acked.

```csharp
await busClient.SubscribeAsync<CreateTodo, TodoContext>(async (msg, context) =>
{
	if (msg.Todo == null)
	{
		return new Nack(false);
	}
	var created = await repo.AddAsync(msg.Todo);
	await busClient.PublishAsync(new TodoCreated
	{
		Todo = created
	});
	return new Ack();
});
```

### Fluent configuration

Get even better control over the client. Override configuration with the fluent `IPipeContext` action

```csharp
var todo = await BusClient.RequestAsync<TodoRequest, TodoResponse>(new TodoRequest {Id = id},
	ctx => ctx.UseRequestTimeout(TimeSpan.FromSeconds(20))
);
```