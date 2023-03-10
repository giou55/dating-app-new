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


// we add this code for deploying our app to fly.io
// and remove some code inside ApplicationServiceExtension.cs
var connString = "";

if (builder.Environment.IsDevelopment())
    connString = builder.Configuration.GetConnectionString("DefaultConnection");
else
{
// Use connection string provided at runtime by Fly.io.
    var connUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

    // Parse connection URL to connection string for Npgsql
    connUrl = connUrl.Replace("postgres://", string.Empty);
    var pgUserPass = connUrl.Split("@")[0];
    var pgHostPortDb = connUrl.Split("@")[1];
    var pgHostPort = pgHostPortDb.Split("/")[0];
    var pgDb = pgHostPortDb.Split("/")[1];
    var pgUser = pgUserPass.Split(":")[0];
    var pgPass = pgUserPass.Split(":")[1];
    var pgHost = pgHostPort.Split(":")[0];
    var pgPort = pgHostPort.Split(":")[1];

    connString = $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};";
}

builder.Services.AddDbContext<DataContext>(opt =>
{
    opt.UseNpgsql(connString);
});
// end of code for deploy to fly.io


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

// when we say default files, it means that is going to look for the index.html from WWW root folder
app.UseDefaultFiles();

// when we say static files, it means that is going to look a WWW root folder and
// serve the content from inside there
app.UseStaticFiles();

app.MapControllers();

// we create the endpoints for SignalR, so client can find our hubs
app.MapHub<PresenceHub>("hubs/presence");
app.MapHub<MessageHub>("hubs/message");

// we specify the action and the fallback controller
// when our API doesn't know how to handle a specific route
app.MapFallbackToController("Index", "Fallback");

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

try
{
    var context = services.GetRequiredService<DataContext>();

    // we add these two lines because we are using Identity for authentication and roles
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();

    await context.Database.MigrateAsync();

    // everytime our application starts or restarts, this removes all of connections from our database,
    // this is a remove operation and it is good for small scale,
    // but if we have thousands of rows in database, that could cause a problem
    // context.Connections.RemoveRange(context.Connections);

    // an alternative approach to previous operation is to literally create a SQL query
    // that's going to truncate the Connections table when application starts or restarts,
    // and this a SQL query without using Entity framework
    // await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [Connections]"); // doesn't work for SQLite
    //await context.Database.ExecuteSqlRawAsync("DELETE FROM [Connections]"); // we use this for SQLite
    //await context.Database.ExecuteSqlRawAsync("DELETE FROM \"Connections\""); // we use this for Postgres

    // finally we use this approach to remove all of connections from our database,
    // using a static method from Seed.cs
    await Seed.ClearConnections(context);

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
