namespace Shuttle.Core.Data.Logging;

public class DataAccessLoggingOptions
{
    public bool DatabaseContext { get; set; } = true;
    public bool DbCommandFactory { get; set; } = true;
}