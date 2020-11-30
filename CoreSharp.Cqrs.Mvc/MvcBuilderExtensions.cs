using System;
using System.Linq;
using CoreSharp.Common.Extensions;
using CoreSharp.Cqrs.Resolver;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace CoreSharp.Cqrs.Mvc
{
    public static class MvcBuilderExtensions
    {
        public static IMvcBuilder AddCqrs(this IMvcBuilder mvc)
        {

            // get container 
            var container = (Container) mvc.Services.FirstOrDefault(d => d.ServiceType == typeof(Container)).ImplementationInstance;

            // get configuration
            var configuration = container.GetRegisteredInstance<IConfiguration>()?.GetSection("CqrsWeb").Get<CqrsConfiguration>() ?? new CqrsConfiguration();

            // get cqrs 
            var assemblies = configuration.Assemblies.ToAssemblies();
            var cqrs = assemblies.SelectMany(x => CqrsInfoResolverUtil.GetCqrsDefinitions(x)).ToList();

            // filter cqrs based on include requirements
            cqrs  = configuration.FilterCqrsInfoList(cqrs).ToList();
            if (configuration.RegisteredOnly)
            {
                var allTypes = container.GetCurrentRegistrations().Select(x => x.Registration.ImplementationType).SelectMany(x => x.GetInterfaces());
                cqrs = cqrs.Where(x => allTypes.Contains(x.GetHandlerType())).ToList();
            }

            // create controllers
            var builder = new ControllerBuilder(cqrs, configuration.Controllers);
            var controllersAssembly = builder.BuildCqrsControllers();

            // register metadata service
            var commands = cqrs.Where(x => x.IsCommand).ToDictionary(x => x.ReqType, x => configuration.Controllers.GetUrlPath(x));
            var queries = cqrs.Where(x => x.IsQuery).ToDictionary(x => x.ReqType, x => configuration.Controllers.GetUrlPath(x));
            container.Register<ICqrsMetaData>(() => new CqrsMetaData(configuration ?? new CqrsConfiguration(), queries, commands), Lifestyle.Singleton);

            // register controllers to runtime mvc
            mvc.PartManager.ApplicationParts.Add(new AssemblyPart(controllersAssembly));
            return mvc;
        }

    }
}
