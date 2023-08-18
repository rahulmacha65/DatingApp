using DatingApp.Data;
using DatingApp.HelperClasses;
using DatingApp.Interfaces;
using DatingApp.Services;
using DatingApp.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Extensions
{
    public static class ApplicationServiceExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services,IConfiguration config)
        {
            //To use DB context as dependency injection
            services.AddDbContext<DataContext>(option =>
            {
                option.UseSqlite(config.GetConnectionString("DefaultConnection"));
            });
            services.AddCors();


            //adding service for creating JWT token
            services.AddScoped<ITokenSevice, TokenService>();

            // Created Repository to abstract entity framework.
            //services.AddScoped<IUserRepository, UserRepository>();

            //AutoMapper to convert DTO to entity and vice versa
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            //Cloudinary settings service
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
            //service to add and delete photos in coudinary
            services.AddScoped<IPhotoService, PhotoService>();
            //service to update last active status of User
            services.AddScoped<LogUserActivity>();

            //service for likes Many to Many relation
            //user likes many user he also like by many user
            //services.AddScoped<ILikesRepository, LikesRepository>();

            //service to handle message feature
            //services.AddScoped<IMessageRepository, MessageRepository>();
            
            //commented few Repositories because they are implemented in
            //Unit of work interface and we adding unit of work service directly.
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            //SignalR to provide real-time data to web apps.
            services.AddSignalR();
            //singleton makes that service is available for applications wide
            services.AddSingleton<PresenceTracker>();
            return services;
        }
    }
}
