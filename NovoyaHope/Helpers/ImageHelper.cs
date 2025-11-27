using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NovoyaHope.Helpers
{
    /// <summary>
    /// Вспомогательный класс для работы с изображениями
    /// </summary>
    public static class ImageHelper
    {
        // Разрешенные форматы изображений
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
        
        // Максимальный размер файла (5 МБ)
        private const long MaxFileSize = 5 * 1024 * 1024;

        /// <summary>
        /// Проверка валидности изображения
        /// </summary>
        public static bool IsValidImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            // Проверка размера
            if (file.Length > MaxFileSize)
                return false;

            // Проверка расширения
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                return false;

            // Проверка MIME типа
            if (!IsValidImageMimeType(file.ContentType))
                return false;

            return true;
        }

        /// <summary>
        /// Проверка MIME типа изображения
        /// </summary>
        private static bool IsValidImageMimeType(string contentType)
        {
            var validMimeTypes = new[]
            {
                "image/jpeg",
                "image/jpg",
                "image/png",
                "image/gif",
                "image/webp",
                "image/bmp"
            };

            return validMimeTypes.Contains(contentType.ToLowerInvariant());
        }

        /// <summary>
        /// Получить сообщение об ошибке для невалидного изображения
        /// </summary>
        public static string GetValidationError(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return "Файл не выбран";

            if (file.Length > MaxFileSize)
                return $"Размер файла превышает {MaxFileSize / (1024 * 1024)} МБ";

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                return $"Недопустимый формат файла. Разрешены: {string.Join(", ", AllowedExtensions)}";

            if (!IsValidImageMimeType(file.ContentType))
                return "Недопустимый MIME тип файла";

            return "Неизвестная ошибка";
        }

        /// <summary>
        /// Сохранить изображение профиля
        /// </summary>
        public static async Task<string> SaveProfileImageAsync(IFormFile file, string userId, string webRootPath)
        {
            if (!IsValidImage(file))
                throw new ArgumentException(GetValidationError(file));

            var uploadsFolder = Path.Combine(webRootPath, "uploads", "profiles");
            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var uniqueFileName = $"{userId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Сохранение файла
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/profiles/{uniqueFileName}";
        }

        /// <summary>
        /// Удалить изображение профиля
        /// </summary>
        public static void DeleteProfileImage(string imagePath, string webRootPath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return;

            var fullPath = Path.Combine(webRootPath, imagePath.TrimStart('/'));
            
            if (File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);
                }
                catch (Exception)
                {
                    // Логирование ошибки, но не прерывание выполнения
                }
            }
        }


        /// <summary>
        /// Получить размер изображения в байтах
        /// </summary>
        public static long GetImageSize(string imagePath, string webRootPath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return 0;

            var fullPath = Path.Combine(webRootPath, imagePath.TrimStart('/'));
            
            if (!File.Exists(fullPath))
                return 0;

            var fileInfo = new FileInfo(fullPath);
            return fileInfo.Length;
        }

        /// <summary>
        /// Форматировать размер файла для отображения
        /// </summary>
        public static string FormatFileSize(long bytes)
        {
            string[] sizes = { "Б", "КБ", "МБ", "ГБ" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }
}

