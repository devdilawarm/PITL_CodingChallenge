using System.Globalization;
using CsvHelper.Configuration;
using Services; // Import the appropriate namespace for your model classes

public class PowerTradeMap : ClassMap<PowerTrade>
{
    public PowerTradeMap()
    {
        // Map properties of PowerTrade as needed
        Map(m => m.Date).Name("Date");
        Map(m => m.Periods).Ignore(); // Ignore the collection itself
    }
}

public class PowerPeriodMap : ClassMap<PowerPeriod>
{
    public PowerPeriodMap()
    {
        // Map properties of PowerPeriod
        Map(m => m.Period).Name("Period");
        Map(m => m.Volume).Name("Volume");
    }
}
