CREATE PROCEDURE [dbo].[GetTableSizes]
AS

	SELECT sys.objects.name, SUM(reserved_page_count) * 8.0 / 1024 / 1024
	FROM sys.dm_db_partition_stats, sys.objects 
	WHERE sys.dm_db_partition_stats.object_id = sys.objects.object_id 
	GROUP BY sys.objects.name; 