
SELECT 'drop table ' +TABLE_SCHEMA +'.'+ table_name FROM information_schema.tables where TABLE_SCHEMA in('dv','dvlog')

--delete from ArchiveTableSettings
