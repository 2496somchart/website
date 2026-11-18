-- รันโดย service db-init ด้วยบัญชี sa ครั้งเดียวต่อการ start (idempotent)
-- สร้างฐาน EasyScheduleWeb (collation ไทย) + login เฉพาะแอป (db_owner เฉพาะฐานนี้ ไม่ใช่ sysadmin)
-- ตัวแปร sqlcmd: $(AppLogin), $(AppSecret)
SET NOCOUNT ON;

IF DB_ID(N'EasyScheduleWeb') IS NULL
    CREATE DATABASE [EasyScheduleWeb] COLLATE Thai_100_CI_AS;
GO

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'$(AppLogin)')
    CREATE LOGIN [$(AppLogin)] WITH PASSWORD = N'$(AppSecret)', DEFAULT_DATABASE = [EasyScheduleWeb], CHECK_POLICY = ON;
ELSE
    ALTER LOGIN [$(AppLogin)] WITH PASSWORD = N'$(AppSecret)';
GO

USE [EasyScheduleWeb];
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'$(AppLogin)')
    CREATE USER [$(AppLogin)] FOR LOGIN [$(AppLogin)];
ALTER ROLE db_owner ADD MEMBER [$(AppLogin)];
GO
PRINT 'db-init: OK';
