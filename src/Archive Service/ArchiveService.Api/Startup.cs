using ArchiveService.Dataverse.Api;
using ArchiveService.Shared.Managers.AlterTable;
using ArchiveService.Shared.Managers.CreateTable;
using ArchiveService.Shared.Managers.DailyArchive;
using ArchiveService.Shared.Managers.TableDailyArchive;
using ArchiveService.Shared.Models;
using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(Startup))]

namespace ArchiveService.Dataverse.Api
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
            builder.Services.AddTransient<IAlterTableManager, AlterTableProcessor>();
            builder.Services.AddTransient<ICreateTableManager, CreateTableProcessor>();
            builder.Services.AddTransient<IDailyArchiveManager, DailyArchiveManager>();
            builder.Services.AddTransient<ITableDailyArchiveManager, TableDailyArchiveManager>();
            builder.Services.AddMemoryCache();
            builder.Services.AddSingleton<IOrganizationService, ServiceClient>(provider =>
            {
                var managedIdentity = provider.GetRequiredService<DefaultAzureCredential>();
                var environment = dataverseurl;
                var cache = provider.GetService<IMemoryCache>();
                return new ServiceClient(
                        tokenProviderFunction: f => GetDataverseToken(environment, managedIdentity, cache),
                        instanceUrl: new Uri(environment),
                        useUniqueInstance: true);
            });

        }

        private async Task<string> GetDataverseToken(string environment, DefaultAzureCredential credential, IMemoryCache cache)
        {
            var accessToken = await cache.GetOrCreateAsync(environment, async (cacheEntry) => {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20);
                var token = (await credential.GetTokenAsync(new TokenRequestContext(new[] { $"{environment}/.default" })));
                return token;
            });
            return accessToken.Token;
        }
    }
}
