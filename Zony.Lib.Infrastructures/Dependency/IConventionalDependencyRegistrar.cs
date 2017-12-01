namespace Zony.Lib.Infrastructures.Dependency
{
    public interface IConventionalDependencyRegistrar
    {
        void RegisterAssembly(IConventionalRegistrationContext context);
    }
}
