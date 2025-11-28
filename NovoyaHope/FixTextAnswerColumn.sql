-- Исправление колонки TextAnswer в таблице UserAnswers
-- Этот скрипт делает колонку TextAnswer nullable, если она еще не nullable

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Identity].[UserAnswers]') AND type in (N'U'))
BEGIN
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Identity].[UserAnswers]') AND name = 'TextAnswer' AND is_nullable = 0)
    BEGIN
        ALTER TABLE [Identity].[UserAnswers] ALTER COLUMN [TextAnswer] nvarchar(max) NULL;
        PRINT 'Колонка TextAnswer успешно изменена на nullable';
    END
    ELSE
    BEGIN
        PRINT 'Колонка TextAnswer уже является nullable';
    END
END
ELSE
BEGIN
    PRINT 'Таблица [Identity].[UserAnswers] не найдена';
END

