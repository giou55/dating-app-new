using api.Data;
using api.Entities;
using api.Extensions;
using api.Middlewares;
using api.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddIdentityServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();

app.UseCors(policyBuilder => 
    policyBuilder
        .AllowAnyHeader()
        .AllowAnyMethod()
        // we need to add AllowCredentials, otherwise we get a problem from SignalR 
        // authenticating to the server from the client
        .AllowCredentials() 
        .WithOrigins("http://localhost:4200")
);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// we create the endpoints for SignalR, so client can find our hubs
app.MapHub<PresenceHub>("hubs/presence");
app.MapHub<MessageHub>("hubs/message");

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

try
{
    var context = services.GetRequiredService<DataContext>();

    // we add these two lines because we are using Identity for authentication and roles
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();

    await context.Database.MigrateAsync();
    
    //await Seed.SeedUsers(context);

    // we add this because we are using Identity for authentication and roles
    await Seed.SeedUsers(userManager, roleManager);
}
catch (Exception ex)
{
    var logger = services.GetService<ILogger<Program>>();
    logger.LogError(ex, "An error occured during migration");
}

app.Run();
