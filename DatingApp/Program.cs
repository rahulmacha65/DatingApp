using DatingApp.Data;
using DatingApp.Extensions;
using DatingApp.Interfaces;
using DatingApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Extension Method to handle DB , CORS, JWT 
builder.Services.AddApplicationServices(builder.Configuration);

//Extension Method to check user authorization
builder.Services.AddIdentityService(builder.Configuration);

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); 
}

//It is to redirect http call to https
app.UseHttpsRedirection();

//Middleware which checks yhe authrization.
app.UseAuthorization();

//Enabling Cors
app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200"));

//Middleware to authorize user
app.UseAuthentication();
app.UseAuthorization();

//Middleware which decides which controller method we need to go
app.MapControllers();

//Runs the application.
app.Run();
