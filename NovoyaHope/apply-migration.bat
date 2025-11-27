@echo off
chcp 65001 > nul
echo ==========================================
echo Применение миграции базы данных
echo ==========================================
echo.

echo Шаг 1: Проверка существующих миграций...
dotnet ef migrations list
echo.

echo Шаг 2: Применение миграции AddUserProfileFields...
dotnet ef database update
echo.

if %ERRORLEVEL% EQU 0 (
    echo ==========================================
    echo ✓ Миграция успешно применена!
    echo ==========================================
) else (
    echo ==========================================
    echo ✗ Ошибка при применении миграции
    echo ==========================================
    echo Попробуйте выполнить вручную:
    echo   dotnet ef database update
)

echo.
pause

