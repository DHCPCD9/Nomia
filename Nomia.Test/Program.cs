using Autofac;
using Nomia.Test;

var builder = new ContainerBuilder();

builder.RegisterType<Bot>();

var container = builder.Build();

using (var scope = container.BeginLifetimeScope())
{
    var bot = scope.Resolve<Bot>();
    await bot.Run();
            
            
    await Task.Delay(-1);
}