using NovoyaHope.Models;
using System;
using System.Collections.Generic;

namespace NovoyaHope.Models.ViewModels
{
    // ViewModel для главной страницы админ-панели
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalSurveys { get; set; }
        public int TotalTemplates { get; set; }
        public int TotalResponses { get; set; }
        public int PublishedSurveys { get; set; }
        public List<SurveyListItemViewModel> RecentSurveys { get; set; } = new List<SurveyListItemViewModel>();
        public List<UserListItemViewModel> RecentUsers { get; set; } = new List<UserListItemViewModel>();
    }

    // ViewModel для списка опросов в админ-панели
    public class SurveyListItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string CreatorName { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsPublished { get; set; }
        public int ResponseCount { get; set; }
    }

    // ViewModel для списка пользователей в админ-панели
    public class UserListItemViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int CreatedSurveysCount { get; set; }
    }

    // ViewModel для управления пользователями
    public class UserManagementViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public bool EmailConfirmed { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public int CreatedSurveysCount { get; set; }
        public int ResponsesCount { get; set; }
    }

    // ViewModel для управления опросами
    public class SurveyManagementViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CreatorName { get; set; }
        public string CreatorEmail { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsPublished { get; set; }
        public SurveyType Type { get; set; }
        public int ResponseCount { get; set; }
        public int QuestionCount { get; set; }
    }

    // ViewModel для управления шаблонами
    public class TemplateManagementViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public SurveyType Type { get; set; }
        public int QuestionCount { get; set; }
    }
}

