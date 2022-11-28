namespace PRDC2022.CustomerApi.Options
{
    public class CosmosDbOptions
    {
        public const string Position = "Cosmos";
        public string DatabaseName { get; set; }
        public string ContainerName { get; set; }
        public string Account { get; set; }
        public string Key { get; set; }
    }
}