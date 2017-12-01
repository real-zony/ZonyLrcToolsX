namespace Zony.Lib.Infrastructures.Dependency
{
    public class ConventionalRegistrationConfig
    {
        public bool InstallInstallers { get; set; }
        public ConventionalRegistrationConfig()
        {
            InstallInstallers = false;
        }
    }
}