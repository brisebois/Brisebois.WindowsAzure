CREATE PROCEDURE [dbo].[GetTop50CpuConsumingQueries]
AS
SELECT
    highest_cpu_queries.plan_handle,  
    highest_cpu_queries.total_worker_time, 
    q.dbid, 
    q.objectid, 
    q.number, 
    q.encrypted, 
    q.[text] 
FROM 
    (SELECT TOP 50  
        qs.plan_handle,  
        qs.total_worker_time 
     FROM 
        sys.dm_exec_query_stats qs 
     ORDER BY qs.total_worker_time desc) AS highest_cpu_queries 
     CROSS APPLY sys.dm_exec_sql_text(plan_handle) AS q 
ORDER BY highest_cpu_queries.total_worker_time desc