namespace PRDC2022.CustomerApi.Options;

public class DatabaseOptions
{
    public const string Position = "Database";
    public ConnectionStrings ConnectionStrings { get; set; }
}

public class ConnectionStrings
{
    public string EventsContext { get; set; }
    public string StagingContext { get; set; }
}
