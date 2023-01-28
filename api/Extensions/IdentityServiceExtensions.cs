using System.Text;
using api.Data;
using api.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace api.Extensions
{
    // in this class we are using Identity for password settings and roles management
    public static class IdentityServiceExtensions
    {
        public static IServiceCollection AddIdentityServices(
            this IServiceCollection services,
            IConfiguration config
        )
        {
            services.AddIdentityCore<AppUser>(options => 
            {
                // by default, Identity requires that passwords contain an uppercase character, 
                // lowercase character, a digit, and a non-alphanumeric character. 
                // Passwords must be at least six characters long.

                // here we are changing all default settings
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 5;
            })
                .AddRoles<AppRole>()
                .AddRoleManager<RoleManager<AppRole>>()
                .AddEntityFrameworkStores<DataContext>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(config["TokenKey"])),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };

                    // we add a new option for how are we going to authenticate inside SignalR
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            // because we cannot pass the bearer token as a HTTP header,
                            // we need to pass this up as a query string

                            // here we get the bearer token from the query string,
                            // and this is what SignalR from the client side is going to use 
                            // when it sends up the token
                            var accessToken = context.Request.Query["access_token"];

                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                            {
                                // this gives our hub the access to our bearer token
                                // because we're adding it to the context
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization(options => 
            {
                // "RequireAdminRole" and "ModeratePhotoRole" comes from AdminController class
                options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
                // this policy means that the user must have Admin or Moderator role
                options.AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("Admin", "Moderator"));
            });

            return services;
        }
        
    }
}