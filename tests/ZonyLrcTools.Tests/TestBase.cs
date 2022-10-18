using System;
using Microsoft.Extensions.DependencyInjection;
using ZonyLrcTools.Cli;
using ZonyLrcTools.Cli.Commands;
using ZonyLrcTools.Common;
using ZonyLrcTools.Common.Infrastructure.DependencyInject;

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

            service.BeginAutoDependencyInject<Program>();
            service.BeginAutoDependencyInject<MusicInfo>();
            service.ConfigureToolService();
            service.ConfigureConfiguration();

            return service;
        }

        protected virtual void BuildServiceProvider() => ServiceProvider = ServiceCollection.BuildServiceProvider();

        protected TService GetService<TService>()
        {
            return ServiceProvider.GetRequiredService<TService>();
        }
    }
}