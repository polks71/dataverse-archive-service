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

namespace ArchiveService.Management.App.Data.Models.Dataverse
{
	
	
	[System.Runtime.Serialization.DataContractAttribute()]
	[Microsoft.Xrm.Sdk.Client.EntityLogicalNameAttribute("entity")]
	[System.CodeDom.Compiler.GeneratedCodeAttribute("Dataverse Model Builder", "1.0.0.24")]
	public partial class Entity_Ent : Microsoft.Xrm.Sdk.Entity, System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	{
		
		/// <summary>
		/// Available fields, a the time of codegen, for the entity entity
		/// </summary>
		public static class Fields
		{
			public const string AddressTableName = "addresstablename";
			public const string BaseTableName = "basetablename";
			public const string CollectionName = "collectionname";
			public const string ComponentState = "componentstate";
			public const string EntityId = "entityid";
			public const string Id = "entityid";
			public const string EntitySetName1 = "entitysetname";
			public const string ExtensionTableName = "extensiontablename";
			public const string ExternalCollectionName = "externalcollectionname";
			public const string ExternalName = "externalname";
			public const string IsActivity = "isactivity";
			public const string LogicalCollectionName = "logicalcollectionname";
			public const string LogicalName1 = "logicalname";
			public const string Name = "name";
			public const string ObjectTypeCode = "objecttypecode";
			public const string OriginalLocalizedCollectionName = "originallocalizedcollectionname";
			public const string OriginalLocalizedName = "originallocalizedname";
			public const string OverwriteTime = "overwritetime";
			public const string ParentControllingAttributeName = "parentcontrollingattributename";
			public const string PhysicalName = "physicalname";
			public const string ReportViewName = "reportviewname";
			public const string SolutionId = "solutionid";
			public const string VersionNumber = "versionnumber";
		}
		
		/// <summary>
		/// Default Constructor.
		/// </summary>
		public Entity_Ent() : 
				base(EntityLogicalName)
		{
		}
		
		public const string EntityLogicalName = "entity";
		
		public const string EntityLogicalCollectionName = "entities";
		
		public const string EntitySetName = "entities";
		
		public const int EntityTypeCode = 9800;
		
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		public event System.ComponentModel.PropertyChangingEventHandler PropertyChanging;
		
		private void OnPropertyChanged(string propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void OnPropertyChanging(string propertyName)
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, new System.ComponentModel.PropertyChangingEventArgs(propertyName));
			}
		}
		
		/// <summary>
		/// The address table name of this entity.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("addresstablename")]
		public string AddressTableName
		{
			get
			{
				return this.GetAttributeValue<string>("addresstablename");
			}
			set
			{
				this.OnPropertyChanging("AddressTableName");
				this.SetAttributeValue("addresstablename", value);
				this.OnPropertyChanged("AddressTableName");
			}
		}
		
		/// <summary>
		/// The base table name of this entity.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("basetablename")]
		public string BaseTableName
		{
			get
			{
				return this.GetAttributeValue<string>("basetablename");
			}
			set
			{
				this.OnPropertyChanging("BaseTableName");
				this.SetAttributeValue("basetablename", value);
				this.OnPropertyChanged("BaseTableName");
			}
		}
		
		/// <summary>
		/// The collection name of this entity.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("collectionname")]
		public string CollectionName
		{
			get
			{
				return this.GetAttributeValue<string>("collectionname");
			}
			set
			{
				this.OnPropertyChanging("CollectionName");
				this.SetAttributeValue("collectionname", value);
				this.OnPropertyChanged("CollectionName");
			}
		}
		
		/// <summary>
		/// For internal use only.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("componentstate")]
		public virtual componentstate? ComponentState
		{
			get
			{
				return ((componentstate?)(EntityOptionSetEnum.GetEnum(this, "componentstate")));
			}
		}
		
		/// <summary>
		/// Unique identifier of the entity.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("entityid")]
		public System.Nullable<System.Guid> EntityId
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<System.Guid>>("entityid");
			}
			set
			{
				this.OnPropertyChanging("EntityId");
				this.SetAttributeValue("entityid", value);
				if (value.HasValue)
				{
					base.Id = value.Value;
				}
				else
				{
					base.Id = System.Guid.Empty;
				}
				this.OnPropertyChanged("EntityId");
			}
		}
		
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("entityid")]
		public override System.Guid Id
		{
			get
			{
				return base.Id;
			}
			set
			{
				this.EntityId = value;
			}
		}
		
		/// <summary>
		/// The entity set name of this entity.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("entitysetname")]
		public string EntitySetName1
		{
			get
			{
				return this.GetAttributeValue<string>("entitysetname");
			}
			set
			{
				this.OnPropertyChanging("EntitySetName1");
				this.SetAttributeValue("entitysetname", value);
				this.OnPropertyChanged("EntitySetName1");
			}
		}
		
		/// <summary>
		/// The extension table name of this entity.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("extensiontablename")]
		public string ExtensionTableName
		{
			get
			{
				return this.GetAttributeValue<string>("extensiontablename");
			}
			set
			{
				this.OnPropertyChanging("ExtensionTableName");
				this.SetAttributeValue("extensiontablename", value);
				this.OnPropertyChanged("ExtensionTableName");
			}
		}
		
		/// <summary>
		/// The external collection name of this entity.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("externalcollectionname")]
		public string ExternalCollectionName
		{
			get
			{
				return this.GetAttributeValue<string>("externalcollectionname");
			}
			set
			{
				this.OnPropertyChanging("ExternalCollectionName");
				this.SetAttributeValue("externalcollectionname", value);
				this.OnPropertyChanged("ExternalCollectionName");
			}
		}
		
		/// <summary>
		/// The external name of this entity.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("externalname")]
		public string ExternalName
		{
			get
			{
				return this.GetAttributeValue<string>("externalname");
			}
			set
			{
				this.OnPropertyChanging("ExternalName");
				this.SetAttributeValue("externalname", value);
				this.OnPropertyChanged("ExternalName");
			}
		}
		
		/// <summary>
		/// Whether this entity is of type activity.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("isactivity")]
		public System.Nullable<bool> IsActivity
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<bool>>("isactivity");
			}
		}
		
		/// <summary>
		/// The logical collection name of this entity.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("logicalcollectionname")]
		public string LogicalCollectionName
		{
			get
			{
				return this.GetAttributeValue<string>("logicalcollectionname");
			}
			set
			{
				this.OnPropertyChanging("LogicalCollectionName");
				this.SetAttributeValue("logicalcollectionname", value);
				this.OnPropertyChanged("LogicalCollectionName");
			}
		}
		
		/// <summary>
		/// The logical name of this entity.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("logicalname")]
		public string LogicalName1
		{
			get
			{
				return this.GetAttributeValue<string>("logicalname");
			}
			set
			{
				this.OnPropertyChanging("LogicalName1");
				this.SetAttributeValue("logicalname", value);
				this.OnPropertyChanged("LogicalName1");
			}
		}
		
		/// <summary>
		/// The name of this Entity.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("name")]
		public string Name
		{
			get
			{
				return this.GetAttributeValue<string>("name");
			}
			set
			{
				this.OnPropertyChanging("Name");
				this.SetAttributeValue("name", value);
				this.OnPropertyChanged("Name");
			}
		}
		
		/// <summary>
		/// The object type code of this entity.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("objecttypecode")]
		public System.Nullable<int> ObjectTypeCode
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<int>>("objecttypecode");
			}
		}
		
		/// <summary>
		/// The original localized collection name of this entity.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("originallocalizedcollectionname")]
		public string OriginalLocalizedCollectionName
		{
			get
			{
				return this.GetAttributeValue<string>("originallocalizedcollectionname");
			}
			set
			{
				this.OnPropertyChanging("OriginalLocalizedCollectionName");
				this.SetAttributeValue("originallocalizedcollectionname", value);
				this.OnPropertyChanged("OriginalLocalizedCollectionName");
			}
		}
		
		/// <summary>
		/// The original localized name of this entity.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("originallocalizedname")]
		public string OriginalLocalizedName
		{
			get
			{
				return this.GetAttributeValue<string>("originallocalizedname");
			}
			set
			{
				this.OnPropertyChanging("OriginalLocalizedName");
				this.SetAttributeValue("originallocalizedname", value);
				this.OnPropertyChanged("OriginalLocalizedName");
			}
		}
		
		/// <summary>
		/// For internal use only.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("overwritetime")]
		public System.Nullable<System.DateTime> OverwriteTime
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<System.DateTime>>("overwritetime");
			}
		}
		
		/// <summary>
		/// The parent controlling attribute name of this entity.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("parentcontrollingattributename")]
		public string ParentControllingAttributeName
		{
			get
			{
				return this.GetAttributeValue<string>("parentcontrollingattributename");
			}
			set
			{
				this.OnPropertyChanging("ParentControllingAttributeName");
				this.SetAttributeValue("parentcontrollingattributename", value);
				this.OnPropertyChanged("ParentControllingAttributeName");
			}
		}
		
		/// <summary>
		/// The physical name of this entity.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("physicalname")]
		public string PhysicalName
		{
			get
			{
				return this.GetAttributeValue<string>("physicalname");
			}
			set
			{
				this.OnPropertyChanging("PhysicalName");
				this.SetAttributeValue("physicalname", value);
				this.OnPropertyChanged("PhysicalName");
			}
		}
		
		/// <summary>
		/// The Report view name of this entity.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("reportviewname")]
		public string ReportViewName
		{
			get
			{
				return this.GetAttributeValue<string>("reportviewname");
			}
			set
			{
				this.OnPropertyChanging("ReportViewName");
				this.SetAttributeValue("reportviewname", value);
				this.OnPropertyChanged("ReportViewName");
			}
		}
		
		/// <summary>
		/// Unique identifier of the associated solution.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("solutionid")]
		public System.Nullable<System.Guid> SolutionId
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<System.Guid>>("solutionid");
			}
		}
		
		/// <summary>
		/// The version number of this entity.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("versionnumber")]
		public System.Nullable<long> VersionNumber
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<long>>("versionnumber");
			}
		}
	}
}
#pragma warning restore CS1591
