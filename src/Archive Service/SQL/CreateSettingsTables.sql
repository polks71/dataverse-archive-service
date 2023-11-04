create table ArchiveServiceSettings (
	HistoricalArchive bit,
	ChangeLogEnabled bit,
	ArchiveEnabled bit,
	DataverseUrl varchar(255),
	SchemaName varchar(10)
)

--drop table archivetablesettings
create table ArchiveTableSettings (
	DisplayName varchar(510) not null,
	LogicalName varchar(510) not null,
	ArchiveEnabled bit,
	ChangeLogEnabled bit,
	ChangeLogEnabledDate datetime,
	LastChangeTrackingToken varchar(1000),
	CONSTRAINT UIX_LogicalName UNIQUE(LogicalName)
)