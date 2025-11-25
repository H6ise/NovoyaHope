document.addEventListener('DOMContentLoaded', () => {
    console.log('Constructor JavaScript loaded successfully.');

    const questionsContainer = document.getElementById('questions-container');
    const sidebar = document.querySelector('.sidebar-tools');
    const editorHeader = document.querySelector('.editor-header');

    if (!questionsContainer) return;

    // --- 1. Динамическое позиционирование сайдбара (Логика Следования) ---

    // Высота шапки конструктора
    const editorHeaderHeight = editorHeader ? editorHeader.offsetHeight : 0;
    const HEADER_OFFSET = editorHeaderHeight + 20; // Отступ от нижней границы шапки

    function updateSidebarPosition() {
        if (window.innerWidth <= 992 || !sidebar || !questionsContainer) {
            // Отключаем логику на мобильных или если элементов нет
            return;
        }

        // 1. Определяем активный или ближайший к верхней части экрана вопрос
        const questionBlocks = Array.from(document.querySelectorAll('.question-block'));
        let targetBlock = questionBlocks.find(b => b.classList.contains('active-block'));

        if (!targetBlock && questionBlocks.length > 0) {
            // Если нет активного, берем первый блок, который виден
            targetBlock = questionBlocks[0];
        }

        if (targetBlock) {
            const rect = targetBlock.getBoundingClientRect();
            const containerRect = questionsContainer.getBoundingClientRect();

            // Вычисляем, где должна быть верхняя часть сайдбара относительно документа
            let newTopPosition = window.scrollY + rect.top;

            // Если активный блок уходит выше зоны заголовка, прилипаем к заголовку
            if (rect.top <= HEADER_OFFSET) {
                newTopPosition = window.scrollY + HEADER_OFFSET;
            }

            // Переводим абсолютную позицию в относительную позицию внутри .editor-main 
            // Мы вычитаем начальное смещение родителя (questionsContainer)
            const finalTop = newTopPosition - questionsContainer.offsetTop;

            // Ограничение: не выходить за нижнюю границу контейнера вопросов
            const maxTop = containerRect.height - sidebar.offsetHeight;

            // Применяем позицию, ограничивая ее снизу нулем (чтобы не уходить выше) и maxTop
            const clampedTop = Math.max(0, Math.min(finalTop, maxTop));

            sidebar.style.top = `${clampedTop}px`;
        }
    }

    // Привязываем функцию к прокрутке и изменению размера окна
    window.addEventListener('scroll', updateSidebarPosition);
    window.addEventListener('resize', updateSidebarPosition);

    // Запускаем при загрузке, чтобы установить начальную позицию
    // Добавляем небольшую задержку, чтобы убедиться, что все стили и размеры загружены
    setTimeout(updateSidebarPosition, 100);

    // --- 2. Управление активным блоком и кнопкой добавления ---

    questionsContainer.addEventListener('click', (e) => {
        const block = e.target.closest('.question-block');
        if (block) {
            setActiveBlock(block);
        }
    });

    function setActiveBlock(block) {
        // Удаляем активный класс со всех блоков
        document.querySelectorAll('.question-block').forEach(b => {
            b.classList.remove('active-block');
        });
        // Устанавливаем активный класс на текущий блок
        block.classList.add('active-block');
        updateSidebarPosition();
    }

    // --- 3. Обработка клика по кнопке "Добавить вопрос" ---

    document.getElementById('add-question-btn')?.addEventListener('click', () => {
        const newQuestionHtml = createNewQuestionBlock();
        questionsContainer.insertAdjacentHTML('beforeend', newQuestionHtml);

        const newBlock = questionsContainer.lastElementChild;
        setActiveBlock(newBlock);
        newBlock.scrollIntoView({ behavior: 'smooth', block: 'center' }); // Прокрутка к новому блоку
    });

    function createNewQuestionBlock() {
        const questionCount = document.querySelectorAll('.question-block').length;
        // Это упрощенная HTML-структура, которая должна соответствовать вашему бэкенду
        return `
            <div class="question-block active-block" data-question-id="temp_${Date.now()}">
                <div class="question-content">
                    <input type="text" class="question-input" value="Вопрос ${questionCount + 1}" placeholder="Введите вопрос">
                    
                    <div class="options-list">
                        <div class="option-item">
                            <input type="radio" disabled>
                            <input type="text" class="option-input" placeholder="Вариант 1">
                            <button class="remove-option-btn" title="Удалить вариант">×</button>
                        </div>
                        <button class="add-option-btn">+ Добавить вариант</button>
                    </div>
                </div>
                <div class="question-footer">
                    <button class="btn-delete-question" title="Удалить вопрос">🗑️</button>
                    <label>
                        <input type="checkbox" checked class="required-toggle"> Обязательный вопрос
                    </label>
                </div>
            </div>
        `;
    }

    // --- 4. Обработка сохранения (AJAX) ---

    // Предполагается, что у вас есть кнопка сохранения или автосохранение
    document.getElementById('save-survey-btn')?.addEventListener('click', async () => {
        const surveyData = collectSurveyData(); // Функция сбора всех данных из DOM

        try {
            const response = await fetch('/api/surveys/save', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    // Добавьте токен CSRF, если вы его используете (рекомендуется)
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: JSON.stringify(surveyData)
            });

            if (response.ok) {
                const result = await response.json();
                console.log('Сохранено успешно:', result);
                alert('Опрос успешно сохранен!');
                // Обновление ID в URL, если это была первая запись
                if (!surveyData.Id) {
                    window.history.pushState({}, '', `/survey/edit/${result.SurveyId}`);
                }
            } else {
                console.error('Ошибка сохранения:', response.statusText);
                alert('Ошибка при сохранении опроса.');
            }
        } catch (error) {
            console.error('Ошибка сети:', error);
            alert('Сбой сети при сохранении.');
        }
    });

    function collectSurveyData() {
        // Эта функция должна парсить DOM и собирать все данные в SaveSurveyViewModel
        const data = {
            Id: parseInt(document.body.dataset.surveyId) || null,
            Title: document.getElementById('survey-title-input').value,
            Description: document.getElementById('survey-description-input')?.value || '',
            // ... другие метаданные
            Questions: []
        };

        document.querySelectorAll('.question-block').forEach((block, index) => {
            // Здесь должна быть логика извлечения ID, текста, типа и опций
            // ...
        });

        return data;
    }

    // Инициализация
    updateSidebarPosition();
});