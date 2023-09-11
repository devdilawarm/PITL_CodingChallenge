using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services;



    class Program
    {
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Load configuration from appsettings.json
                    IConfiguration configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .Build();

                    // Configure your services here
                    services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
                    services.AddHostedService<PowerPositionWorker>(); // Add your hosted service

                    // Add your services to the DI container
                    services.AddSingleton<IPowerService, PowerService>();
                    services.AddSingleton<IPowerTradeReportService, PowerTradeReportService>();


                });


    //public void ConfigureServices(IServiceCollection services)
    //{
    //    // Other services registration

    //    services.AddSingleton<IPowerService, PowerService>(); // Register IPowerService
    //}

}

