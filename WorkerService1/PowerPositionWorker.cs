using System;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Services;
using CsvHelper;
using CsvHelper.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;


public class PowerPositionWorker : BackgroundService
{
    private readonly ILogger<PowerPositionWorker> _logger;
    private readonly IOptions<AppSettings> _appSettings;
    private readonly IPowerService _powerService;
    private readonly IPowerTradeReportService _powerTradeService;

    public PowerPositionWorker(
        ILogger<PowerPositionWorker> logger,
        IOptions<AppSettings> appSettings,
        IPowerService powerService,
        IPowerTradeReportService powerTradeService)
    {
        _logger = logger;
        _appSettings = appSettings;
        _powerService = powerService;
        _powerTradeService = powerTradeService;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        // Execute the extract when the service first starts      
        await _powerTradeService.GenerateTradeReport(cancellationToken);

    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

    }


}
