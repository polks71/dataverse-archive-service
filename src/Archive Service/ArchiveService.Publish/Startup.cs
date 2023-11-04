using ArchiveService.Shared.Models;
using Azure.Identity;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using ArchiveService.Publish;
using ArchiveService.Shared.Managers.PublishManager;
using ArchiveService.Shared.Managers.CreateTable;
using ArchiveService.Shared.Managers.AlterTable;
using ArchiveService.Shared.Managers.TableDailyArchive;
using ArchiveService.Shared.Managers.DailyArchive;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Xrm.Sdk;
using Azure.Core;
using Microsoft.PowerPlatform.Dataverse.Client;

[assembly: FunctionsStartup(typeof(Startup))]
namespace ArchiveService.Publish
{

    public class Startup : FunctionsStartup
    {
        private static string dataverseurl;
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            if (Debugger.IsAttached)
            {
                var context = builder.GetContext();
                builder.ConfigurationBuilder.AddJsonFile($"{context.ApplicationRootPath}\\local.{context.EnvironmentName}.settings.json");
            }
            var config = builder.ConfigurationBuilder.Build();
            dataverseurl = config["dataverseUrl"];
            //DefaultAzureCredential = https://learn.microsoft.com/en-us/dotnet/api/azure.identity.defaultazurecredential?view=azure-dotnet
            builder.ConfigurationBuilder.AddAzureKeyVault(new Uri(config["keyvaulturl"]), new DefaultAzureCredential());
            base.ConfigureAppConfiguration(builder);
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddOptions<SqlSettings>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection("SqlSettings").Bind(settings);
            });
            builder.Services.AddSingleton(new DefaultAzureCredential());
            builder.Services.AddTransient<IPublishManager, PublishManager>();
            builder.Services.AddTransient<ICreateTableManager, CreateTableSendToQueue>();
            builder.Services.AddTransient<IAlterTableManager, AlterTableSendToQueue>();
            builder.Services.AddTransient<IDailyArchiveManager, DailyArchiveManager>();
            builder.Services.AddTransient<ITableDailyArchiveManager, TableDailyArchiveSendToQueue>();
            builder.Services.AddMemoryCache();
            
        }

       
    }
}
