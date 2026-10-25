using GrpcServer.Services;
using GrpcServer.Services.Commands;
using GrpcServer.Services.Event;
using GrpcServer.Services.Streams;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<EventSystemService>();
app.MapGrpcService<BesStreamService>();
app.MapGrpcService<PCStreamService>();
app.MapGrpcService<SmartMeterStreamService>();
app.MapGrpcService<SolarStreamService>();
app.MapGrpcService<SystemStreamService>();

//Commands
app.MapGrpcService<SystemPowerCommandService>();
app.MapGrpcService<SystemSetupCommandService>();
app.MapGrpcService<QuerySystemInfoCommandService>();

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
