using System.Reflection;

namespace Zony.Lib.Infrastructures.Dependency
{
    public interface IConventionalRegistrationContext
    {
        Assembly Assembly { get; }

        IIocManager IocManager { get; }

        ConventionalRegistrationConfig Config { get; }
    }
}