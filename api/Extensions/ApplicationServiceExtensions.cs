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
            services.AddDbContext<DataContext>(options => 
            {
                options.UseSqlite(config.GetConnectionString("DefaultConnection"));
            });

            services.AddCors();
            services.AddScoped<ITokenService, TokenService>();
            
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // "CloudinarySettings" is a string key taken from appsetting.json 
            // <CloudinarySettings> is from Helpers.CloudinarySettings.cs file
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));

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