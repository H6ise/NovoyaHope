-- ==========================================
-- Добавление полей профиля пользователя
-- ==========================================

USE [NovoyaHopeDB];  -- Замените на имя вашей базы данных
GO

-- Проверка существования столбцов
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Identity].[User]') AND name = 'FirstName')
BEGIN
    ALTER TABLE [Identity].[User]
    ADD [FirstName] nvarchar(50) NULL;
    PRINT '✓ Добавлен столбец FirstName';
END
ELSE
BEGIN
    PRINT '○ Столбец FirstName уже существует';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Identity].[User]') AND name = 'LastName')
BEGIN
    ALTER TABLE [Identity].[User]
    ADD [LastName] nvarchar(50) NULL;
    PRINT '✓ Добавлен столбец LastName';
END
ELSE
BEGIN
    PRINT '○ Столбец LastName уже существует';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Identity].[User]') AND name = 'ProfileImagePath')
BEGIN
    ALTER TABLE [Identity].[User]
    ADD [ProfileImagePath] nvarchar(500) NULL;
    PRINT '✓ Добавлен столбец ProfileImagePath';
END
ELSE
BEGIN
    PRINT '○ Столбец ProfileImagePath уже существует';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Identity].[User]') AND name = 'ShowPhoneToPublic')
BEGIN
    ALTER TABLE [Identity].[User]
    ADD [ShowPhoneToPublic] bit NOT NULL DEFAULT 0;
    PRINT '✓ Добавлен столбец ShowPhoneToPublic';
END
ELSE
BEGIN
    PRINT '○ Столбец ShowPhoneToPublic уже существует';
END

-- Добавление записи в таблицу миграций
IF NOT EXISTS (SELECT * FROM [Identity].[__EFMigrationsHistory] WHERE [MigrationId] = N'20251127000000_AddUserProfileFields')
BEGIN
    INSERT INTO [Identity].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251127000000_AddUserProfileFields', N'8.0.0');
    PRINT '✓ Миграция добавлена в историю';
END
ELSE
BEGIN
    PRINT '○ Миграция уже в истории';
END

-- Проверка результата
SELECT 
    'FirstName' AS [Column],
    CASE WHEN COL_LENGTH('[Identity].[User]', 'FirstName') IS NOT NULL THEN '✓' ELSE '✗' END AS [Exists]
UNION ALL
SELECT 
    'LastName',
    CASE WHEN COL_LENGTH('[Identity].[User]', 'LastName') IS NOT NULL THEN '✓' ELSE '✗' END
UNION ALL
SELECT 
    'ProfileImagePath',
    CASE WHEN COL_LENGTH('[Identity].[User]', 'ProfileImagePath') IS NOT NULL THEN '✓' ELSE '✗' END
UNION ALL
SELECT 
    'ShowPhoneToPublic',
    CASE WHEN COL_LENGTH('[Identity].[User]', 'ShowPhoneToPublic') IS NOT NULL THEN '✓' ELSE '✗' END;

PRINT '';
PRINT '==========================================';
PRINT 'Миграция завершена успешно!';
PRINT '==========================================';
GO

