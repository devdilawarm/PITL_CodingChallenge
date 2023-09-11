using CsvHelper;
using Microsoft.Extensions.Options;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PowerTradeReportService : IPowerTradeReportService
{
    private readonly ILogger<PowerPositionWorker> _logger;
    private readonly IOptions<AppSettings> _appSettings;
    private readonly IPowerService _powerService;
    public PowerTradeReportService(
    ILogger<PowerPositionWorker> logger,
    IOptions<AppSettings> appSettings,
    IPowerService powerService)
    {
        _logger = logger;
        _appSettings = appSettings;
        _powerService = powerService;
    }

    public async Task GenerateTradeReport(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Calculate the local date for London time zone (previous day at 23:00) //DateTime currentDate = DateTime.Now.Date; // Get the current date                
                DateTime currentDate = CalculateLondonLocalDate();

                // Retrieve power trades from the PowerService
                IEnumerable<PowerTrade> powerTrades = _powerService.GetTrades(currentDate);

                // Aggregate volumes per hour
                Dictionary<int, int> aggregatedVolumes = AggregateVolumesPerHour(powerTrades);

                //Log on console 
                LogExpectedOutput(aggregatedVolumes);

                // Save the aggregated volumes to a CSV file
                SaveAggregatedVolumesToCsv(aggregatedVolumes, currentDate);

                // Schedule subsequent executions based on the specified interval
                var intervalMinutes = _appSettings.Value.IntervalMinutes;
                await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), cancellationToken);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing power trades.");
            }
        }
    }
    private DateTime CalculateLondonLocalDate()
    {
        TimeZoneInfo londonTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        DateTime londonNow = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, londonTimeZone);

        // Calculate the local date for London time zone (previous day at 23:00)
        DateTime londonLocalDate = londonNow.Date.AddHours(-1);

        return londonLocalDate;
    }
    private Dictionary<int, int> AggregateVolumesPerHour(IEnumerable<PowerTrade> powerTrades)
    {
        Dictionary<int, int> aggregatedVolumes = new Dictionary<int, int>();

        foreach (var trade in powerTrades)

        {
            foreach (var period in trade.Periods)
            {
                // Calculate the hour based on the period number (assuming 1-based period numbering)
                int hour = period.Period - 1;

                // If the hour key doesn't exist in the dictionary, add it with the volume
                if (!aggregatedVolumes.ContainsKey(hour))
                {
                    aggregatedVolumes[hour] = 0;
                }

                // Add the period's volume to the existing volume for that hour
                aggregatedVolumes[hour] += (int)period.Volume;

            }
        }

        return aggregatedVolumes;
    }
    private void SaveAggregatedVolumesToCsv(Dictionary<int, int> aggregatedVolumes, DateTime currentDate)
    {

        // Calculate the local start time of the day (23:00 on the previous day) in London time zone
        TimeZoneInfo londonTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        DateTime localStartTime = TimeZoneInfo.ConvertTime(new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 23, 0, 0), londonTimeZone, TimeZoneInfo.Local);

        // Format the timestamp as YYYYMMDD_HHMM
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");

        // Construct the CSV file name using the timestamp // Use this if you want to generate file per internal from app setting 
        string csvFileName = $"PowerPosition_{timestamp}.csv";

        // Create a CSV file path based on the current date || Use this if you want to generate same file per day
        // string csvFileName = $"PowerPosition_{localStartTime:yyyyMMdd_HHmm}.csv";

        string csvFilePath = Path.Combine(_appSettings.Value.OutputDirectory, csvFileName);

        // Prepare data for CSV
        var records = new List<PowerPositionCsvRecord>();
        for (int hour = 0; hour < 24; hour++)
        {
            int volume = aggregatedVolumes.ContainsKey(hour) ? aggregatedVolumes[hour] : 0;
            records.Add(new PowerPositionCsvRecord
            {
                LocalTime = localStartTime.AddHours(hour).ToString("HH:mm"),
                Volume = volume
            });
        }

        using (var writer = new StreamWriter(csvFilePath))
        using (var csv = new CsvWriter(writer, new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture) { HasHeaderRecord = true }))
        {
            csv.WriteRecords(records);
        }

        _logger.LogInformation($"CSV file saved: {csvFilePath}");
    }
    private void LogExpectedOutput(Dictionary<int, int> aggregatedVolumes)
    {
        _logger.LogInformation("Local Time Volume");

        for (int hour = 0; hour < 24; hour++) // Iterate through all 24 hours
        {
            int volume = aggregatedVolumes.ContainsKey(hour) ? aggregatedVolumes[hour] : 0;
            _logger.LogInformation($"{hour:00}:00 {volume}");
        }
    }


}

