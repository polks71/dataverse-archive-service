#pragma warning disable CS1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

[assembly: Microsoft.Xrm.Sdk.Client.ProxyTypesAssemblyAttribute()]

namespace ArchiveService.Management.App.Data.Models.Dataverse
{
	
	
	/// <summary>
	/// Represents a source of entities bound to a Dataverse service. It tracks and manages changes made to the retrieved entities.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("Dataverse Model Builder", "1.0.0.24")]
	public partial class dataverse : Microsoft.Xrm.Sdk.Client.OrganizationServiceContext
	{
		
		/// <summary>
		/// Constructor.
		/// </summary>
		public dataverse(Microsoft.Xrm.Sdk.IOrganizationService service) : 
				base(service)
		{
		}
		
		/// <summary>
		/// Gets a binding to the set of all <see cref="ArchiveService.Management.App.Data.Models.Dataverse.Entity_Ent"/> entities.
		/// </summary>
		public System.Linq.IQueryable<ArchiveService.Management.App.Data.Models.Dataverse.Entity_Ent> Entity_EntSet
		{
			get
			{
				return this.CreateQuery<ArchiveService.Management.App.Data.Models.Dataverse.Entity_Ent>();
			}
		}
		
		/// <summary>
		/// Gets a binding to the set of all <see cref="ArchiveService.Management.App.Data.Models.Dataverse.PluginType"/> entities.
		/// </summary>
		public System.Linq.IQueryable<ArchiveService.Management.App.Data.Models.Dataverse.PluginType> PluginTypeSet
		{
			get
			{
				return this.CreateQuery<ArchiveService.Management.App.Data.Models.Dataverse.PluginType>();
			}
		}
		
		/// <summary>
		/// Gets a binding to the set of all <see cref="ArchiveService.Management.App.Data.Models.Dataverse.SdkMessage"/> entities.
		/// </summary>
		public System.Linq.IQueryable<ArchiveService.Management.App.Data.Models.Dataverse.SdkMessage> SdkMessageSet
		{
			get
			{
				return this.CreateQuery<ArchiveService.Management.App.Data.Models.Dataverse.SdkMessage>();
			}
		}
		
		/// <summary>
		/// Gets a binding to the set of all <see cref="ArchiveService.Management.App.Data.Models.Dataverse.SdkMessageFilter"/> entities.
		/// </summary>
		public System.Linq.IQueryable<ArchiveService.Management.App.Data.Models.Dataverse.SdkMessageFilter> SdkMessageFilterSet
		{
			get
			{
				return this.CreateQuery<ArchiveService.Management.App.Data.Models.Dataverse.SdkMessageFilter>();
			}
		}
		
		/// <summary>
		/// Gets a binding to the set of all <see cref="ArchiveService.Management.App.Data.Models.Dataverse.SdkMessageProcessingStep"/> entities.
		/// </summary>
		public System.Linq.IQueryable<ArchiveService.Management.App.Data.Models.Dataverse.SdkMessageProcessingStep> SdkMessageProcessingStepSet
		{
			get
			{
				return this.CreateQuery<ArchiveService.Management.App.Data.Models.Dataverse.SdkMessageProcessingStep>();
			}
		}
	}
}
#pragma warning restore CS1591
