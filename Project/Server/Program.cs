using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Server.Controllers;
using Server.Data;
using System.Text;
namespace Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<DataContext>(oprions => oprions.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddControllers();
        builder.Services.AddSignalR();

        var jwtKey = builder.Configuration["Jwt:Secret"] ??
    throw new ArgumentNullException("Jwt:Key", "JWT Key is not configured");
        var jwtIssuer = builder.Configuration["Jwt:Issuer"] ??
            throw new ArgumentNullException("Jwt:Issuer", "JWT Issuer is not configured");
        var jwtAudience = builder.Configuration["Jwt:Audience"] ??
            throw new ArgumentNullException("Jwt:Audience", "JWT Audience is not configured");

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });

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
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.MapControllers();

        app.MapHub<ChatHub>("/chat");

        app.Run();
    }
}
