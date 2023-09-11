using System;
using System.ServiceProcess;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class PowerPositionService : ServiceBase
{
    private IHost _host;

    public PowerPositionService()
    {
        ServiceName = "PowerPositionService";
    }

    protected override void OnStart(string[] args)
    {
        _host = new HostBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services.Configure<AppSettings>(hostContext.Configuration.GetSection("AppSettings"));
                services.AddHostedService<PowerPositionWorker>();
            })
            .ConfigureLogging((hostContext, configLogging) =>
            {
                configLogging.AddEventLog();
            })
            .Build();

        _host.Start();
    }

    protected override void OnStop()
    {
        _host.StopAsync().Wait();
        _host.Dispose();
        _host = null;
    }
}
