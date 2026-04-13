using robot_controller_api.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Dependency Injection registrations
builder.Services.AddScoped<IRobotCommandDataAccess, RobotCommandEF>();
builder.Services.AddScoped<IMapDataAccess, MapEF>();
builder.Services.AddScoped<RobotContext>();


var app = builder.Build();

// Configure middleware
app.UseHttpsRedirection();

// Enable controller routes
app.MapControllers();

app.Run();