using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;

namespace CourseLibrary.API.DbContexts
{
    public class DatabaseSetting
    {
        public string[] Urls { get; set; }
        public string DatabaseName { get; set; }
        public string CertPath { get;set; }
        public string CertPass { get;set; }

        public static (DatabaseSetting, X509Certificate2) FromConfig(IConfiguration configuration, string sectionName = null)
        {
            var dbSettings =  new DatabaseSetting();
            configuration.Bind(sectionName ?? nameof(DatabaseSetting), dbSettings);
            var certificate = !string.IsNullOrEmpty(dbSettings?.CertPath)
                ? new X509Certificate2(dbSettings.CertPath, dbSettings.CertPass)
                : null;
            return (dbSettings, certificate);
        }
    }
}