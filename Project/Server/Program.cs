using Microsoft.EntityFrameworkCore;
using Server.Controllers;
using Server.Data;
namespace Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<DataContext>(oprions => oprions.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddControllers();
        builder.Services.AddSignalR();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        var app = builder.Build();

        app.UseCors("AllowAll");

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.MapHub<ChatHub>("/chat");

        app.Run();
    }
}
