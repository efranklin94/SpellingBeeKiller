using DomainModels.Models;
using DomainModels.Models.Game;
using DomainServices.Contracts;
using DomainServices.Contracts.UserServices;
using DomainServices.Implementations;
using DomainServices.Implementations.UserServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RedisTools;
using Repositories;
using Repositories.Contracts;
using Repositories.Implementations;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Filters;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IDatabaseContext, DatabaseContext>();
builder.Services.AddSingleton<IRedisConnection, RedisConnection>();
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost"));

builder.Services.AddScoped<ClassicModeRedisRepository>();
builder.Services.AddScoped<CoreBeeGameRedisRepository>();
builder.Services.AddScoped<GameHistoryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// User
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<GameService>();

builder.Services.AddScoped<LoadService>();
builder.Services.AddScoped<MigrationService>();
builder.Services.AddScoped<ResponseFactory>();

builder.Services.AddSignalR();

// Necessary for messagePack serialization of objects of objects in DTOs problem
builder.Services.AddControllersWithViews().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ContractResolver = null;
});

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GameServer", Version = "v1" });
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.OperationFilter<SecurityRequirementsOperationFilter>();
});
// JWT
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII
            .GetBytes(builder.Configuration.GetSection("ProjectSettings")["AuthSecert"]!)),
        ValidIssuer = "MainServer",
        ValidAudience = "EternalBits", // TODO
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true
    };
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<GameHub>("/gamehub");

app.MapPost("/api/test/send-start", async (IHubContext<GameHub> hubContext) =>
{
    var userId = "test-user-123";
    var payload = "game-start!";

    if (GameHub.TryGetConnectionId(userId, out var connectionId))
    {
        await hubContext.Clients.Client(connectionId).SendAsync("GameStart", payload);
        return Results.Ok(new { Sent = "GameStart", To = userId, Payload = payload });
    }

    return Results.NotFound($"No active connection found for user {userId}");
});
app.Run();
