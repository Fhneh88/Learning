var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Allow all origins for testing
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");

// ── Routes ──────────────────────────────────────────────

// GET /
app.MapGet("/", () => new {
    message = "👋 Hello from .NET on Google Colab!",
    version = "1.0",
    time    = DateTime.UtcNow
});

// GET /health
app.MapGet("/health", () => new {
    status = "healthy",
    uptime = Environment.TickCount64 / 1000,
    time   = DateTime.UtcNow
});

// GET /greet/{name}
app.MapGet("/greet/{name}", (string name) => new {
    message = $"Hello, {name}! 👋",
    time    = DateTime.UtcNow
});

// GET /todos
var todos = new List<Todo>
{
    new(1, "Buy groceries",   false),
    new(2, "Read a book",     true),
    new(3, "Go for a walk",   false),
};

app.MapGet("/todos", () => todos);

app.MapGet("/todos/{id:int}", (int id) =>
{
    var todo = todos.FirstOrDefault(t => t.Id == id);
    return todo is not null ? Results.Ok(todo) : Results.NotFound(new { error = $"Todo {id} not found" });
});

app.MapPost("/todos", (TodoRequest req) =>
{
    var todo = new Todo(todos.Max(t => t.Id) + 1, req.Title, false);
    todos.Add(todo);
    return Results.Created($"/todos/{todo.Id}", todo);
});

app.MapPut("/todos/{id:int}", (int id, TodoRequest req) =>
{
    var todo = todos.FirstOrDefault(t => t.Id == id);
    if (todo is null) return Results.NotFound(new { error = $"Todo {id} not found" });
    todos.Remove(todo);
    var updated = todo with { Title = req.Title, Done = req.Done };
    todos.Add(updated);
    return Results.Ok(updated);
});

app.MapDelete("/todos/{id:int}", (int id) =>
{
    var todo = todos.FirstOrDefault(t => t.Id == id);
    if (todo is null) return Results.NotFound(new { error = $"Todo {id} not found" });
    todos.Remove(todo);
    return Results.Ok(new { message = $"Todo {id} deleted" });
});

// GET /calc?a=5&b=3&op=add
app.MapGet("/calc", (double a, double b, string op) =>
{
    var result = op.ToLower() switch
    {
        "add"      => a + b,
        "subtract" => a - b,
        "multiply" => a * b,
        "divide"   => b != 0 ? a / b : double.NaN,
        _          => double.NaN
    };
    return new { a, b, op, result };
});

app.Run("http://0.0.0.0:5000");

// ── Models ───────────────────────────────────────────────
record Todo(int Id, string Title, bool Done);
record TodoRequest(string Title, bool Done = false);
