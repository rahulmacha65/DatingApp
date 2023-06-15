using DatingApp.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//To use it as dependency injection
builder.Services.AddDbContext<DataContext>(option =>
{
    option.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});


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
//Middleware which decides which controller method we need to go
app.MapControllers();
//Runs the application.
app.Run();
