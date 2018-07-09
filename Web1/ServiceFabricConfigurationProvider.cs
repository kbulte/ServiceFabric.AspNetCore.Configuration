using Microsoft.Extensions.Configuration;
using System.Fabric;

namespace Web1
{
    public class ServiceFabricConfigurationProvider : ConfigurationProvider
    {
        private readonly string _packageName;
        private readonly CodePackageActivationContext _context;

        public ServiceFabricConfigurationProvider(string packageName)
        {
            _packageName = packageName;
            _context = FabricRuntime.GetActivationContext();

            //It's possible to upgrade a service's configuration package without changing the code package. In that case, Service Fabric does not restart the service. Instead, the service receives a ConfigurationPackageModifiedEvent event to notify it that the package changed.
            _context.ConfigurationPackageModifiedEvent += (sender, e) =>
            {
                this.LoadPackage(e.NewPackage, reload: true);
                this.OnReload();
            };
        }

        public override void Load()
        {
            var config = _context.GetConfigurationPackageObject(_packageName);
            LoadPackage(config);
        }

        private void LoadPackage(ConfigurationPackage config, bool reload = false)
        {
            if (reload)
            {
                Data.Clear();
            }
            foreach (var section in config.Settings.Sections)
            {
                foreach (var param in section.Parameters)
                {
                    Data[$"{section.Name}:{param.Name}"] = param.IsEncrypted ? param.DecryptValue().ToUnsecureString() : param.Value;
                }
            }
        }
    }
}
