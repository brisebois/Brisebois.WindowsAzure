CREATE PROCEDURE [dbo].[GetActiveConnections]
AS
	SELECT
	  e.connection_id,
	  s.session_id,
	  s.login_name,
	  s.last_request_end_time,
	  s.cpu_time
	FROM
	  sys.dm_exec_sessions s
	  INNER JOIN sys.dm_exec_connections e
	  ON s.session_id = e.session_id