using DiscordClientProxy;
using Environment = System.Environment;

//set working dir to environment specified base directory
if (!Directory.Exists(DiscordClientProxy.Environment.BaseDir))
    Directory.CreateDirectory(DiscordClientProxy.Environment.BaseDir);

Environment.CurrentDirectory = DiscordClientProxy.Environment.BaseDir;
Configuration.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
/*if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}*/

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();