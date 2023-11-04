using ArchiveService.Shared.Models;
using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveService.Shared.Managers
{
    public abstract class ManagerBase
    {
        private ServiceClient _serviceClient;
        protected readonly IConfiguration _configuration;

        protected readonly SqlSettings _sqlSettings;
        protected readonly ArchiveServiceSettings _archiveServiceSettings = new ArchiveServiceSettings();
        protected readonly IMemoryCache _memoryCache;

        public ManagerBase(IOptions<SqlSettings> sqlSettings, IConfiguration config, IMemoryCache memoryCache)
        {

            _configuration = config;
            _sqlSettings = sqlSettings.Value;
            _memoryCache = memoryCache;
            GetArchiveServiceSettings();
        }

        private void GetArchiveServiceSettings()
        {
            using (var conn = GetSqlConnection())
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"select 
HistoricalArchive,
ChangeLogEnabled,
ArchiveEnabled,
DataverseUrl,
SchemaName,
ServiceEndPointId
from ArchiveServiceSettings";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _archiveServiceSettings.HistoricalArchive = reader.IsDBNull("HistoricalArchive") ? true : reader.GetFieldValue<bool>("HistoricalArchive");
                            _archiveServiceSettings.ChangeLogEnabled = reader.IsDBNull("ChangeLogEnabled") ? false : reader.GetFieldValue<bool>("ChangeLogEnabled");
                            _archiveServiceSettings.DataverseUrl = reader.IsDBNull("DataverseUrl") ? string.Empty : reader.GetFieldValue<string>("DataverseUrl");
                            _archiveServiceSettings.SchemaName = reader.IsDBNull("SchemaName") ? "dv" : reader.GetFieldValue<string>("SchemaName");
                            _archiveServiceSettings.ServiceEndPointId = reader.IsDBNull("ServiceEndPointId") ? null : reader.GetFieldValue<Guid>("ServiceEndPointId");
                        }
                    }
                }
            }
        }

        protected SqlConnection GetSqlConnection()
        {
            SqlConnection connection = new SqlConnection(_sqlSettings.SqlConnectionString);
            
            connection.Open();
            return connection;
        }

        protected ServiceClient GetServiceClient(ILogger logger)
        {
            if (_serviceClient == null)
            {
                var managedIdentity = new DefaultAzureCredential();
                var environment = _configuration.GetValue<string>("dataverseUrl");
                logger.LogInformation($"DVEnvironment: {environment}");
                var cache = _memoryCache;
                _serviceClient = new ServiceClient(
                        logger: logger,
                        tokenProviderFunction: f => GetDataverseToken(environment, managedIdentity, cache),
                        instanceUrl: new Uri(environment),
                        useUniqueInstance: true);
                return _serviceClient;
            }
            else
            {
                return _serviceClient;
            }

        }


        private async Task<string> GetDataverseToken(string environment, DefaultAzureCredential credential, IMemoryCache cache)
        {
            var accessToken = await cache.GetOrCreateAsync(environment, async (cacheEntry) =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20);
                var token = (await credential.GetTokenAsync(new TokenRequestContext(new[] { $"{environment}/.default" })));
                return token;
            });
            return accessToken.Token;
        }
    }
}
