CREATE PROCEDURE [dbo].[GetDatabaseSizeRecommendation]
	@databasename AS VARCHAR(64)
AS

	DECLARE @dbsize AS FLOAT
	
	SELECT @dbsize = SUM(reserved_page_count)*8.0/1024/1024
	FROM sys.dm_db_partition_stats

	DECLARE @increment AS FLOAT = 0.1
	
	DECLARE @maxSize AS BIGINT
	SELECT @maxSize = CONVERT(
						BIGINT,
					    DATABASEPROPERTYEX (
								@databasename,
								'MaxSizeInBytes'))

	SELECT @maxSize = @maxSize / 1024 / 1024 / 1024
	
	--1 | 5 | 10 | 20 | 30 | 40 | 50 | 100 | 150
	SELECT @dbsize = @dbsize + @increment

	DECLARE @newMaxSize AS INT
	SELECT @newMaxSize = 150
		
	IF(@dbsize < 100)
	BEGIN
		SELECT @newMaxSize = 100
	END

	IF(@dbsize < 50)
	BEGIN
		SELECT @newMaxSize = 50
	END

	IF(@dbsize < 40)
	BEGIN
		SELECT @newMaxSize = 40
	END

	IF(@dbsize < 30)
	BEGIN
		SELECT @newMaxSize = 30
	END

	IF(@dbsize < 20)
	BEGIN
		SELECT @newMaxSize = 20
	END

	IF(@dbsize < 10)
	BEGIN
		SELECT @newMaxSize = 10
	END

	IF(@dbsize < 5)
	BEGIN
		SELECT @newMaxSize = 5
	END

	IF(@dbsize < 1)
	BEGIN
		SELECT @newMaxSize = 1
	END

	DECLARE @edition AS VARCHAR(50) = 'Business'
	IF(@newMaxSize < 10)
	BEGIN
		SELECT @edition = 'Web'
	END
		
	SELECT @dbsize AS [CurrentSize], 
		   CONVERT(INT,@MaxSize) AS [CurrentMaxSize], 
		   @newMaxSize AS [MaxSize], 
		   @edition AS [Edition]