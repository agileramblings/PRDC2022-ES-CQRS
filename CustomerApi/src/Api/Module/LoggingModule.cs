using Autofac;
using AutofacSerilogIntegration;

namespace PRDC2022.CustomerApi.Module
{
    public class LoggingModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterLogger();
            builder.RegisterGeneric(typeof(ILogger<>))
                .As(typeof(ILogger<>));
        }
    }
}
