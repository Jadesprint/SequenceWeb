using Sequence.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<RoomManager>();
builder.Services.AddSignalR();

const string ClientCorsPolicy = "ClientCorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(ClientCorsPolicy, policy =>
    {
        policy
            .WithOrigins("http://localhost:5216", "https://localhost:7153")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors(ClientCorsPolicy);

app.MapGet("/", () => "Sequence game server is running.");
app.MapHub<GameHub>("/hubs/game");

app.Run();
