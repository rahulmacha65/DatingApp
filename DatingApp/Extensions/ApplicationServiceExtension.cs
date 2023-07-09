using DatingApp.Data;
using DatingApp.HelperClasses;
using DatingApp.Interfaces;
using DatingApp.Services;
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
            services.AddScoped<IUserRepository, UserRepository>();
            //AutoMapper to convert DTO to entity and vice versa
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            //Cloudinary settings service
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
            //service to add and delete photos in coudinary
            services.AddScoped<IPhotoService, PhotoService>();
            return services;
        }
    }
}
