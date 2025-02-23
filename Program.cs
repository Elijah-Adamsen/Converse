using Converse.Data;
using Converse.Hubs;
using Converse.Events;
using Converse.Services.Chat;
using Converse.Services.User;
using Converse.Services.Group;
using Converse.Services.Message;
using MongoDB.Driver;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container
builder.Services.AddControllers(); // Add controllers for API
builder.Services.AddEndpointsApiExplorer(); // Enable endpoint explorer


// var baseDirectory = Directory.GetCurrentDirectory();

var hostURL = configuration.GetValue<string>("HostURL");

// Initialize MongoDB Client and Database
var mongoClient = new MongoClient(configuration.GetValue<string>("MongoDB:ConnectionString"));
var mongoDatabase = mongoClient.GetDatabase(configuration.GetValue<string>("MongoDB:DatabaseName"));

// Register MongoDB as a Singleton
builder.Services.AddSingleton<IMongoClient>(mongoClient);
builder.Services.AddSingleton(mongoDatabase);

// Register database classes with injected `IMongoDatabase`
builder.Services.AddSingleton<UserDb>();
builder.Services.AddSingleton<GroupDb>();
builder.Services.AddSingleton<MessageDb>();

builder.Services.AddSignalR();

builder.Services.AddScoped<MessageEventHandler>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<ConnectionService>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<RegistrationService>();
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<UserManagementService>();
builder.Services.AddScoped<GroupManagementService>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMyApp", policy =>
    {
        policy.WithOrigins(hostURL)
              .AllowAnyMethod() // Allow all HTTP methods (GET, POST, etc.)
              .AllowAnyHeader()
              .AllowCredentials(); // Allow all headers in requests
              //.AllowCredentials(); // Allow cookies and authentication headers
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                Console.WriteLine($"SignalR connection request to {path}");

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chathub"))
                {
                    Console.WriteLine($"Received token: {accessToken}"); // Log received token
                    context.Token = accessToken;
                }
                else
                {
                    Console.WriteLine("No token found in request.");
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var userId = context.Principal?.FindFirst(ClaimTypes.Name)?.Value;
                Console.WriteLine($"Token successfully validated for user: {userId}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();


var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowMyApp");

app.MapFallbackToFile("index.html");

// Apply authentication middleware

app.UseAuthentication();
app.UseAuthorization();


app.MapHub<ChatHub>("/chathub");

// Map controllers to handle API requests (like registration, message history, etc.)
app.MapControllers();

app.Run();