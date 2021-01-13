namespace CoreSharp.Cqrs.Mvc
{
    public class CqrsConfiguration
    {

        public CqrsControllerConfiguration Controllers { get; set; } = new CqrsControllerConfiguration();

        public bool RegisteredOnly { get; set; } = false;

        public string[] Assemblies { get; set; }

        public string[] ExposeList { get; set; }

    }
}
