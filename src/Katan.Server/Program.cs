using Katan.Server.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddSingleton<GameRepository>();

var app = builder.Build();

app.MapGrpcService<GameServiceImpl>();
app.MapGet("/", () => "Katan gRPC server. Use a gRPC client to connect.");

app.Run();
