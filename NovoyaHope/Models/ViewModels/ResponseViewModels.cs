using System.Collections.Generic;
using NovoyaHope.Models;
using System;

namespace NovoyaHope.Models.ViewModels
{
    // Основная модель для страницы результатов/аналитики
    public class SurveyResultsViewModel
    {
        public int SurveyId { get; set; }
        public string SurveyTitle { get; set; }
        public int TotalResponses { get; set; }
        public DateTime CreationDate { get; set; }

        // Список вопросов с агрегированными результатами
        public List<QuestionResultViewModel> QuestionResults { get; set; }
    }

    // Результаты для одного вопроса (для построения графика)
    public class QuestionResultViewModel
    {
        public int QuestionId { get; set; }
        public string Text { get; set; }
        public QuestionType Type { get; set; }

        // Для вопросов с выбором: ключ - ID опции, значение - количество голосов
        public Dictionary<int, int> OptionCounts { get; set; }
        public Dictionary<int, string> OptionTexts { get; set; } // Текст опций

        // Для текстовых вопросов
        public List<string> TextAnswers { get; set; }

        // Для шкалы
        public double AverageScore { get; set; }
    }

    // Модель для детального просмотра списка ответов (таблица)
    public class DetailedResponseListViewModel
    {
        public int ResponseId { get; set; }
        public string RespondentEmail { get; set; } // Null, если анонимно
        public DateTime SubmissionDate { get; set; }

        // Словарь: Ключ - ID вопроса, Значение - строка ответа (уже отформатированная)
        public Dictionary<int, string> Answers { get; set; }
    }
}