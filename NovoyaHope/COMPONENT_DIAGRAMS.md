# Диаграммы компонентов - NovoyaHope

## Содержание

1. [Общая архитектура компонентов](#общая-архитектура-компонентов)
2. [Детальная диаграмма компонентов](#детальная-диаграмма-компонентов)
3. [Диаграмма слоев архитектуры](#диаграмма-слоев-архитектуры)
4. [Диаграмма взаимодействия контроллеров](#диаграмма-взаимодействия-контроллеров)
5. [Диаграмма потока данных](#диаграмма-потока-данных)
6. [Диаграмма зависимостей](#диаграмма-зависимостей)
7. [Диаграмма базы данных](#диаграмма-базы-данных)

---

## Общая архитектура компонентов

```mermaid
graph TB
    subgraph "Client Layer"
        Browser[Браузер пользователя]
        StaticFiles[Статические файлы<br/>CSS, JS, изображения]
    end

    subgraph "Presentation Layer"
        Controllers[Контроллеры]
        Views[Razor Views]
    end

    subgraph "Business Layer"
        Services[Сервисы<br/>SurveyService]
        Helpers[Вспомогательные классы<br/>ImageHelper, ValidationHelper]
    end

    subgraph "Data Layer"
        DbContext[ApplicationDbContext]
        DbInitializer[DbInitializer]
    end

    subgraph "Infrastructure"
        Identity[ASP.NET Core Identity]
        EF[Entity Framework Core]
        DB[(SQL Server<br/>База данных)]
        FileSystem[Файловая система<br/>uploads/]
    end

    Browser --> Controllers
    StaticFiles --> Browser
    Controllers --> Views
    Controllers --> Services
    Controllers --> Helpers
    Controllers --> Identity
    Services --> DbContext
    Helpers --> FileSystem
    DbContext --> EF
    EF --> DB
    DbInitializer --> Identity
    DbInitializer --> DB
    Identity --> DB
```

---

## Детальная диаграмма компонентов

```mermaid
graph TB
    subgraph "Controllers"
        AccountCtrl[AccountController<br/>Аутентификация, профили]
        SurveyCtrl[SurveyController<br/>Управление опросами]
        PublicCtrl[PublicController<br/>Публичный доступ]
        AdminCtrl[AdminController<br/>Администрирование]
        HomeCtrl[HomeController<br/>Публичные страницы]
    end

    subgraph "Services"
        ISurveyService[ISurveyService<br/>Интерфейс]
        SurveyService[SurveyService<br/>Бизнес-логика опросов]
    end

    subgraph "Helpers"
        ImageHelper[ImageHelper<br/>Работа с изображениями]
        ValidationHelper[ValidationHelper<br/>Валидация данных]
    end

    subgraph "Data"
        DbContext[ApplicationDbContext<br/>Контекст БД]
        DbInitializer[DbInitializer<br/>Инициализация]
    end

    subgraph "Models"
        Entities[Entity Models<br/>Survey, Question, etc.]
        ViewModels[ViewModels<br/>DTO для Views]
    end

    subgraph "Identity"
        UserManager[UserManager]
        SignInManager[SignInManager]
        RoleManager[RoleManager]
    end

    subgraph "External"
        DB[(SQL Server)]
        FileSystem[Файловая система]
    end

    AccountCtrl --> UserManager
    AccountCtrl --> SignInManager
    AccountCtrl --> RoleManager
    AccountCtrl --> ImageHelper
    AccountCtrl --> ValidationHelper
    AccountCtrl --> DbContext

    SurveyCtrl --> ISurveyService
    SurveyCtrl --> ImageHelper
    SurveyCtrl --> DbContext
    SurveyCtrl --> ViewModels

    PublicCtrl --> DbContext
    PublicCtrl --> ViewModels

    AdminCtrl --> UserManager
    AdminCtrl --> RoleManager
    AdminCtrl --> DbContext

    HomeCtrl --> DbContext

    ISurveyService -.->|реализует| SurveyService
    SurveyService --> DbContext
    SurveyService --> Entities

    ImageHelper --> FileSystem
    ValidationHelper --> ValidationHelper

    DbContext --> Entities
    DbContext --> DB
    DbInitializer --> UserManager
    DbInitializer --> RoleManager
    DbInitializer --> DB

    UserManager --> DB
    SignInManager --> DB
    RoleManager --> DB
```

---

## Диаграмма слоев архитектуры

```mermaid
graph TD
    subgraph "Слой представления (Presentation Layer)"
        direction TB
        HTTP[HTTP Requests/Responses]
        Controllers[Controllers<br/>5 контроллеров]
        Views[Views<br/>Razor Pages]
        ViewModels[ViewModels<br/>DTO Objects]
        
        HTTP --> Controllers
        Controllers --> Views
        Controllers --> ViewModels
        Views --> ViewModels
    end

    subgraph "Слой бизнес-логики (Business Layer)"
        direction TB
        Services[Services<br/>SurveyService]
        BusinessRules[Бизнес-правила<br/>Валидация, синхронизация]
        Helpers[Helpers<br/>ImageHelper, ValidationHelper]
        
        Services --> BusinessRules
        Services --> Helpers
    end

    subgraph "Слой доступа к данным (Data Access Layer)"
        direction TB
        DbContext[ApplicationDbContext<br/>EF Core Context]
        Repositories[DbSet<br/>Репозитории сущностей]
        Migrations[Migrations<br/>Схема БД]
        
        DbContext --> Repositories
        DbContext --> Migrations
    end

    subgraph "Инфраструктурный слой (Infrastructure Layer)"
        direction TB
        Identity[ASP.NET Core Identity]
        EF[Entity Framework Core]
        FileSystem[Файловая система]
        DB[(SQL Server<br/>База данных)]
        
        Identity --> DB
        EF --> DB
    end

    Controllers --> Services
    Controllers --> DbContext
    Services --> DbContext
    DbContext --> EF
    Helpers --> FileSystem
    Controllers --> Identity
```

---

## Диаграмма взаимодействия контроллеров

```mermaid
sequenceDiagram
    participant User as Пользователь
    participant Browser as Браузер
    participant Account as AccountController
    participant Survey as SurveyController
    participant Public as PublicController
    participant Admin as AdminController
    participant Service as SurveyService
    participant DB as ApplicationDbContext
    participant Identity as Identity
    participant Helper as Helpers

    Note over User,Helper: Сценарий 1: Регистрация и создание опроса
    
    User->>Browser: Регистрация
    Browser->>Account: POST /Account/Register
    Account->>Identity: UserManager.CreateAsync()
    Identity->>DB: Сохранение пользователя
    Account->>Browser: Redirect to Home
    Browser->>User: Отображение главной страницы

    User->>Browser: Создание опроса
    Browser->>Survey: GET /Survey/Edit
    Survey->>DB: Получение опросов пользователя
    DB-->>Survey: Список опросов
    Survey->>Browser: View с конструктором
    Browser->>User: Отображение конструктора

    User->>Browser: Сохранение опроса (AJAX)
    Browser->>Survey: POST /api/surveys/save
    Survey->>Service: SaveSurveyAsync()
    Service->>DB: Синхронизация данных
    DB-->>Service: Успех
    Service-->>Survey: ID опроса
    Survey-->>Browser: JSON ответ
    Browser->>User: Уведомление об успехе

    Note over User,Helper: Сценарий 2: Прохождение опроса
    
    User->>Browser: Переход по ссылке опроса
    Browser->>Public: GET /Public/ViewSurvey/{id}
    Public->>DB: Загрузка опроса
    DB-->>Public: Данные опроса
    Public->>Browser: View с формой опроса
    Browser->>User: Отображение опроса

    User->>Browser: Отправка ответов
    Browser->>Public: POST /Public/SubmitResponse
    Public->>DB: Валидация и сохранение
    DB-->>Public: Успех
    Public->>Browser: Redirect to ThankYou
    Browser->>User: Страница благодарности

    Note over User,Helper: Сценарий 3: Просмотр результатов
    
    User->>Browser: Просмотр результатов
    Browser->>Survey: GET /Survey/Results/{id}
    Survey->>DB: Загрузка опроса с ответами
    DB-->>Survey: Данные с ответами
    Survey->>Survey: Агрегация результатов
    Survey->>Browser: View с результатами
    Browser->>User: Отображение результатов
```

---

## Диаграмма потока данных

```mermaid
flowchart LR
    subgraph "Входные данные"
        HTTPRequest[HTTP Request]
        FormData[Данные формы]
        FileUpload[Загруженные файлы]
    end

    subgraph "Обработка"
        Validation{Валидация}
        Auth{Авторизация}
        BusinessLogic[Бизнес-логика]
        DataTransform[Преобразование данных]
    end

    subgraph "Хранение"
        Database[(База данных)]
        FileSystem[(Файловая система)]
    end

    subgraph "Выходные данные"
        ViewModel[ViewModel]
        HTML[HTML страница]
        JSON[JSON ответ]
        File[Файл для скачивания]
    end

    HTTPRequest --> Validation
    FormData --> Validation
    FileUpload --> Validation

    Validation -->|Валидно| Auth
    Validation -->|Невалидно| HTML

    Auth -->|Авторизован| BusinessLogic
    Auth -->|Не авторизован| HTML

    BusinessLogic --> DataTransform
    DataTransform --> Database
    DataTransform --> FileSystem

    Database --> ViewModel
    FileSystem --> ViewModel
    ViewModel --> HTML
    ViewModel --> JSON
    ViewModel --> File

    HTML --> HTTPResponse[HTTP Response]
    JSON --> HTTPResponse
    File --> HTTPResponse
```

---

## Диаграмма зависимостей

```mermaid
graph LR
    subgraph "Внешние зависимости"
        ASPNET[ASP.NET Core 8.0]
        EFCore[Entity Framework Core 8.0]
        Identity[ASP.NET Core Identity]
        Bootstrap[Bootstrap 5]
        jQuery[jQuery]
    end

    subgraph "Проект NovoyaHope"
        Program[Program.cs<br/>Конфигурация]
        
        subgraph "Controllers"
            AccountCtrl[AccountController]
            SurveyCtrl[SurveyController]
            PublicCtrl[PublicController]
            AdminCtrl[AdminController]
            HomeCtrl[HomeController]
        end

        subgraph "Services"
            SurveyService[SurveyService]
        end

        subgraph "Helpers"
            ImageHelper[ImageHelper]
            ValidationHelper[ValidationHelper]
        end

        subgraph "Data"
            DbContext[ApplicationDbContext]
            DbInitializer[DbInitializer]
        end

        subgraph "Models"
            Entities[Entity Models]
            ViewModels[ViewModels]
        end
    end

    ASPNET --> Program
    ASPNET --> AccountCtrl
    ASPNET --> SurveyCtrl
    ASPNET --> PublicCtrl
    ASPNET --> AdminCtrl
    ASPNET --> HomeCtrl

    Identity --> AccountCtrl
    Identity --> DbContext
    Identity --> DbInitializer

    EFCore --> DbContext
    EFCore --> DbInitializer

    Program --> AccountCtrl
    Program --> SurveyCtrl
    Program --> DbContext
    Program --> SurveyService

    AccountCtrl --> ImageHelper
    AccountCtrl --> ValidationHelper
    AccountCtrl --> Identity
    AccountCtrl --> ViewModels

    SurveyCtrl --> SurveyService
    SurveyCtrl --> ImageHelper
    SurveyCtrl --> ViewModels

    PublicCtrl --> ViewModels
    AdminCtrl --> Identity

    SurveyService --> DbContext
    SurveyService --> Entities

    DbContext --> Entities
    DbContext --> EFCore
    DbContext --> Identity

    ImageHelper --> ASPNET
    ValidationHelper --> ValidationHelper

    Bootstrap --> Views[Views]
    jQuery --> Views
```

---

## Диаграмма базы данных

```mermaid
erDiagram
    ApplicationUser ||--o{ Survey : creates
    ApplicationUser ||--o{ SurveyResponse : submits
    ApplicationUser ||--o{ IdentityUserRole : has
    
    Survey ||--o{ Question : contains
    Survey ||--o{ SurveyResponse : receives
    Survey ||--o{ Section : has
    Survey ||--o{ Media : includes
    Survey ||--o{ SurveyTemplate : "can create from"
    
    Question ||--o{ AnswerOption : has
    Question ||--o{ UserAnswer : receives
    Question ||--o{ Media : "can have"
    
    SurveyResponse ||--o{ UserAnswer : contains
    
    UserAnswer }o--|| AnswerOption : "may reference"
    
    SurveyTemplate ||--o{ TemplateQuestion : contains
    TemplateQuestion ||--o{ TemplateAnswerOption : has
    
    IdentityRole ||--o{ IdentityUserRole : assigned
    
    ApplicationUser {
        string Id PK
        string UserName
        string Email
        string FirstName
        string LastName
        string PhoneNumber
        string ProfileImageUrl
    }
    
    Survey {
        int Id PK
        string CreatorId FK
        string Title
        string Description
        bool IsPublished
        bool IsAnonymous
        DateTime CreatedDate
    }
    
    Question {
        int Id PK
        int SurveyId FK
        string Text
        QuestionType Type
        int Order
        bool IsRequired
    }
    
    AnswerOption {
        int Id PK
        int QuestionId FK
        string Text
        int Order
        bool IsOther
    }
    
    SurveyResponse {
        int Id PK
        int SurveyId FK
        string UserId FK
        DateTime SubmissionDate
    }
    
    UserAnswer {
        int Id PK
        int ResponseId FK
        int QuestionId FK
        int SelectedOptionId FK
        string TextAnswer
    }
    
    Section {
        int Id PK
        int SurveyId FK
        string Title
        int Order
    }
    
    Media {
        int Id PK
        int SurveyId FK
        int QuestionId FK
        string Url
        MediaType Type
    }
    
    SurveyTemplate {
        int Id PK
        string Name
        string Description
    }
```

---

## Диаграмма жизненного цикла запроса

```mermaid
flowchart TD
    Start([HTTP Request]) --> Middleware[Middleware Pipeline]
    
    Middleware --> Exception{Development?}
    Exception -->|Yes| DevPage[Developer Exception Page]
    Exception -->|No| ErrorHandler[Exception Handler]
    
    DevPage --> HTTPS[HTTPS Redirection]
    ErrorHandler --> HTTPS
    
    HTTPS --> Static[Static Files]
    Static --> Routing[Routing]
    
    Routing --> Auth[Authentication]
    Auth --> Authorize{Authorized?}
    
    Authorize -->|No| LoginPage[Redirect to Login]
    Authorize -->|Yes| Controller[Controller Action]
    
    Controller --> Validation{Model Valid?}
    Validation -->|No| ReturnView[Return View with Errors]
    Validation -->|Yes| Service[Call Service]
    
    Service --> BusinessLogic[Business Logic]
    BusinessLogic --> DbContext[DbContext]
    DbContext --> Database[(Database)]
    
    Database --> DbContext
    DbContext --> Service
    Service --> Controller
    
    Controller --> View[Render View]
    View --> Response([HTTP Response])
    
    LoginPage --> Response
    ReturnView --> Response
```

---

## Диаграмма компонентов с интерфейсами

```mermaid
classDiagram
    class ISurveyService {
        <<interface>>
        +SaveSurveyAsync()
        +GetSurveyForEditAsync()
        +ChangePublicationStatusAsync()
        +DeleteSurveyAsync()
    }
    
    class SurveyService {
        -ApplicationDbContext _context
        +SaveSurveyAsync()
        +GetSurveyForEditAsync()
        +ChangePublicationStatusAsync()
        +DeleteSurveyAsync()
    }
    
    class AccountController {
        -UserManager userManager
        -SignInManager signInManager
        -IWebHostEnvironment env
        +Login()
        +Register()
        +Profile()
        +ChangePassword()
    }
    
    class SurveyController {
        -ApplicationDbContext _context
        -ISurveyService _surveyService
        -IWebHostEnvironment _env
        +Index()
        +Edit()
        +SaveSurvey()
        +Publish()
        +Results()
        +Delete()
    }
    
    class PublicController {
        -ApplicationDbContext _context
        -UserManager userManager
        +ViewSurvey()
        +SubmitResponse()
    }
    
    class ApplicationDbContext {
        +DbSet~Survey~ Surveys
        +DbSet~Question~ Questions
        +DbSet~AnswerOption~ AnswerOptions
        +OnModelCreating()
    }
    
    class ImageHelper {
        <<static>>
        +IsValidImage()
        +SaveProfileImageAsync()
        +SaveMediaImageAsync()
        +DeleteProfileImage()
    }
    
    class ValidationHelper {
        <<static>>
        +IsValidEmail()
        +IsValidPhone()
        +ValidatePasswordStrength()
        +SanitizeString()
    }
    
    ISurveyService <|.. SurveyService : implements
    SurveyController --> ISurveyService : uses
    SurveyController --> ApplicationDbContext : uses
    SurveyController --> ImageHelper : uses
    AccountController --> ImageHelper : uses
    AccountController --> ValidationHelper : uses
    SurveyService --> ApplicationDbContext : uses
```

---

## Диаграмма развертывания

```mermaid
graph TB
    subgraph "Клиентское устройство"
        Browser[Веб-браузер]
        Mobile[Мобильное устройство]
    end

    subgraph "Сеть"
        Internet[Internet/Intranet]
        HTTPS[HTTPS Connection]
    end

    subgraph "Веб-сервер"
        IIS[IIS / Kestrel Server]
        StaticFiles[Static Files Handler]
        ASPNET[ASP.NET Core Runtime]
    end

    subgraph "Приложение NovoyaHope"
        Controllers[Controllers]
        Services[Services]
        Middleware[Middleware Pipeline]
    end

    subgraph "Инфраструктура"
        Identity[Identity Services]
        EF[EF Core]
        FileSystem[File System<br/>wwwroot/uploads/]
    end

    subgraph "База данных"
        SQLServer[(SQL Server<br/>NovoyaHopeDb)]
    end

    Browser --> Internet
    Mobile --> Internet
    Internet --> HTTPS
    HTTPS --> IIS
    IIS --> StaticFiles
    IIS --> ASPNET
    ASPNET --> Middleware
    Middleware --> Controllers
    Controllers --> Services
    Services --> Identity
    Services --> EF
    Controllers --> FileSystem
    EF --> SQLServer
    Identity --> SQLServer
```

---

## Диаграмма потоков данных для создания опроса

```mermaid
flowchart TD
    Start([Пользователь создает опрос]) --> LoadView[Загрузка Edit View]
    LoadView --> DisplayConstructor[Отображение конструктора]
    
    DisplayConstructor --> UserInput[Ввод данных в конструкторе]
    UserInput --> JS[JavaScript constructor.js]
    
    JS --> BuildModel[Построение ViewModel]
    BuildModel --> AJAX[AJAX запрос /api/surveys/save]
    
    AJAX --> Controller[SurveyController.SaveSurvey]
    Controller --> Validate{Валидация}
    
    Validate -->|Ошибка| ErrorResponse[JSON Error]
    Validate -->|Успех| Service[SurveyService.SaveSurveyAsync]
    
    Service --> Sync[Синхронизация данных]
    Sync --> Questions[Обработка вопросов]
    Sync --> Options[Обработка вариантов]
    Sync --> Sections[Обработка разделов]
    Sync --> Media[Обработка медиа]
    
    Questions --> DbContext[ApplicationDbContext]
    Options --> DbContext
    Sections --> DbContext
    Media --> ImageHelper
    ImageHelper --> FileSystem[Сохранение файлов]
    ImageHelper --> DbContext
    
    DbContext --> SaveChanges[SaveChangesAsync]
    SaveChanges --> Transaction{Успех?}
    
    Transaction -->|Ошибка| Rollback[Откат транзакции]
    Transaction -->|Успех| Success[JSON Success + Survey ID]
    
    Rollback --> ErrorResponse
    Success --> JS
    ErrorResponse --> JS
    
    JS --> UpdateUI[Обновление UI]
    UpdateUI --> End([Опрос сохранен])
    ```

---

## Условные обозначения диаграмм

### Стрелки и соединения

- `-->` - Направленная зависимость
- `-.->` - Реализация интерфейса
- `-->|метка|` - Аннотированная зависимость
- `||--o{` - Связь один-ко-многим (ER-диаграммы)
- `}o--||` - Связь многие-к-одному (ER-диаграммы)

### Фигуры

- **Прямоугольник** - Компонент/класс
- **Прямоугольник с двойными границами** - Интерфейс
- **Ромб** - Условие/решение
- **Цилиндр** - База данных
- **Овал** - Начало/конец процесса
- **Параллелограмм** - Ввод/вывод данных

### Цвета (в визуализаторах)

- **Синий** - Контроллеры
- **Зеленый** - Сервисы
- **Оранжевый** - Helpers
- **Фиолетовый** - Data/Models
- **Красный** - Внешние зависимости

---

## Инструменты для просмотра диаграмм

Эти диаграммы написаны на языке **Mermaid**, который поддерживается:

1. **GitHub/GitLab** - автоматически рендерят в README.md
2. **Visual Studio Code** - через расширение "Markdown Preview Mermaid Support"
3. **Онлайн редакторы**:
   - [Mermaid Live Editor](https://mermaid.live/)
   - [Mermaid.ink](https://mermaid.ink/)
4. **Другие Markdown редакторы** - Typora, Obsidian, и т.д.

### Альтернативные форматы

Для создания изображений из Mermaid диаграмм:

```bash
# Установка Mermaid CLI
npm install -g @mermaid-js/mermaid-cli

# Генерация PNG
mmdc -i COMPONENT_DIAGRAMS.md -o diagrams.png

# Генерация SVG
mmdc -i COMPONENT_DIAGRAMS.md -o diagrams.svg
```

---

## Версия документации

- **Версия**: 1.0
- **Дата**: 2025
- **Автор**: NovoyaHope Development Team
- **Формат диаграмм**: Mermaid
- **Последнее обновление**: Соответствует коду на момент создания документации

---

## Связанная документация

- `PROJECT_STRUCTURE_DOCUMENTATION.md` - Структура проекта
- `SERVER_LOGIC_DOCUMENTATION.md` - Серверная логика
- `DATABASE_DOCUMENTATION.md` - База данных
- `SYSTEM_DESIGN.md` - Дизайн системы

