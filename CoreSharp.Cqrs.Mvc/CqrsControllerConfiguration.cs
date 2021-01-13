namespace CoreSharp.Cqrs.Mvc
{
    public class CqrsControllerConfiguration
    {
        public string CommandsPath { get; set; } = "/api/command/";

        public string QueriesPath { get; set; } = "/api/query/";

        public bool QueryResultMode { get; set; } = true;

        public bool CopyAttributes { get; set; } = true;

        public bool Authorization { get; set; } = true;

    }
}
