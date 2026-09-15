using Microsoft.Data.Sqlite;
using ShopingCart.Api;
using ShopingCart.BusinessLogic;
using ShopingCart.Repository;

var builder = WebApplication.CreateBuilder(args);
 builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

var databasePath = builder.Configuration["Database:Path"] ?? "App_Data/shopingcart.db";
databasePath = Path.GetFullPath(databasePath, builder.Environment.ContentRootPath);
var connectionString = new SqliteConnectionStringBuilder
{
    DataSource = databasePath, ForeignKeys = true, DefaultTimeout = 15
}.ToString();

builder.Services.AddScoped(_ => new DbSession(connectionString));
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<CartRepository>();
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<CheckoutService>();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? ["http://localhost:3000", "http://127.0.0.1:3000"])
        .AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
DatabaseInitializer.Initialize(connectionString);
app.UseExceptionHandler();
app.UseCors();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();
app.Run();

public partial class Program { }
