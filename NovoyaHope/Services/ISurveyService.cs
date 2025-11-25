// Services/ISurveyService.cs

using NovoyaHope.Models.DataModels;
using NovoyaHope.Models.ViewModels.SurveyViewModels;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NovoyaHope.Services
{
    public interface ISurveyService
    {
        /// <summary>
        /// Сохраняет или обновляет опрос, включая его вопросы и варианты ответов.
        /// </summary>
        /// <param name="model">Модель данных из конструктора.</param>
        /// <param name="userId">ID пользователя, который сохраняет опрос.</param>
        /// <returns>ID сохраненного или обновленного опроса.</returns>
        Task<int> SaveSurveyAsync(SaveSurveyViewModel model, string userId);

        /// <summary>
        /// Получает полный опрос по ID для отображения в конструкторе.
        /// </summary>
        /// <param name="surveyId">ID опроса.</param>
        /// <param name="userId">ID пользователя (для проверки прав доступа).</param>
        /// <returns>Модель представления SaveSurveyViewModel или null, если опрос не найден.</returns>
        Task<SaveSurveyViewModel> GetSurveyForEditAsync(int surveyId, string userId);

        /// <summary>
        /// Публикует или снимает с публикации опрос.
        /// </summary>
        /// <param name="surveyId">ID опроса.</param>
        /// <param name="userId">ID пользователя (для проверки прав доступа).</param>
        /// <param name="isPublished">Новый статус публикации.</param>
        /// <returns>True, если статус изменен успешно.</returns>
        Task<bool> ChangePublicationStatusAsync(int surveyId, string userId, bool isPublished);

        /// <summary>
        /// Проверяет, валиден ли опрос для публикации (есть ли вопросы).
        /// </summary>
        /// <param name="surveyId">ID опроса.</param>
        /// <param name="userId">ID пользователя.</param>
        /// <returns>True, если опрос можно публиковать.</returns>
        Task<bool> IsSurveyValidForPublishingAsync(int surveyId, string userId);

        /// <summary>
        /// Удаляет опрос по ID.
        /// </summary>
        Task<bool> DeleteSurveyAsync(int surveyId, string userId);
    }
}