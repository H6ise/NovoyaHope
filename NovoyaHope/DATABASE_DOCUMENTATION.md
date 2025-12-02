# Документация базы данных NovoyaHope

## Содержание

1. [Обзор](#обзор)
2. [Архитектура базы данных](#архитектура-базы-данных)
3. [Таблицы Identity](#таблицы-identity)
4. [Основные таблицы опросов](#основные-таблицы-опросов)
5. [Таблицы ответов пользователей](#таблицы-ответов-пользователей)
6. [Таблицы шаблонов](#таблицы-шаблонов)
7. [Вспомогательные таблицы](#вспомогательные-таблицы)
8. [Связи между таблицами](#связи-между-таблицами)
9. [Каскадные удаления](#каскадные-удаления)
10. [Диаграмма связей](#диаграмма-связей)

---

## Обзор

База данных проекта NovoyaHope предназначена для создания, управления и прохождения опросов. Система построена на ASP.NET Core Identity для аутентификации и авторизации пользователей, и использует Entity Framework Core для работы с данными.

### Технологии

- **СУБД**: Microsoft SQL Server
- **ORM**: Entity Framework Core 8.0
- **Аутентификация**: ASP.NET Core Identity
- **Схема Identity**: `Identity`

---

## Архитектура базы данных

База данных состоит из следующих групп таблиц:

1. **Таблицы Identity** - управление пользователями и ролями (схема `Identity`)
2. **Основные таблицы опросов** - Survey, Question, AnswerOption
3. **Таблицы ответов** - SurveyResponse, UserAnswer
4. **Таблицы шаблонов** - SurveyTemplate, TemplateQuestion, TemplateAnswerOption
5. **Вспомогательные таблицы** - Section, Media

---

## Таблицы Identity

### User (ApplicationUser)

Расширенная таблица пользователей на основе ASP.NET Core Identity.

**Расположение**: Схема `Identity`

| Поле | Тип | Описание |
|------|-----|----------|
| Id | string (PK) | Уникальный идентификатор пользователя |
| UserName | string | Имя пользователя |
| Email | string | Email адрес |
| EmailConfirmed | bool | Подтвержден ли email |
| PasswordHash | string | Хеш пароля |
| PhoneNumber | string? | Номер телефона (nullable) |
| PhoneNumberConfirmed | bool | Подтвержден ли телефон |
| TwoFactorEnabled | bool | Включена ли двухфакторная аутентификация |
| LockoutEnd | DateTimeOffset? | Дата окончания блокировки |
| LockoutEnabled | bool | Включена ли блокировка |
| AccessFailedCount | int | Количество неудачных попыток входа |
| **FirstName** | string? | Имя пользователя (дополнительное поле) |
| **LastName** | string? | Фамилия пользователя (дополнительное поле) |
| **ProfileImagePath** | string? | Путь к изображению профиля (дополнительное поле) |
| **ShowPhoneToPublic** | bool | Показывать ли телефон публично (дополнительное поле) |

**Связи**:
- Один ко многим: `CreatedSurveys` → Survey
- Один ко многим: `Responses` → SurveyResponse

### Role

Таблица ролей пользователей.

| Поле | Тип | Описание |
|------|-----|----------|
| Id | string (PK) | Уникальный идентификатор роли |
| Name | string | Название роли |
| NormalizedName | string | Нормализованное название роли |

### UserRoles

Связующая таблица пользователей и ролей (Many-to-Many).

| Поле | Тип | Описание |
|------|-----|----------|
| UserId | string (FK) | ID пользователя |
| RoleId | string (FK) | ID роли |

---

## Основные таблицы опросов

### Surveys

Основная таблица опросов.

| Поле | Тип | Описание |
|------|-----|----------|
| Id | int (PK, Identity) | Уникальный идентификатор опроса |
| Title | string (Required) | Заголовок опроса |
| Description | string (Required) | Описание опроса |
| Type | int | Тип опроса (1 = Poll, 2 = Questionnaire) |
| IsPublished | bool | Опубликован ли опрос (по умолчанию: false) |
| IsAnonymous | bool | Анонимный ли опрос (по умолчанию: true) |
| CreatedDate | DateTime | Дата создания опроса |
| EndDate | DateTime? | Дата окончания опроса (nullable) |
| **CreatorId** | string (FK, Required) | ID создателя опроса |
| **Настройки теста** | | |
| IsTestMode | bool | Режим теста (по умолчанию: false) |
| GradePublication | int | Когда публиковать оценку (1 = Сразу, 2 = После проверки) |
| ShowIncorrectAnswers | bool | Показывать неправильные ответы (по умолчанию: true) |
| ShowCorrectAnswers | bool | Показывать правильные ответы (по умолчанию: true) |
| ShowPoints | bool | Показывать баллы (по умолчанию: true) |
| DefaultMaxPoints | int | Максимальное количество баллов по умолчанию (по умолчанию: 0) |
| **Настройки темы** | | |
| ThemeColor | string? | Цвет темы (по умолчанию: "#673AB7") |
| BackgroundColor | string? | Цвет фона (по умолчанию: "#F3E5F5") |
| HeaderImagePath | string? | Путь к изображению заголовка |
| HeaderFontFamily | string? | Шрифт заголовка (по умолчанию: "Courier New") |
| HeaderFontSize | int? | Размер шрифта заголовка (по умолчанию: 24) |
| QuestionFontFamily | string? | Шрифт вопросов (по умолчанию: "Roboto") |
| QuestionFontSize | int? | Размер шрифта вопросов (по умолчанию: 12) |
| TextFontFamily | string? | Шрифт текста (по умолчанию: "Roboto") |
| TextFontSize | int? | Размер шрифта текста (по умолчанию: 11) |

**Индексы**:
- `CreatorId` - внешний ключ на `Identity.User(Id)`

**Связи**:
- Многие к одному: `Creator` → ApplicationUser (DeleteBehavior.Restrict)
- Один ко многим: `Questions` → Question
- Один ко многим: `Responses` → SurveyResponse
- Один ко многим: `Sections` → Section
- Один ко многим: `Media` → Media

**Enum: SurveyType**
- `Poll = 1` - Голосование (обычно один вопрос)
- `Questionnaire = 2` - Много вопросов

**Enum: GradePublicationType**
- `Immediately = 1` - Сразу после отправки формы
- `AfterManualReview = 2` - После ручной проверки

### Questions

Таблица вопросов опроса.

| Поле | Тип | Описание |
|------|-----|----------|
| Id | int (PK, Identity) | Уникальный идентификатор вопроса |
| SurveyId | int (FK, Required) | ID опроса |
| Text | string (Required) | Текст вопроса |
| Type | int | Тип вопроса (см. QuestionType enum) |
| IsRequired | bool | Обязательный ли вопрос |
| Order | int | Порядок отображения вопроса |

**Индексы**:
- `SurveyId` - внешний ключ на `Surveys(Id)`

**Связи**:
- Многие к одному: `Survey` → Survey (DeleteBehavior.Cascade)
- Один ко многим: `AnswerOptions` → AnswerOption
- Один ко многим: `Answers` → Answer

**Enum: QuestionType**
- `ShortText` - Текст (строка)
- `ParagraphText` - Текст (абзац)
- `SingleChoice` - Один из списка (радио-кнопки)
- `MultipleChoice` - Несколько из списка (чек-боксы)
- `Dropdown` - Раскрывающийся список
- `FileUpload` - Загрузка файлов
- `Scale` - Шкала (линейная)
- `Rating` - Оценка (звезды)
- `CheckboxGrid` - Сетка флажков
- `RadioGrid` - Сетка (множественный выбор)
- `Date` - Дата
- `Time` - Время

### AnswerOptions

Таблица вариантов ответов для вопросов с выбором.

| Поле | Тип | Описание |
|------|-----|----------|
| Id | int (PK, Identity) | Уникальный идентификатор варианта ответа |
| QuestionId | int (FK, Required) | ID вопроса |
| Text | string (Required) | Текст варианта ответа |
| Order | int | Порядок отображения варианта |
| IsOther | bool | Является ли вариант "Другое" |

**Индексы**:
- `QuestionId` - внешний ключ на `Questions(Id)`

**Связи**:
- Многие к одному: `Question` → Question (DeleteBehavior.Cascade)

---

## Таблицы ответов пользователей

### SurveyResponses

Таблица ответов на опросы (одна запись = один проход опроса).

| Поле | Тип | Описание |
|------|-----|----------|
| Id | int (PK, Identity) | Уникальный идентификатор ответа |
| SurveyId | int (FK, Required) | ID опроса |
| SubmissionDate | DateTime | Дата и время отправки ответа |
| UserId | string? (FK) | ID пользователя (nullable для анонимных опросов) |

**Индексы**:
- `SurveyId` - внешний ключ на `Surveys(Id)`
- `UserId` - внешний ключ на `Identity.User(Id)` (nullable)

**Связи**:
- Многие к одному: `Survey` → Survey (DeleteBehavior.Cascade)
- Многие к одному: `User` → ApplicationUser (nullable)
- Один ко многим: `UserAnswers` → UserAnswer

### UserAnswers

Таблица конкретных ответов на вопросы (в рамках одного прохода опроса).

| Поле | Тип | Описание |
|------|-----|----------|
| Id | int (PK, Identity) | Уникальный идентификатор ответа |
| ResponseId | int (FK, Required) | ID ответа на опрос (SurveyResponse) |
| QuestionId | int (FK, Required) | ID вопроса |
| TextAnswer | string? (Nullable) | Текстовый ответ (для ShortText, ParagraphText) |
| SelectedOptionId | int? (FK, Nullable) | ID выбранного варианта ответа (для SingleChoice, Scale) |

**Индексы**:
- `ResponseId` - внешний ключ на `SurveyResponses(Id)`
- `QuestionId` - внешний ключ на `Questions(Id)`
- `SelectedOptionId` - внешний ключ на `AnswerOptions(Id)` (nullable)

**Связи**:
- Многие к одному: `Response` → SurveyResponse
- Многие к одному: `Question` → Question
- Многие к одному: `SelectedOption` → AnswerOption (nullable)

**Примечание**: 
- Для текстовых вопросов заполняется `TextAnswer`
- Для вопросов с выбором заполняется `SelectedOptionId`
- Для множественного выбора (MultipleChoice) создается несколько записей UserAnswer с одним `ResponseId` и `QuestionId`, но разными `SelectedOptionId`

---

## Таблицы шаблонов

### SurveyTemplates

Таблица шаблонов опросов.

| Поле | Тип | Описание |
|------|-----|----------|
| Id | int (PK, Identity) | Уникальный идентификатор шаблона |
| Title | string (Required) | Заголовок шаблона |
| Description | string (Required) | Описание шаблона |
| Type | int | Тип опроса (SurveyType) |
| CreatorId | string (Required) | ID создателя шаблона |

**Связи**:
- Один ко многим: `Questions` → TemplateQuestion

### TemplateQuestions

Таблица вопросов шаблона.

| Поле | Тип | Описание |
|------|-----|----------|
| Id | int (PK, Identity) | Уникальный идентификатор вопроса |
| SurveyTemplateId | int (FK, Required) | ID шаблона опроса |
| Order | int | Порядок отображения |
| Text | string (Required) | Текст вопроса |
| Type | int | Тип вопроса (QuestionType) |

**Индексы**:
- `SurveyTemplateId` - внешний ключ на `SurveyTemplates(Id)`

**Связи**:
- Многие к одному: `SurveyTemplate` → SurveyTemplate
- Один ко многим: `AnswerOptions` → TemplateAnswerOption

### TemplateAnswerOptions

Таблица вариантов ответов для вопросов шаблона.

| Поле | Тип | Описание |
|------|-----|----------|
| Id | int (PK, Identity) | Уникальный идентификатор варианта |
| QuestionId | int (FK, Required) | ID вопроса шаблона |
| Text | string (Required) | Текст варианта ответа |
| Order | int | Порядок отображения |

**Индексы**:
- `QuestionId` - внешний ключ на `TemplateQuestions(Id)`

**Связи**:
- Многие к одному: `Question` → TemplateQuestion

---

## Вспомогательные таблицы

### Sections

Таблица разделов опроса для группировки вопросов.

| Поле | Тип | Описание |
|------|-----|----------|
| Id | int (PK, Identity) | Уникальный идентификатор раздела |
| SurveyId | int (FK, Required) | ID опроса |
| Title | string (Required, MaxLength: 200) | Заголовок раздела |
| Description | string? (MaxLength: 1000) | Описание раздела |
| Order | int | Порядок отображения раздела |

**Индексы**:
- `SurveyId` - внешний ключ на `Surveys(Id)`

**Связи**:
- Многие к одному: `Survey` → Survey (DeleteBehavior.Cascade)

### Media

Таблица медиа-элементов (изображения и видео) в опросе.

| Поле | Тип | Описание |
|------|-----|----------|
| Id | int (PK, Identity) | Уникальный идентификатор медиа |
| SurveyId | int (FK, Required) | ID опроса |
| Type | int | Тип медиа (MediaType enum) |
| Url | string (Required, MaxLength: 500) | URL или путь к файлу |
| Title | string? (MaxLength: 200) | Заголовок/альтернативный текст |
| Description | string? (MaxLength: 1000) | Описание медиа |
| Order | int | Порядок отображения |
| QuestionId | int? (FK, Nullable) | ID вопроса, к которому привязан медиа (nullable) |

**Индексы**:
- `SurveyId` - внешний ключ на `Surveys(Id)`
- `QuestionId` - внешний ключ на `Questions(Id)` (nullable)

**Связи**:
- Многие к одному: `Survey` → Survey (DeleteBehavior.Cascade)
- Многие к одному: `Question` → Question (nullable, DeleteBehavior.SetNull)

**Enum: MediaType**
- `Image = 0` - Изображение
- `Video = 1` - Видео

---

## Связи между таблицами

### Иерархия основных таблиц

```
ApplicationUser (Identity.User)
    └── Surveys (1:N)
        ├── Questions (1:N)
        │   └── AnswerOptions (1:N)
        ├── SurveyResponses (1:N)
        │   └── UserAnswers (1:N)
        │       └── AnswerOption (N:1) [для выбранных вариантов]
        ├── Sections (1:N)
        └── Media (1:N)
            └── Question (N:1) [опционально]
```

### Детальное описание связей

1. **ApplicationUser ↔ Surveys**
   - Тип: Один ко многим
   - Связь: `CreatorId` → `User.Id`
   - Удаление: Restrict (нельзя удалить пользователя, если у него есть опросы)

2. **Survey ↔ Questions**
   - Тип: Один ко многим
   - Связь: `SurveyId` → `Survey.Id`
   - Удаление: Cascade (при удалении опроса удаляются все вопросы)

3. **Question ↔ AnswerOptions**
   - Тип: Один ко многим
   - Связь: `QuestionId` → `Question.Id`
   - Удаление: Cascade (при удалении вопроса удаляются все варианты ответов)

4. **Survey ↔ SurveyResponses**
   - Тип: Один ко многим
   - Связь: `SurveyId` → `Survey.Id`
   - Удаление: Cascade (при удалении опроса удаляются все ответы)

5. **ApplicationUser ↔ SurveyResponses**
   - Тип: Один ко многим (опционально)
   - Связь: `UserId` → `User.Id` (nullable)
   - Удаление: Restrict

6. **SurveyResponse ↔ UserAnswers**
   - Тип: Один ко многим
   - Связь: `ResponseId` → `SurveyResponse.Id`
   - Удаление: Cascade

7. **Question ↔ UserAnswers**
   - Тип: Один ко многим
   - Связь: `QuestionId` → `Question.Id`
   - Удаление: Cascade

8. **AnswerOption ↔ UserAnswers**
   - Тип: Один ко многим
   - Связь: `SelectedOptionId` → `AnswerOption.Id` (nullable)
   - Удаление: SetNull

9. **Survey ↔ Sections**
   - Тип: Один ко многим
   - Связь: `SurveyId` → `Survey.Id`
   - Удаление: Cascade

10. **Survey ↔ Media**
    - Тип: Один ко многим
    - Связь: `SurveyId` → `Survey.Id`
    - Удаление: Cascade

11. **Question ↔ Media**
    - Тип: Многие к одному (опционально)
    - Связь: `QuestionId` → `Question.Id` (nullable)
    - Удаление: SetNull (при удалении вопроса медиа остается, но связь удаляется)

---

## Каскадные удаления

### Полная иерархия каскадных удалений

```
Survey
    ├── Questions (Cascade)
    │   └── AnswerOptions (Cascade)
    ├── SurveyResponses (Cascade)
    │   └── UserAnswers (Cascade)
    ├── Sections (Cascade)
    └── Media (Cascade)
        └── QuestionId → SetNull (не удаляется, просто обнуляется связь)
```

### Правила каскадного удаления

1. **При удалении Survey**:
   - ✅ Удаляются все Questions
   - ✅ Удаляются все SurveyResponses
   - ✅ Удаляются все Sections
   - ✅ Удаляются все Media

2. **При удалении Question**:
   - ✅ Удаляются все AnswerOptions
   - ✅ Удаляются все UserAnswers на этот вопрос
   - ⚠️ Media, привязанные к вопросу, остаются, но `QuestionId` становится NULL

3. **При удалении SurveyResponse**:
   - ✅ Удаляются все UserAnswers этого ответа

4. **При удалении AnswerOption**:
   - ⚠️ UserAnswers, ссылающиеся на этот вариант, остаются, но `SelectedOptionId` становится NULL

5. **При удалении ApplicationUser**:
   - ❌ Нельзя удалить, если есть Surveys с этим CreatorId (Restrict)

---

## Диаграмма связей

### Текстовая ER-диаграмма

```
┌─────────────────────────────────────────────────────────┐
│                    APPLICATION USER                      │
│                  (Identity Schema)                       │
│  ┌────────────┐                                         │
│  │ Id (PK)    │                                         │
│  │ UserName   │                                         │
│  │ Email      │                                         │
│  │ FirstName  │                                         │
│  │ LastName   │                                         │
│  │ ...        │                                         │
│  └────────────┘                                         │
└─────────────────────────────────────────────────────────┘
         │                        │
         │ (1:N)                 │ (1:N)
         │ CreatorId             │ UserId
         │                       │
         ▼                       ▼
┌────────────────────┐  ┌──────────────────────┐
│      SURVEYS       │  │  SURVEY RESPONSES    │
│  ┌──────────────┐  │  │  ┌────────────────┐  │
│  │ Id (PK)      │  │  │  │ Id (PK)        │  │
│  │ Title        │  │  │  │ SurveyId (FK)  │  │
│  │ Description  │  │  │  │ SubmissionDate │  │
│  │ CreatorId(FK)│──┼──┼──│ UserId (FK)    │  │
│  │ Type         │  │  │  └────────────────┘  │
│  │ IsPublished  │  │  └──────────────────────┘
│  │ ...          │  │              │
│  └──────────────┘  │              │ (1:N)
         │           │              │
         │ (1:N)     │              ▼
         │           │  ┌──────────────────────┐
         ▼           │  │    USER ANSWERS      │
┌────────────────┐  │  │  ┌────────────────┐  │
│    QUESTIONS   │  │  │  │ Id (PK)        │  │
│  ┌──────────┐  │  │  │  │ ResponseId (FK)│  │
│  │ Id (PK)  │  │  │  │  │ QuestionId(FK) │  │
│  │SurveyId  │──┼──┘  │  │ TextAnswer     │  │
│  │ Text     │  │     │  │SelectedOptId(FK)│ │
│  │ Type     │  │     │  └────────────────┘  │
│  │ Order    │  │     └──────────────────────┘
│  └──────────┘  │
         │       │
         │ (1:N) │
         │       │
         ▼       │
┌──────────────┐ │
│ANSWER OPTIONS│ │
│  ┌────────┐  │ │
│  │ Id(PK) │  │ │
│  │QuestId │──┼─┘
│  │ Text   │  │
│  │ Order  │  │
│  └────────┘  │
└──────────────┘
         │
         │ (N:1) SelectedOptionId
         │
         └──────────┘ (связь с UserAnswers для выбранных вариантов)
```

### Схема основных связей

```
ApplicationUser
    ├── CreatedSurveys (1:N)
    │   └── Surveys
    │       ├── Questions (1:N)
    │       │   └── AnswerOptions (1:N)
    │       ├── SurveyResponses (1:N)
    │       ├── Sections (1:N)
    │       └── Media (1:N)
    │
    └── Responses (1:N)
        └── SurveyResponses
            └── UserAnswers (1:N)
                ├── Question (N:1)
                └── SelectedOption → AnswerOption (N:1)
```

---

## Примечания

### Важные особенности

1. **Анонимные опросы**: В таблице `SurveyResponses` поле `UserId` может быть NULL для анонимных опросов.

2. **Множественный выбор**: Для вопросов типа `MultipleChoice` создается несколько записей в `UserAnswers` с одним `ResponseId` и `QuestionId`, но разными `SelectedOptionId`.

3. **Текстовые ответы**: Для вопросов типа `ShortText` и `ParagraphText` заполняется поле `TextAnswer`, а `SelectedOptionId` остается NULL.

4. **Вариант "Другое"**: Поле `IsOther` в `AnswerOptions` позволяет отметить вариант как "Другое", что может использоваться в UI для предоставления пользователю возможности ввести свой вариант.

5. **Шаблоны**: Таблицы `SurveyTemplate`, `TemplateQuestion`, `TemplateAnswerOption` являются независимыми от основных таблиц опросов и используются для создания новых опросов на основе шаблонов.

6. **Медиа**: Медиа-элементы могут быть привязаны к опросу в целом или к конкретному вопросу через поле `QuestionId`.

### Рекомендации по производительности

1. **Индексы**: Рекомендуется создать индексы на:
   - `Surveys.CreatorId`
   - `Surveys.IsPublished`
   - `Questions.SurveyId`
   - `SurveyResponses.SurveyId`
   - `SurveyResponses.UserId`
   - `UserAnswers.ResponseId`
   - `UserAnswers.QuestionId`

2. **Каскадные удаления**: Учитывайте, что удаление опроса приведет к удалению всех связанных данных. При необходимости сохранения истории используйте мягкое удаление (Soft Delete).

3. **Архивация**: Рассмотрите возможность архивации старых ответов для повышения производительности.

---

## Версия документации

- **Версия**: 1.0
- **Дата**: 2025
- **Автор**: NovoyaHope Development Team
- **Последнее обновление**: Соответствует коду на момент создания документации

---

## Изменения

### Версия 1.0 (2025)
- Первоначальная версия документации
- Описание всех таблиц и связей
- Документация каскадных удалений
- Диаграммы связей

