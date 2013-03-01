CREATE PROCEDURE [dbo].[GetDatabaseSize]
AS
	DECLARE @bdsize AS FLOAT
	
	SELECT @bdsize = SUM(reserved_page_count)
	FROM sys.dm_db_partition_stats

RETURN @bdsize * 8.0 / 1024 / 1024