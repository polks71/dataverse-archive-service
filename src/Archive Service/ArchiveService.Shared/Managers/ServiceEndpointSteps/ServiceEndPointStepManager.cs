using ArchiveService.Dataverse;
using ArchiveService.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveService.Shared.Managers.ServiceEndpointSteps
{
    /// <summary>
    /// Helper to create and delete the message steps for tables that have change long enabled
    /// </summary>
    public class ServiceEndPointStepManager
    {
        private readonly ILogger _logger;
        private readonly ServiceClient _serviceClient;
        private static SdkMessage _updateMessage;
        private static SdkMessage _createMessage;
        private static SdkMessage _deleteMessage;
        public ServiceEndPointStepManager(ServiceClient serviceClient, ILogger logger) 
        {
            _serviceClient = serviceClient;
            _logger = logger;
        }

        public void AddOrRemoveServiceEndPointStep(string logicalName, Guid serviceEndPointId, bool changeLogEnabled)
        {
            using (dataverse dv = new dataverse(_serviceClient))
            {

                GetMessages(dv);
                _logger.LogInformation($"UpdateMessageId:{_updateMessage.Id}");
                SdkMessageFilter updateMessageFilter = GetMessageFilter(logicalName, _updateMessage, dv);
                SdkMessageFilter createMessageFilter = GetMessageFilter(logicalName, _createMessage, dv);
                SdkMessageFilter deleteMessageFilter = GetMessageFilter(logicalName, _deleteMessage, dv);



                if (!changeLogEnabled)
                {
                    _logger.LogInformation($"Removing steps for {logicalName}");
                    DeleteSdkStep(dv, updateMessageFilter, serviceEndPointId);
                    DeleteSdkStep(dv, createMessageFilter, serviceEndPointId);
                    DeleteSdkStep(dv, deleteMessageFilter, serviceEndPointId);
                }
                else
                {
                    CreateSdkStep(logicalName, dv, updateMessageFilter, serviceEndPointId, _updateMessage);
                    CreateSdkStep(logicalName, dv, createMessageFilter, serviceEndPointId, _createMessage);
                    CreateSdkStep(logicalName, dv, deleteMessageFilter, serviceEndPointId, _deleteMessage);
                }
            }
        }        

        private void CreateSdkStep(string logicalname, dataverse dv, SdkMessageFilter messageFilter, Guid serviceEndPointId, SdkMessage sdkMessage)
        {
            var existingprocessingStep = (from m in dv.SdkMessageProcessingStepSet
                                          where m.SdkMessageFilterId.Id == messageFilter.Id
                                          && m.EventHandler.Id == serviceEndPointId
                                          select m).FirstOrDefault();
            if (existingprocessingStep == null)
            {
                _logger.LogInformation($"Adding steps for {logicalname}");
                var updateprocessingStep = new SdkMessageProcessingStep();
                updateprocessingStep.Mode = sdkmessageprocessingstep_mode.Asynchronous;
                updateprocessingStep.Name = $"{logicalname} Archive Service {sdkMessage.Name} Step";
                updateprocessingStep.Configuration = "";
                updateprocessingStep.Rank = 1;
                updateprocessingStep.Stage = sdkmessageprocessingstep_stage.Postoperation;
                updateprocessingStep.SupportedDeployment = sdkmessageprocessingstep_supporteddeployment.ServerOnly;
                updateprocessingStep.InvocationSource = sdkmessageprocessingstep_invocationsource.Parent;
                updateprocessingStep.SdkMessageId = sdkMessage.ToEntityReference();
                updateprocessingStep.SdkMessageFilterId = messageFilter.ToEntityReference();
                updateprocessingStep.AsyncAutoDelete = true;
                updateprocessingStep.EventHandler = new EntityReference("serviceendpoint", serviceEndPointId);
                _serviceClient.Create(updateprocessingStep);
            }
            else
            {
                _logger.LogWarning($"Update step already exists for {logicalname}");
            }
        }

        private void DeleteSdkStep(dataverse dv, SdkMessageFilter messageFilter, Guid serviceEndPointId)
        {
            var updateSdkStep = (from m in dv.SdkMessageProcessingStepSet
                                 where m.SdkMessageFilterId.Id == messageFilter.Id
                                 && m.EventHandler.Id == serviceEndPointId
                                 select m).FirstOrDefault();
            if (updateSdkStep == null)
            {
                _logger.LogInformation("Processing Step Not Found");
            }
            else
            {
                _serviceClient.Delete(SdkMessageProcessingStep.EntityLogicalName, updateSdkStep.Id);
            }
        }


        private SdkMessageFilter GetMessageFilter(string settingLogicalName, SdkMessage sdkMessage, dataverse dv)
        {
            var messageFilter = (from m in dv.SdkMessageFilterSet
                                 where m.SdkMessageId.Id == sdkMessage.Id
                                 && m.PrimaryObjectTypeCode == settingLogicalName
                                 select m).FirstOrDefault();
            if (messageFilter == null)
            {
                _logger.LogInformation($"Message Filter Not Found for Entity {settingLogicalName} and Message {sdkMessage.Name}");
                throw new Exception($"Message Filter Not Found for Entity {settingLogicalName}  and Message {sdkMessage.Name}");
            }
            _logger.LogInformation($"{sdkMessage.Name} Message filter id: {messageFilter.Id}");
            return messageFilter;
        }

        private void GetMessages(dataverse dv)
        {
            if (_updateMessage == null)
            {
#pragma warning disable CS8601 // Possible null reference assignment.
                _updateMessage = (from m in dv.SdkMessageSet
                                  where m.CategoryName == "Update"
                                  select m).FirstOrDefault();

                if (_updateMessage == null)
                {
                    //shouldn't happen
                    throw new Exception("Update Message not found");
                }
            }
            if (_createMessage == null)
            {
                _createMessage = (from m in dv.SdkMessageSet
                                  where m.CategoryName == "Create"
                                  select m).FirstOrDefault();
                if (_createMessage == null)
                {
                    //shouldn't happen
                    throw new Exception("Create Message not found");
                }
            }
            if (_deleteMessage == null)
            {
                _deleteMessage = (from m in dv.SdkMessageSet
                                  where m.CategoryName == "Delete"
                                  select m).FirstOrDefault();

                if (_deleteMessage == null)
                {
                    //shouldn't happen
                    throw new Exception("Delete Message not found");
                }
            }
#pragma warning restore CS8601 // Possible null reference assignment.
        }

    }
}
