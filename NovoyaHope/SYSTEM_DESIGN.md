# Проектирование системы NovoyaHope Forms

## Введение

NovoyaHope Forms - это веб-приложение для создания, публикации и управления опросами. Система позволяет пользователям создавать опросы с различными типами вопросов, настраивать внешний вид, собирать ответы и анализировать результаты.

**Технологический стек:**
- **Backend:** ASP.NET Core 8.0 (MVC)
- **Frontend:** Razor Pages, JavaScript (ES6+), CSS3
- **База данных:** SQL Server (Entity Framework Core 8.0)
- **Аутентификация:** ASP.NET Core Identity
- **Архитектура:** MVC (Model-View-Controller)
- **ORM:** Entity Framework Core
- **Стили:** Custom CSS, Bootstrap (частично)
 
- Microsoft.AspNetCore.Identity.EntityFrameworkCore (8.0.0)
- Microsoft.EntityFrameworkCore.SqlServer (8.0.0)
- Microsoft.AspNetCore.Authorization (8.0.0)
- Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation (8.0.0)

---

## 2.1 Описание логической и физической структуры системы

### 2.1.1 Логическая структура системы

Система NovoyaHope Forms построена по архитектуре MVC (Model-View-Controller) с использованием ASP.NET Core и Entity Framework Core. Логическая структура включает следующие основные компоненты:

#### Уровни системы:

1. **Уровень представления (Presentation Layer)**
   - Razor Views (CSHTML файлы)
   - JavaScript для интерактивности
   - CSS для стилизации
   - Расположение: `Views/`, `wwwroot/`

2. **Уровень контроллеров (Controller Layer)**
   - Обработка HTTP-запросов
   - Валидация данных
   - Координация между моделями и представлениями
   - Расположение: `Controllers/`

3. **Уровень бизнес-логики (Business Logic Layer)**
   - Сервисы для обработки бизнес-логики
   - Расположение: `Services/`

4. **Уровень доступа к данным (Data Access Layer)**
   - Entity Framework Core
   - DbContext для работы с базой данных
   - Расположение: `Data/`, `Models/`

5. **Уровень данных (Data Layer)**
   - SQL Server база данных
   - Таблицы для хранения опросов, вопросов, ответов

### 2.1.2 Физическая структура системы

```
NovoyaHope/
├── Controllers/          # Контроллеры MVC
│   ├── AccountController.cs      # Управление аккаунтом
│   ├── AdminController.cs        # Административная панель
│   ├── HomeController.cs         # Главная страница
│   ├── PublicController.cs       # Публичный доступ к опросам
│   └── SurveyController.cs       # Управление опросами
│
├── Data/                 # Доступ к данным
│   ├── ApplicationDbContext.cs   # Контекст базы данных
│   └── DbInitializer.cs          # Инициализация БД
│
├── Models/              # Модели данных
│   ├── ApplicationUser.cs         # Пользователь
│   ├── Survey.cs                 # Опрос
│   ├── Question.cs               # Вопрос
│   ├── AnswerOption.cs           # Вариант ответа
│   ├── Section.cs                # Раздел
│   ├── Media.cs                  # Медиа-контент
│   ├── SurveyResponse.cs         # Ответ на опрос
│   ├── UserAnswer.cs             # Ответ пользователя
│   └── ViewModels/               # Модели представления
│
├── Services/            # Бизнес-логика
│   ├── ISurveyService.cs
│   └── SurveyService.cs
│
├── Views/              # Представления
│   ├── Account/        # Страницы аккаунта
│   ├── Admin/          # Административная панель
│   ├── Home/           # Главная страница
│   ├── Public/         # Публичные страницы
│   ├── Survey/         # Управление опросами
│   └── Shared/         # Общие компоненты
│
├── wwwroot/            # Статические файлы
│   ├── css/            # Стили
│   ├── js/             # JavaScript
│   └── uploads/        # Загруженные файлы
│
└── Program.cs          # Точка входа приложения
```

### 2.1.3 Архитектурные паттерны

- **MVC (Model-View-Controller)**: Разделение логики, данных и представления
- **Repository Pattern**: Через Entity Framework Core
- **Dependency Injection**: Встроенная поддержка ASP.NET Core
- **Service Layer**: Для бизнес-логики

---

## 2.2 Моделирование взаимодействия компонентов системы

### 2.2.1 Диаграмма взаимодействия компонентов

```
┌─────────────┐
│   Browser   │
└──────┬──────┘
       │ HTTP Request
       ▼
┌─────────────────┐
│   Controller    │ ◄──────┐
│  (MVC Layer)    │        │
└──────┬──────────┘        │
       │                   │
       ├──► Service Layer  │
       │    (Business)      │
       │                    │
       ├──► ViewModel      │
       │    (Data Transfer) │
       │                    │
       └──► DbContext ──────┘
            (Data Access)
                 │
                 ▼
         ┌───────────────┐
         │  SQL Server   │
         │   Database    │
         └───────────────┘
```

### 2.2.2 Основные потоки взаимодействия

#### Поток 1: Создание и редактирование опроса

```
Пользователь → SurveyController.Edit()
    ↓
Загрузка данных через ApplicationDbContext
    ↓
Создание SurveyConstructorViewModel
    ↓
Отображение Views/Survey/Constructor.cshtml
    ↓
JavaScript обработка (constructor.js)
    ↓
AJAX запрос → SurveyController.Save()
    ↓
SurveyService.SaveSurveyAsync()
    ↓
Сохранение в БД через ApplicationDbContext
```

#### Поток 2: Прохождение опроса

```
Пользователь → PublicController.ViewSurvey()
    ↓
Загрузка опроса из БД
    ↓
Создание PassSurveyViewModel
    ↓
Отображение Views/Public/Pass.cshtml
    ↓
Отправка формы → PublicController.SubmitResponse()
    ↓
Валидация и обработка ответов
    ↓
Сохранение SurveyResponse и UserAnswer
    ↓
Перенаправление на ThankYou
```

#### Поток 3: Просмотр результатов

```
Администратор → SurveyController.Results()
    ↓
Загрузка опроса с ответами
    ↓
Агрегация данных ответов
    ↓
Создание SurveyResultsViewModel
    ↓
Отображение Views/Survey/Results.cshtml
```

### 2.2.3 Взаимодействие с базой данных

```
ApplicationDbContext
    ├── Surveys (DbSet<Survey>)
    ├── Questions (DbSet<Question>)
    ├── AnswerOptions (DbSet<AnswerOption>)
    ├── Sections (DbSet<Section>)
    ├── Media (DbSet<Media>)
    ├── SurveyResponses (DbSet<SurveyResponse>)
    ├── UserAnswers (DbSet<UserAnswer>)
    └── ApplicationUsers (Identity)
```

**Связи между сущностями:**
- Survey 1:N Question
- Survey 1:N Section
- Survey 1:N Media
- Survey 1:N SurveyResponse
- Question 1:N AnswerOption
- Question 1:N UserAnswer
- SurveyResponse 1:N UserAnswer
- ApplicationUser 1:N Survey (Creator)
- ApplicationUser 1:N SurveyResponse

---

## 2.3 Проектирование классов и структур данных в системе

### 2.3.1 Основные классы моделей

#### ApplicationUser
```csharp
public class ApplicationUser : IdentityUser
{
    // Связи
    public ICollection<Survey> CreatedSurveys { get; set; }
    public ICollection<SurveyResponse> Responses { get; set; }
    
    // Профиль
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfileImagePath { get; set; }
    public bool ShowPhoneToPublic { get; set; }
}
```

**Назначение:** Расширение стандартного IdentityUser для хранения профиля пользователя и связей с опросами.

#### Survey
```csharp
public class Survey
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public SurveyType Type { get; set; }
    public bool IsPublished { get; set; }
    public bool IsAnonymous { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    // Настройки теста
    public bool IsTestMode { get; set; }
    public GradePublicationType GradePublication { get; set; }
    public bool ShowIncorrectAnswers { get; set; }
    public bool ShowCorrectAnswers { get; set; }
    public bool ShowPoints { get; set; }
    public int DefaultMaxPoints { get; set; }
    
    // Настройки темы
    public string? ThemeColor { get; set; }
    public string? BackgroundColor { get; set; }
    public string? HeaderImagePath { get; set; }
    public string? HeaderFontFamily { get; set; }
    public int? HeaderFontSize { get; set; }
    public string? QuestionFontFamily { get; set; }
    public int? QuestionFontSize { get; set; }
    public string? TextFontFamily { get; set; }
    public int? TextFontSize { get; set; }
    
    // Связи
    public required string CreatorId { get; set; }
    public ApplicationUser? Creator { get; set; }
    public ICollection<Question>? Questions { get; set; }
    public ICollection<SurveyResponse>? Responses { get; set; }
    public ICollection<Section>? Sections { get; set; }
    public ICollection<Media>? Media { get; set; }
}
```

**Назначение:** Основная сущность опроса, содержит все настройки и связи с вопросами, разделами, медиа и ответами.

#### Question
```csharp
public class Question
{
    public int Id { get; set; }
    public int SurveyId { get; set; }
    public string Text { get; set; }
    public QuestionType Type { get; set; }
    public bool IsRequired { get; set; }
    public int Order { get; set; }
    
    // Связи
    public Survey? Survey { get; set; }
    public ICollection<AnswerOption>? AnswerOptions { get; set; }
    public ICollection<Answer>? Answers { get; set; }
}

public enum QuestionType
{
    ShortText,        // Текст (строка)
    ParagraphText,    // Текст (абзац)
    SingleChoice,     // Один из списка
    MultipleChoice,   // Несколько из списка
    Dropdown,         // Раскрывающийся список
    FileUpload,       // Загрузка файлов
    Scale,            // Шкала
    Rating,           // Оценка (звезды)
    CheckboxGrid,     // Сетка флажков
    RadioGrid,        // Сетка (множественный выбор)
    Date,             // Дата
    Time              // Время
}
```

**Назначение:** Представляет вопрос в опросе с указанием типа и порядка отображения.

#### AnswerOption
```csharp
public class AnswerOption
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string Text { get; set; }
    public int Order { get; set; }
    public bool IsOther { get; set; }
    
    // Связи
    public Question? Question { get; set; }
}
```

**Назначение:** Вариант ответа для вопросов с выбором. Флаг `IsOther` указывает на вариант "Другое".

#### Section
```csharp
public class Section
{
    public int Id { get; set; }
    public int SurveyId { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }
    
    // Связи
    public Survey? Survey { get; set; }
}
```

**Назначение:** Раздел для группировки вопросов в опросе.

#### Media
```csharp
public class Media
{
    public int Id { get; set; }
    public int SurveyId { get; set; }
    public MediaType Type { get; set; }
    public string Url { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }
    public int? QuestionId { get; set; }
    
    // Связи
    public Survey? Survey { get; set; }
    public Question? Question { get; set; }
}

public enum MediaType
{
    Image,  // Изображение
    Video   // Видео
}
```

**Назначение:** Медиа-контент (изображения и видео) для опроса.

#### SurveyResponse
```csharp
public class SurveyResponse
{
    public int Id { get; set; }
    public int SurveyId { get; set; }
    public DateTime SubmissionDate { get; set; }
    public string? UserId { get; set; }
    
    // Связи
    public Survey? Survey { get; set; }
    public ApplicationUser? User { get; set; }
    public ICollection<UserAnswer>? UserAnswers { get; set; }
}
```

**Назначение:** Представляет одну попытку прохождения опроса пользователем.

#### UserAnswer
```csharp
public class UserAnswer
{
    public int Id { get; set; }
    public int ResponseId { get; set; }
    public int QuestionId { get; set; }
    public string? TextAnswer { get; set; }
    public int? SelectedOptionId { get; set; }
    
    // Связи
    public SurveyResponse? Response { get; set; }
    public Question? Question { get; set; }
    public AnswerOption? SelectedOption { get; set; }
}
```

**Назначение:** Конкретный ответ на вопрос в рамках одной попытки прохождения опроса.

### 2.3.2 ViewModels (Модели представления)

ViewModels используются для передачи данных между контроллерами и представлениями, обеспечивая разделение между моделями данных и представлениями.

#### SaveSurveyViewModel
**Назначение:** Используется для сохранения опроса из конструктора

**Свойства:**
- `Id?` - ID опроса (null для нового)
- `Title` - Заголовок
- `Description` - Описание
- `Type` - Тип опроса (Poll/Questionnaire)
- `IsPublished` - Статус публикации
- `IsAnonymous` - Анонимность
- `Questions` - Список вопросов
- `Sections` - Список разделов
- `Media` - Список медиа-элементов
- Настройки теста и темы

#### PassSurveyViewModel
**Назначение:** Используется для отображения опроса пользователю при прохождении

**Свойства:**
- `Id` - ID опроса
- `Title` - Заголовок
- `Description` - Описание
- `IsAnonymous` - Анонимность
- `Questions` - Список вопросов (PassQuestionViewModel)
- `Sections` - Список разделов
- `Media` - Список медиа-элементов
- Настройки темы (цвета, шрифты)

#### PassQuestionViewModel
**Назначение:** Представление вопроса для прохождения опроса

**Свойства:**
- `Id` - ID вопроса
- `Text` - Текст вопроса
- `Type` - Тип вопроса
- `IsRequired` - Обязательность
- `Order` - Порядок
- `Options` - Варианты ответов (PassAnswerOptionViewModel)

#### PassAnswerOptionViewModel
**Назначение:** Представление варианта ответа

**Свойства:**
- `Id` - ID варианта
- `Text` - Текст варианта
- `Order` - Порядок
- `IsOther` - Флаг варианта "Другое"

#### SurveyConstructorViewModel
**Назначение:** Используется для отображения конструктора опросов

**Свойства:**
- `Survey` - Данные опроса
- `Questions` - Список вопросов
- `Sections` - Список разделов
- `Media` - Список медиа-элементов
- `ResultsData` - Данные результатов (для вкладки "Ответы")

#### SurveyResultsViewModel
**Назначение:** Используется для отображения результатов опроса

**Свойства:**
- `SurveyId` - ID опроса
- `SurveyTitle` - Заголовок опроса
- `TotalResponses` - Общее количество ответов
- `CreationDate` - Дата создания
- `QuestionResults` - Результаты по каждому вопросу

#### QuestionResultViewModel
**Назначение:** Результаты по одному вопросу

**Свойства:**
- `QuestionId` - ID вопроса
- `Text` - Текст вопроса
- `Type` - Тип вопроса
- `OptionCounts` - Количество ответов по вариантам
- `OptionTexts` - Тексты вариантов
- `TextAnswers` - Текстовые ответы
- `AverageScore` - Средний балл (для шкалы)

### 2.3.3 Структура базы данных

**Основные таблицы:**
- `Identity.User` - Пользователи
- `Surveys` - Опросы
- `Questions` - Вопросы
- `AnswerOptions` - Варианты ответов
- `Sections` - Разделы
- `Media` - Медиа-контент
- `SurveyResponses` - Ответы на опросы
- `UserAnswers` - Ответы пользователей на вопросы

**Связи:**
- Surveys → Questions (1:N, Cascade Delete)
- Surveys → Sections (1:N, Cascade Delete)
- Surveys → Media (1:N, Cascade Delete)
- Surveys → SurveyResponses (1:N, Cascade Delete)
- Questions → AnswerOptions (1:N, Cascade Delete)
- Questions → UserAnswers (1:N)
- SurveyResponses → UserAnswers (1:N)
- Identity.User → Surveys (1:N, Restrict Delete)

---

## 2.3 Проектирование взаимодействия с пользователем

### 2.3.1 Роли пользователей

#### 1. Анонимный пользователь
- **Доступ:** Просмотр и прохождение опубликованных опросов
- **Ограничения:** Не может создавать опросы, не видит результаты

#### 2. Зарегистрированный пользователь
- **Доступ:**
  - Создание и редактирование опросов
  - Просмотр своих опросов
  - Просмотр результатов своих опросов
  - Управление профилем

#### 3. Администратор
- **Доступ:**
  - Все возможности пользователя
  - Управление всеми опросами
  - Управление пользователями
  - Управление шаблонами

### 2.3.2 Сценарии взаимодействия

#### Сценарий 1: Создание опроса
```
1. Пользователь входит в систему
2. Переходит в раздел "Мои опросы"
3. Нажимает "Создать новый опрос"
4. Открывается конструктор опросов
5. Пользователь:
   - Вводит заголовок и описание
   - Добавляет вопросы через FAB меню
   - Настраивает типы вопросов
   - Добавляет варианты ответов
   - Добавляет разделы и медиа
   - Настраивает тему
6. Сохраняет опрос
7. При необходимости публикует опрос
```

#### Сценарий 2: Прохождение опроса
```
1. Пользователь получает ссылку на опрос
2. Открывает страницу опроса
3. Видит заголовок, описание, разделы, медиа
4. Заполняет вопросы:
   - Текстовые поля
   - Выбор вариантов (radio/checkbox)
   - Загрузка файлов
   - Выбор даты/времени
5. Нажимает "Отправить"
6. Видит страницу благодарности
```

#### Сценарий 3: Просмотр результатов
```
1. Создатель опроса входит в систему
2. Переходит в раздел "Мои опросы"
3. Выбирает опрос
4. Переходит на вкладку "Ответы"
5. Видит:
   - Статистику по каждому вопросу
   - Графики распределения ответов
   - Текстовые ответы
   - Возможность экспорта данных
```

### 2.3.3 Интерфейсы взаимодействия

#### Конструктор опросов
- **Drag-and-drop** для изменения порядка элементов
- **Inline редактирование** текста вопросов и вариантов
- **FAB меню** для быстрого добавления элементов
- **Вкладки** для переключения между вопросами, ответами и настройками
- **Предпросмотр** в реальном времени

#### Прохождение опроса
- **Адаптивный дизайн** для всех устройств
- **Валидация** обязательных полей
- **Автосохранение** (опционально)
- **Прогресс-бар** (опционально)

#### Просмотр результатов
- **Графики и диаграммы** для визуализации
- **Фильтры** по датам
- **Экспорт** в различные форматы
- **Статистика** по каждому вопросу

---

## 2.4 Проектирование интерфейса

### 2.4.1 Принципы дизайна

1. **Минимализм**: Чистый и понятный интерфейс
2. **Консистентность**: Единый стиль во всех разделах
3. **Доступность**: Поддержка различных устройств
4. **Интуитивность**: Понятная навигация

### 2.4.2 Цветовая схема

**Основные цвета:**
- **Primary**: #2e7d32 (Зеленый) / #673AB7 (Фиолетовый) - настраивается
- **Background**: #F3E5F5 (Светло-фиолетовый) - настраивается
- **Text**: #202124 (Темно-серый)
- **Secondary Text**: #5f6368 (Серый)
- **Border**: #dadce0 (Светло-серый)
- **Error**: #d93025 (Красный)
- **Success**: #4caf50 (Зеленый)

**Настройка темы:**
- Пользователь может выбрать цвет темы из палитры
- Настройка шрифтов (семейство и размер)
- Настройка фона
- Загрузка изображения заголовка

### 2.4.3 Типографика

**Шрифты:**
- **Заголовок опроса**: Courier New (по умолчанию), настраивается
- **Вопросы**: Roboto (по умолчанию), настраивается
- **Текст**: Roboto (по умолчанию), настраивается

**Размеры:**
- Заголовок: 24pt (настраивается)
- Вопросы: 12px (настраивается)
- Текст: 11px (настраивается)

### 2.4.4 Компоненты интерфейса

#### Карточки вопросов
- Белый фон
- Тень для глубины
- Синяя полоса слева для активного вопроса
- Поля ввода с подчеркиванием
- Кнопки действий в футере

#### Кнопки
- **Primary**: Зеленый/Фиолетовый фон, белый текст
- **Secondary**: Белый фон, цветной текст и граница
- **Icon buttons**: Иконки без фона, появляются при наведении

#### Формы
- Поля ввода с нижним подчеркиванием
- Фокус: изменение цвета подчеркивания
- Валидация: красный цвет для ошибок
- Placeholder текст для подсказок

#### Модальные окна
- Полупрозрачный фон (overlay)
- Центрированное окно
- Кнопка закрытия
- Анимация появления

### 2.4.5 Адаптивность

**Breakpoints:**
- Desktop: > 992px
- Tablet: 768px - 992px
- Mobile: < 768px

**Адаптация:**
- Гибкая сетка для карточек
- Вертикальное меню на мобильных
- Упрощенная навигация
- Оптимизация изображений

### 2.4.6 Анимации и переходы

- **Slide-in**: Появление новых элементов
- **Fade**: Плавное появление/исчезновение
- **Hover effects**: Изменение цвета и тени
- **Loading states**: Индикаторы загрузки

### 2.4.7 Структура страниц

#### Главная страница
- Шапка с навигацией
- Hero-секция
- Особенности системы
- Призыв к действию

#### Страница опросов
- Сетка карточек опросов
- Фильтры и поиск
- Сортировка
- Модальное меню для действий

#### Конструктор опросов
- Верхняя панель с инструментами
- Центральная область с вопросами
- FAB меню для добавления элементов
- Боковая панель для настроек темы

#### Страница прохождения опроса
- Заголовок опроса
- Последовательность элементов (разделы, медиа, вопросы)
- Форма с валидацией
- Кнопка отправки

#### Страница результатов
- Статистика по опросу
- Карточки с результатами по каждому вопросу
- Графики и диаграммы
- Кнопка экспорта

### 2.4.8 Доступность

- **Семантический HTML**: Правильное использование тегов
- **ARIA атрибуты**: Для screen readers
- **Клавиатурная навигация**: Поддержка Tab и Enter
- **Контрастность**: Соответствие WCAG стандартам
- **Альтернативный текст**: Для изображений

### 2.4.9 Диаграмма классов интерфейса

```
┌─────────────────────────────────────────┐
│         BaseController                  │
│  - UserManager                          │
│  - ApplicationDbContext                 │
└──────────────┬──────────────────────────┘
               │
       ┌───────┴────────┬──────────────┬──────────────┐
       │                │              │              │
┌──────▼──────┐ ┌──────▼──────┐ ┌─────▼──────┐ ┌─────▼──────┐
│SurveyCtrl   │ │PublicCtrl   │ │AccountCtrl │ │AdminCtrl  │
│- Edit()     │ │- ViewSurvey │ │- Login()   │ │- Index()  │
│- Save()     │ │- SubmitResp │ │- Register()│ │- Surveys()│
│- Results()  │ │             │ │- Profile() │ │- Users()  │
└─────────────┘ └─────────────┘ └────────────┘ └───────────┘
```

### 2.4.10 Диаграмма последовательности: Создание опроса

```
Пользователь    Browser    SurveyController    SurveyService    DbContext    Database
    │              │              │                  │              │            │
    │──POST───────>│              │                  │              │            │
    │              │──POST───────>│                  │              │            │
    │              │              │──SaveSurveyAsync─>│              │            │
    │              │              │                  │──Save───────>│            │
    │              │              │                  │              │──INSERT───>│
    │              │              │                  │              │<──OK───────│
    │              │              │                  │<──Success────│            │
    │              │              │<──SurveyId───────│              │            │
    │              │<──JSON───────│                  │              │            │
    │<──Response───│              │                  │              │            │
```

### 2.4.11 Диаграмма последовательности: Прохождение опроса

```
Пользователь    Browser    PublicController    DbContext    Database
    │              │              │              │            │
    │──GET─────────>│              │              │            │
    │              │──GET─────────>│              │            │
    │              │              │──Load───────>│            │
    │              │              │              │──SELECT───>│
    │              │              │              │<──Data─────│
    │              │              │<──Survey─────│            │
    │              │<──HTML───────│              │            │
    │<──Page───────│              │              │            │
    │              │              │              │            │
    │──Submit─────>│              │              │            │
    │              │──POST───────>│              │            │
    │              │              │──Validate───>│            │
    │              │              │──Save───────>│            │
    │              │              │              │──INSERT───>│
    │              │              │              │<──OK───────│
    │              │<──Redirect───│              │            │
    │<──ThankYou───│              │              │            │
```

### 2.4.12 Диаграмма состояний опроса

```
[Черновик]
    │
    │ Сохранение
    ▼
[Сохранен]
    │
    │ Публикация
    ▼
[Опубликован] ────> [Снят с публикации]
    │                      │
    │                      │ Публикация
    │                      ▼
    │                 [Опубликован]
    │
    │ Удаление
    ▼
[Удален]
```

### 2.4.13 Диаграмма компонентов системы

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                    │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │
│  │   Views      │  │  JavaScript  │  │     CSS      │ │
│  │  (Razor)     │  │              │  │              │ │
│  └──────────────┘  └──────────────┘  └──────────────┘ │
└───────────────────────────┬─────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────┐
│                   Controller Layer                       │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │
│  │SurveyCtrl    │  │PublicCtrl    │  │AccountCtrl   │ │
│  └──────────────┘  └──────────────┘  └──────────────┘ │
└───────────────────────────┬─────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────┐
│                  Business Logic Layer                    │
│  ┌────────────────────────────────────────────────────┐ │
│  │            SurveyService                           │ │
│  │  - SaveSurveyAsync()                              │ │
│  │  - GetSurveyForEditAsync()                        │ │
│  │  - ChangePublicationStatusAsync()                 │ │
│  └────────────────────────────────────────────────────┘ │
└───────────────────────────┬─────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────┐
│                  Data Access Layer                       │
│  ┌────────────────────────────────────────────────────┐ │
│  │         ApplicationDbContext                       │ │
│  │  - Surveys (DbSet)                                 │ │
│  │  - Questions (DbSet)                               │ │
│  │  - AnswerOptions (DbSet)                           │ │
│  │  - SurveyResponses (DbSet)                         │ │
│  └────────────────────────────────────────────────────┘ │
└───────────────────────────┬─────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────┐
│                      Data Layer                          │
│  ┌────────────────────────────────────────────────────┐ │
│  │              SQL Server Database                    │ │
│  │  - Identity Schema (Users, Roles)                 │ │
│  │  - Surveys, Questions, AnswerOptions                │ │
│  │  - SurveyResponses, UserAnswers                    │ │
│  │  - Sections, Media                                 │ │
│  └────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
```

### 2.4.14 ER-диаграмма базы данных

```
┌──────────────┐
│ Application  │
│    User      │
│──────────────│
│ Id (PK)      │
│ Email        │
│ FirstName    │
│ LastName     │
└──────┬───────┘
       │ 1
       │
       │ N
┌──────▼───────┐         ┌──────────────┐
│   Survey     │         │   Question   │
│──────────────│         │──────────────│
│ Id (PK)      │◄───1:N──│ Id (PK)      │
│ CreatorId(FK) │         │ SurveyId(FK) │
│ Title        │         │ Text         │
│ Description  │         │ Type         │
│ IsPublished  │         │ Order        │
└──────┬───────┘         └──────┬───────┘
       │                        │ 1
       │ 1:N                    │
       │                        │ N
┌──────▼───────┐         ┌──────▼───────┐
│   Section    │         │ AnswerOption │
│──────────────│         │──────────────│
│ Id (PK)      │         │ Id (PK)      │
│ SurveyId(FK) │         │ QuestionId  │
│ Title        │         │ Text         │
│ Order        │         │ Order        │
└──────────────┘         │ IsOther      │
                         └──────────────┘
┌──────────────┐
│    Media     │
│──────────────│
│ Id (PK)      │
│ SurveyId(FK) │
│ Type         │
│ Url          │
│ Order        │
└──────────────┘

┌──────────────┐         ┌──────────────┐
│SurveyResponse│         │  UserAnswer  │
│──────────────│         │──────────────│
│ Id (PK)      │◄───1:N──│ Id (PK)      │
│ SurveyId(FK) │         │ ResponseId   │
│ UserId (FK)  │         │ QuestionId   │
│ SubmissionDt │         │ TextAnswer   │
└──────────────┘         │ SelectedOptId│
                         └──────────────┘
```

### 2.4.15 Описание основных контроллеров

#### SurveyController
**Ответственность:** Управление опросами пользователя

**Основные методы:**
- `Index()` - Список опросов пользователя
- `Edit(int id)` - Открытие конструктора опроса
- `Save()` - Сохранение опроса (AJAX)
- `Results(int id)` - Просмотр результатов
- `Preview(int id)` - Предпросмотр опроса
- `Publish(int id)` - Публикация опроса
- `Unpublish(int id)` - Снятие с публикации

#### PublicController
**Ответственность:** Публичный доступ к опросам

**Основные методы:**
- `ViewSurvey(int id)` - Отображение опроса для прохождения
- `SubmitResponse()` - Обработка отправки ответов
- `ThankYou()` - Страница благодарности

#### AccountController
**Ответственность:** Управление аккаунтом

**Основные методы:**
- `Login()` - Вход в систему
- `Register()` - Регистрация
- `Profile()` - Профиль пользователя
- `Settings()` - Настройки

#### AdminController
**Ответственность:** Административное управление

**Основные методы:**
- `Index()` - Панель администратора
- `Surveys()` - Управление опросами
- `UserManagement()` - Управление пользователями
- `TemplateManager()` - Управление шаблонами

### 2.4.16 Описание сервисов

#### SurveyService
**Интерфейс:** `ISurveyService`

**Методы:**
- `SaveSurveyAsync()` - Сохранение/обновление опроса с синхронизацией вопросов, вариантов, разделов и медиа
- `GetSurveyForEditAsync()` - Получение опроса для редактирования
- `ChangePublicationStatusAsync()` - Изменение статуса публикации
- `IsSurveyValidForPublishingAsync()` - Проверка готовности к публикации
- `DeleteSurveyAsync()` - Удаление опроса

**Особенности:**
- Сложная логика синхронизации при обновлении (добавление, изменение, удаление элементов)
- Каскадное удаление связанных сущностей
- Валидация данных перед сохранением

---

## Заключение

Система NovoyaHope Forms спроектирована с использованием современных технологий и паттернов проектирования. Архитектура обеспечивает масштабируемость, поддерживаемость и удобство использования. Интерфейс разработан с учетом принципов UX/UI дизайна и обеспечивает комфортную работу для всех категорий пользователей.

