using System;
using Microsoft.Extensions.DependencyInjection;
using ZonyLrcTools.Cli.Commands;
using ZonyLrcTools.Cli.Infrastructure.DependencyInject;
using ZonyLrcTools.Cli.Infrastructure.Extensions;

namespace ZonyLrcTools.Tests
{
    public class TestBase
    {
        protected IServiceProvider ServiceProvider { get; private set; }
        protected IServiceCollection ServiceCollection { get; private set; }

        public TestBase()
        {
            ServiceCollection = BuildService();
            BuildServiceProvider();
        }

        protected virtual IServiceCollection BuildService()
        {
            var service = new ServiceCollection();

            service.BeginAutoDependencyInject<ToolCommand>();
            service.ConfigureToolService();
            service.ConfigureConfiguration();

            return service;
        }

        protected virtual void BuildServiceProvider() => ServiceProvider = ServiceCollection.BuildServiceProvider();
    }
}