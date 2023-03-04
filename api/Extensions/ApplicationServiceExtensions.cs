using api.Data;
using api.Helpers;
using api.Interfaces;
using api.Services;
using api.SignalR;
using Microsoft.EntityFrameworkCore;

namespace api.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices
        (
            this IServiceCollection services,
            IConfiguration config
        )
        {
            // when we're going to deploy in fly.io
            // we must remove these lines,
            // and add some code inside Program.cs
            services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlite(config.GetConnectionString("DefaultConnection"));
                //options.UseNpgsql(config.GetConnectionString("DefaultConnection"));
            });

            services.AddCors();
            services.AddScoped<ITokenService, TokenService>();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // "CloudinarySettings" is a string key taken from appsetting.json
            // <CloudinarySettings> is from Helpers.CloudinarySettings.cs file
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));

            // that is saying whenever a IPhotoService is required,
            // create a PhotoService and pass that in
            services.AddScoped<IPhotoService, PhotoService>();

            // this is a action filter class
            services.AddScoped<LogUserActivity>();

            // we're going to inject the UnitOfWork instead of these
            // services.AddScoped<ILikesRepository, LikesRepository>();
            // services.AddScoped<IMessageRepository, MessageRepository>();
            // services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddSignalR();

            // we want the dictionary of OnlineUsers to be available application wide
            // for every user that connects to our service
            services.AddSingleton<PresenceTracker>();

            return services;
        }
    }
}