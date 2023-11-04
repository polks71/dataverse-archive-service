using ArchiveService.Management.App.Data.Models;
using ArchiveService.Management.App.Data.Models.Dataverse;
using Microsoft.EntityFrameworkCore;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.PowerPlatform.Dataverse.Client.Extensions;
using Microsoft.Xrm.Sdk;
using System.Collections.ObjectModel;
using System.Reflection.Metadata.Ecma335;
using System.Linq;
using ArchiveService.Management.App.Helper;
using ArchiveService.Shared.Managers.ServiceEndpointSteps;
using ArchiveService.Shared.Helpers;

namespace ArchiveService.Management.App.Data
{
    public class TablesService : ServiceBase
    {
        private readonly ILogger<TablesService> _logger;
        private readonly ServiceClient _serviceClient;
        private static SdkMessage _updateMessage;
        private static SdkMessage _createMessage;
        private static SdkMessage _deleteMessage;
        public TablesService(ArchiveServiceContext context, ILogger<TablesService> log, IOrganizationService organizationService) : base(context)
        {
            _logger = log;
            _serviceClient = (ServiceClient)organizationService;
        }

        public async Task CreateServiceEndPointSteps(ArchiveTableSetting setting)
        {
            //get messages for create, update, and delete
            //get object type code from entity
            //get sdkmessage filter for entity an Create, update, delete

        }

        public async Task<ObservableCollection<ArchiveTableSetting>> GetTablesAsync()
        {
            try
            {
                var o = new ObservableCollection<ArchiveTableSetting>();
                var l= await _context.ArchiveTableSettings.ToListAsync();
                l.ForEach(l => o.Add(l));
                return o;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in Tables Service");
                throw;
            }
        }

        public void AddOrRemoveServiceEndPointStep(ArchiveTableSetting setting)
        {
            var existing = _context.ArchiveTableSettings.Where(ats => ats.Id == setting.Id).AsNoTracking().First();
            if (!setting.ChangeLogEnabled.HasValue)
            {
                return;
            }
            if (existing.ChangeLogEnabled.HasValue && setting.ChangeLogEnabled.Value == existing.ChangeLogEnabled.Value)
            {
                return;
            }

            using (dataverse dv = new dataverse(_serviceClient))
            {
                var serviceSettings = _context.ArchiveServiceSettings.AsNoTracking().First();
                if (!serviceSettings.ServiceEndPointId.HasValue)
                {
                    throw new Exception("Service End Point must have a value to edit Change Log on tables");
                }
                ServiceEndPointStepManager endpointHelper = new ServiceEndPointStepManager(_serviceClient, _logger);
                endpointHelper.AddOrRemoveServiceEndPointStep(setting.LogicalName, serviceSettings.ServiceEndPointId.Value, setting.ChangeLogEnabled.Value);
            }

        }

       

       
       

        public bool ValidateTableHasChangeTrackingEnabled(ArchiveTableSetting setting)
        {
            try
            {
                //get the instance from the db outside EF change tracking
                var existing = _context.ArchiveTableSettings.Where(ats => ats.Id == setting.Id).AsNoTracking().First();
                //validate if ARchiveEnabled was changed before checking the metadata. ChangeLog (audit history) doesn't require 
                var existingValue = existing.ArchiveEnabled.HasValue ? existing.ArchiveEnabled.Value : false;
                var updatedValue = setting.ArchiveEnabled.HasValue ? setting.ArchiveEnabled.Value : false;
                if (updatedValue == true && existingValue != updatedValue)
                {

                    var metadata = _serviceClient.GetEntityMetadata(setting.LogicalName, Microsoft.Xrm.Sdk.Metadata.EntityFilters.Entity);
                    if (metadata == null)
                    {
                        throw new Exception($"Table {setting.LogicalName} not found");
                    }
                    var metadataValue = metadata.ChangeTrackingEnabled.HasValue ? metadata.ChangeTrackingEnabled.Value : false;
                    if (!metadataValue)
                        setting.ArchiveEnabled = false;
                    return metadataValue;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error retrieving {setting.LogicalName}");
                throw;
            }
        }

        public void SaveTableSetting(ArchiveTableSetting setting)
        {
            var existing = _context.ArchiveTableSettings.Where(ats => ats.Id == setting.Id).FirstOrDefault();
            if (existing == null)
            {
                _context.ArchiveTableSettings.Add(setting);
            }
            _context.SaveChanges();
        }
    }
}
