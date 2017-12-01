using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System.Reflection;
using Zony.Lib.Infrastructures.Dependency;
using Zony.Lib.Infrastructures.EventBus.Factories;
using Zony.Lib.Infrastructures.EventBus.Handlers;

namespace Zony.Lib.Infrastructures.EventBus
{
    public class EventBusInstaller : IWindsorInstaller
    {
        private readonly IIocResolver m_iocResolver;
        private IEventBus m_eventBus;

        public EventBusInstaller(IIocResolver iocResolver)
        {
            m_iocResolver = iocResolver;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IEventBus>().UsingFactoryMethod(() => EventBus.Default).LifestyleSingleton());
            //container.Register(Component.For<IEventBus>().ImplementedBy<EventBus>().LifestyleSingleton());

            m_eventBus = container.Resolve<IEventBus>();
            container.Kernel.ComponentRegistered += Kernel_ComponentRegistered;
        }

        private void Kernel_ComponentRegistered(string key, IHandler handler)
        {
            if (!typeof(IEventHandler).GetTypeInfo().IsAssignableFrom(handler.ComponentModel.Implementation)) return;

            var _interfaces = handler.ComponentModel.Implementation.GetTypeInfo().GetInterfaces();
            foreach (var @interface in _interfaces)
            {
                if (!typeof(IEventHandler).GetTypeInfo().IsAssignableFrom(@interface)) continue;

                var _genericArgs = @interface.GetGenericArguments();

                if (_genericArgs.Length == 1)
                {
                    m_eventBus.Register(_genericArgs[0], new IocHandlerFactory(m_iocResolver, handler.ComponentModel.Implementation));
                }
            }
        }
    }
}
