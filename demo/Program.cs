using demo.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyDemo.Models;
using System;
using NLog.Web;

namespace demo
{
    /// <summary>
    /// �������
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            //��������
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    //ÿ�����ж������ݿ�ɾ���ؽ���P1��
                    var context = services.GetRequiredService<DBContext>();
                    context.Database.EnsureDeleted();
                    context.Database.Migrate(); //Ǩ��
                    SeedData.Initialize(services);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred seeding the DB.");
                }
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                        //����������log��
                        .ConfigureLogging((hostingContext, builder) =>
                        {
                            builder.ClearProviders();
                            builder.SetMinimumLevel(LogLevel.Trace); //ʹ��Nlogs������
                            //builder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                            //builder.AddConsole();
                            //builder.AddDebug();
                        })
                        .UseNLog(); // NLog: setup NLog for Dependency injection. 3.0����������;
                });
    }
}