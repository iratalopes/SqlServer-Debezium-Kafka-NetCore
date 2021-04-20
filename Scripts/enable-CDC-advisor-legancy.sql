EXEC sp_changedbowner 'sa'
GO

EXEC sys.sp_cdc_enable_db
GO

EXEC sys.sp_cdc_enable_table
    @source_schema = N'dbo',
    @source_name   = N'ADVISOR_ClIENT_ACCOUNT',
    @role_name     = NULL,
    @supports_net_changes = 1
GO

-- Conferir CDC habilitou
SELECT  name, is_tracked_by_cdc
FROM    sys.tables
WHERE   is_tracked_by_cdc = 1

EXECUTE sys.sp_cdc_help_change_data_capture @source_schema = 'dbo', @source_name = 'ADVISOR_ClIENT_ACCOUNT';

--Verificar os dados 
select * from cdc.dbo_ADVISOR_ClIENT_ACCOUNT_CT
