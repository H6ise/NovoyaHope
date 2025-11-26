// Глобальный счетчик для временных ID новых, несохраненных элементов
let newQuestionCounter = -1;
let newOptionCounter = -1;
let newSectionCounter = -1;
let newMediaCounter = -1;

document.addEventListener('DOMContentLoaded', () => {
    console.log('Constructor JavaScript loaded.');
    setupBlockActivation();
    setupFAB();
});

// ===========================================
// FAB (FLOATING ACTION BAR) УПРАВЛЕНИЕ
// ===========================================

function setupFAB() {
    // Закрытие FAB меню при клике вне его
    document.addEventListener('click', (e) => {
        const fabContainer = document.querySelector('.fab-container');
        const fabMenu = document.getElementById('fab-menu');
        const fabMain = document.getElementById('fab-main');
        
        if (fabContainer && !fabContainer.contains(e.target)) {
            fabMenu.classList.remove('active');
            fabMain.classList.remove('active');
        }
    });
}

function toggleFabMenu() {
    const fabMenu = document.getElementById('fab-menu');
    const fabMain = document.getElementById('fab-main');
    
    fabMenu.classList.toggle('active');
    fabMain.classList.toggle('active');
}

// ===========================================
// 1. АКТИВАЦИЯ БЛОКА ВОПРОСА
// ===========================================

function setupBlockActivation() {
    // Делаем активным только тот блок, на который кликнули
    document.querySelectorAll('.question-block').forEach(block => {
        block.onclick = (e) => {
            // Игнорируем клики по элементам управления (кнопки, селекты, инпуты) внутри блока
            if (e.target.tagName === 'INPUT' || e.target.tagName === 'BUTTON' || e.target.tagName === 'SELECT' || e.target.closest('.btn-icon')) {
                return;
            }

            // Убираем активное состояние со всех блоков
            document.querySelectorAll('.question-block').forEach(b => b.classList.remove('active'));

            // Добавляем активное состояние текущему блоку
            block.classList.add('active');
        };
    });
}

// ===========================================
// 2. ГЕНЕРАЦИЯ HTML ДЛЯ НОВЫХ ЭЛЕМЕНТОВ
// ===========================================

// Генерирует HTML для нового варианта ответа (option)
function getOptionHtml(questionId, text = 'Вариант', isOther = false) {
    const tempOptionId = newOptionCounter--; // Используем отрицательный ID для временных опций

    // Тип инпута (radio или checkbox) будет определен функцией changeQuestionType
    const inputType = document.querySelector(`.question-block[data-question-id="${questionId}"] .question-type-select`).value === 'MultipleChoice' ? 'checkbox' : 'radio';

    const baseHtml = `
        <div class="option-item" data-option-id="${tempOptionId}">
            <input type="${inputType}" disabled style="margin-right: 10px;">
            <input type="text" class="option-input" placeholder="Вариант" 
                   value="${isOther ? 'Другое...' : text}" 
                   name="Questions[${questionId}].Options[${tempOptionId}].Text">
            
            <input type="hidden" name="Questions[${questionId}].Options[${tempOptionId}].Order" value="${Math.abs(tempOptionId)}">
            <input type="hidden" name="Questions[${questionId}].Options[${tempOptionId}].IsOther" value="${isOther}">

            <button type="button" class="btn-icon delete-option-btn" onclick="deleteOption(this)">
                <i class="fas fa-times"></i>
            </button>
        </div>
    `;
    return baseHtml.trim();
}


// Генерирует HTML для нового блока вопроса
function getNewQuestionHtml(tempId) {
    const defaultOptionHtml = getOptionHtml(tempId, 'Вариант 1');

    return `
    <div class="question-block active" data-question-id="${tempId}" data-is-new="true">
        <div class="question-header">
            <input type="text" class="question-input question-text-input" 
                   placeholder="Вопрос без заголовка" value="" 
                   name="Questions[${tempId}].Text">
            
            <select class="form-control question-type-select" 
                    name="Questions[${tempId}].Type"
                    onchange="changeQuestionType(this, ${tempId})">
                <option value="SingleChoice" selected>Один из списка</option>
                <option value="MultipleChoice">Несколько из списка</option>
                <option value="Dropdown">Раскрывающийся список</option>
                <option value="ShortText">Текст (строка)</option>
                <option value="ParagraphText">Текст (абзац)</option>
                <option value="FileUpload">Загрузка файлов</option>
                <option value="Scale">Шкала</option>
                <option value="Rating">Оценка ⭐</option>
                <option value="CheckboxGrid">Сетка (множественный выбор)</option>
                <option value="RadioGrid">Сетка флажков</option>
                <option value="Date">Дата</option>
                <option value="Time">Время</option>
            </select>
        </div>

        <div class="question-body options-container" data-question-id="${tempId}">
            ${defaultOptionHtml}
            
            <div class="add-option-area" onclick="addOptionToQuestion(${tempId})">
                <input type="radio" disabled style="margin-right: 10px;">
                <span class="add-option-link">Добавить вариант</span>
            </div>
        </div>

        <div class="question-footer">
            <button type="button" class="btn-icon duplicate-btn" title="Дублировать" onclick="duplicateQuestion(this)"><i class="far fa-copy"></i></button>
            <button type="button" class="btn-icon delete-btn" title="Удалить" onclick="deleteQuestion(this)"><i class="fas fa-trash-alt"></i></button>
            <div class="separator-line"></div>
            <label class="required-label">
                Обязательный вопрос
                <input type="checkbox" class="required-toggle" name="Questions[${tempId}].IsRequired">
            </label>
        </div>
    </div>
    `;
}

// ===========================================
// 3. ОСНОВНАЯ ЛОГИКА ДОБАВЛЕНИЯ
// ===========================================

function addNewQuestion() {
    // 1. Снимаем активность со всех блоков
    document.querySelectorAll('.question-block').forEach(block => block.classList.remove('active'));

    // 2. Генерируем новый временный ID
    const tempId = newQuestionCounter--;
    const container = document.getElementById('questions-container');

    // 3. Находим блок заголовка, чтобы вставить новый вопрос после него
    const headerBlock = container.querySelector('.question-block:first-child');

    // 4. Создаем новый блок
    const newBlock = document.createElement('div');
    newBlock.innerHTML = getNewQuestionHtml(tempId).trim();
    const insertedBlock = newBlock.firstChild;

    // 5. Вставляем
    if (headerBlock) {
        headerBlock.after(insertedBlock);
    } else {
        container.appendChild(insertedBlock);
    }

    // 6. Прокручиваем и активируем слушателей
    insertedBlock.scrollIntoView({ behavior: 'smooth', block: 'center' });
    setupBlockActivation(); // Перенастраиваем слушателей кликов

    // Дополнительный шаг: фокусируемся на поле ввода вопроса
    insertedBlock.querySelector('.question-text-input').focus();
}

// ===========================================
// 4. ФУНКЦИИ УПРАВЛЕНИЯ ЭЛЕМЕНТАМИ
// ===========================================

function deleteQuestion(button) {
    if (confirm('Вы уверены, что хотите удалить этот вопрос?')) {
        const block = button.closest('.question-block');
        block.remove();
    }
}

function addOptionToQuestion(questionId) {
    const container = document.querySelector(`.question-block[data-question-id="${questionId}"] .options-container`);
    const addOptionArea = container.querySelector('.add-option-area');

    const newOptionHtml = getOptionHtml(questionId);

    // Вставляем новый вариант перед кнопкой "Добавить вариант"
    addOptionArea.insertAdjacentHTML('beforebegin', newOptionHtml);

    // Фокусируемся на новом поле ввода
    addOptionArea.previousElementSibling.querySelector('.option-input').focus();
}

function deleteOption(button) {
    button.closest('.option-item').remove();
}

function changeQuestionType(selectElement, questionId) {
    const type = selectElement.value;
    const container = document.querySelector(`.question-block[data-question-id="${questionId}"] .options-container`);
    let optionsHtml = '';

    // Сбрасываем содержимое контейнера
    container.innerHTML = '';

    // Текстовые типы
    if (type === 'ShortText' || type === 'ParagraphText') {
        const inputTag = type === 'ShortText' ?
            `<input type="text" class="input-line" placeholder="Краткий ответ" disabled>` :
            `<textarea class="input-line" rows="3" placeholder="Развернутый ответ" disabled></textarea>`;

        container.innerHTML = `<div style="padding: 10px 0; color: var(--gray-text); font-style: italic;">${inputTag}</div>`;

    }
    // Дата и время
    else if (type === 'Date') {
        container.innerHTML = `<div style="padding: 10px 0; color: var(--gray-text); font-style: italic;">
            <input type="date" class="input-line" disabled>
        </div>`;
    }
    else if (type === 'Time') {
        container.innerHTML = `<div style="padding: 10px 0; color: var(--gray-text); font-style: italic;">
            <input type="time" class="input-line" disabled>
        </div>`;
    }
    // Загрузка файлов
    else if (type === 'FileUpload') {
        container.innerHTML = `<div style="padding: 15px; border: 2px dashed #dadce0; border-radius: 4px; text-align: center; color: #5f6368;">
            <i class="fas fa-cloud-upload-alt" style="font-size: 2rem; margin-bottom: 10px; display: block;"></i>
            <span>Добавить файл или перетащите сюда</span>
        </div>`;
    }
    // Оценка звездами
    else if (type === 'Rating') {
        container.innerHTML = `<div class="rating-stars" style="padding: 10px 0;">
            ${[1,2,3,4,5].map(i => `<span class="star-icon">⭐</span>`).join('')}
        </div>`;
    }
    // Шкала
    else if (type === 'Scale') {
        container.innerHTML = `<div style="padding: 15px 0;">
            <div style="display: flex; justify-content: space-between; margin-bottom: 10px;">
                <span style="color: #5f6368; font-size: 0.9rem;">1 (Низкая)</span>
                <span style="color: #5f6368; font-size: 0.9rem;">5 (Высокая)</span>
            </div>
            <div style="display: flex; gap: 10px; justify-content: center;">
                ${[1,2,3,4,5].map(i => `
                    <label style="display: flex; flex-direction: column; align-items: center; gap: 5px;">
                        <input type="radio" name="scale_${questionId}" disabled>
                        <span style="color: #5f6368; font-size: 0.9rem;">${i}</span>
                    </label>
                `).join('')}
            </div>
        </div>`;
    }
    // Сетка с чекбоксами
    else if (type === 'CheckboxGrid') {
        container.innerHTML = `<div class="grid-container">
            <p style="color: #5f6368; font-size: 0.9rem; margin-bottom: 10px;">Строки и столбцы будут настроены отдельно</p>
            <table class="grid-table" style="width: 100%; border-collapse: collapse;">
                <thead>
                    <tr>
                        <th></th>
                        <th>Столбец 1</th>
                        <th>Столбец 2</th>
                        <th>Столбец 3</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td style="font-weight: 500;">Строка 1</td>
                        <td><input type="checkbox" disabled></td>
                        <td><input type="checkbox" disabled></td>
                        <td><input type="checkbox" disabled></td>
                    </tr>
                    <tr>
                        <td style="font-weight: 500;">Строка 2</td>
                        <td><input type="checkbox" disabled></td>
                        <td><input type="checkbox" disabled></td>
                        <td><input type="checkbox" disabled></td>
                    </tr>
                </tbody>
            </table>
        </div>`;
    }
    // Сетка с радио-кнопками
    else if (type === 'RadioGrid') {
        container.innerHTML = `<div class="grid-container">
            <p style="color: #5f6368; font-size: 0.9rem; margin-bottom: 10px;">Строки и столбцы будут настроены отдельно</p>
            <table class="grid-table" style="width: 100%; border-collapse: collapse;">
                <thead>
                    <tr>
                        <th></th>
                        <th>Столбец 1</th>
                        <th>Столбец 2</th>
                        <th>Столбец 3</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td style="font-weight: 500;">Строка 1</td>
                        <td><input type="radio" name="grid_row1_${questionId}" disabled></td>
                        <td><input type="radio" name="grid_row1_${questionId}" disabled></td>
                        <td><input type="radio" name="grid_row1_${questionId}" disabled></td>
                    </tr>
                    <tr>
                        <td style="font-weight: 500;">Строка 2</td>
                        <td><input type="radio" name="grid_row2_${questionId}" disabled></td>
                        <td><input type="radio" name="grid_row2_${questionId}" disabled></td>
                        <td><input type="radio" name="grid_row2_${questionId}" disabled></td>
                    </tr>
                </tbody>
            </table>
        </div>`;
    }
    // Варианты ответов (радио, чекбокс, выпадающий список)
    else if (type === 'SingleChoice' || type === 'MultipleChoice' || type === 'Dropdown') {
        const defaultOptions = ['Вариант 1'];
        const inputType = type === 'MultipleChoice' ? 'checkbox' : 'radio';
        const icon = type === 'Dropdown' ? '▼' : '';

        defaultOptions.forEach((text, index) => {
            optionsHtml += getOptionHtml(questionId, text);
        });

        // Для выпадающего списка показываем другой вид
        if (type === 'Dropdown') {
            container.innerHTML = optionsHtml + `
                <div class="add-option-area" onclick="addOptionToQuestion(${questionId})">
                    <span style="margin-right: 10px;">⊕</span>
                    <span class="add-option-link">Добавить вариант</span>
                </div>
            `;
        } else {
            container.innerHTML = optionsHtml + `
                <div class="add-option-area" onclick="addOptionToQuestion(${questionId})">
                    <input type="${inputType}" disabled style="margin-right: 10px;">
                    <span class="add-option-link">Добавить вариант</span>
                </div>
            `;
        }
    }

    // Обновляем тип инпутов в options (если они есть)
    updateOptionInputTypes(questionId, type);
}

function updateOptionInputTypes(questionId, newType) {
    const questionBlock = document.querySelector(`.question-block[data-question-id="${questionId}"]`);
    const isMulti = newType === 'MultipleChoice';
    const inputType = isMulti ? 'checkbox' : 'radio';

    questionBlock.querySelectorAll('.option-item input[type="radio"], .option-item input[type="checkbox"]').forEach(input => {
        // Мы не можем изменить type напрямую, поэтому клонируем элемент
        const newInput = input.cloneNode(true);
        newInput.type = inputType;
        input.parentNode.replaceChild(newInput, input);
    });
}

function duplicateQuestion(button) {
    const originalBlock = button.closest('.question-block');
    const newBlock = originalBlock.cloneNode(true);
    
    // Генерируем новый ID для дублированного вопроса
    const newId = newQuestionCounter--;
    
    // Обновляем data-question-id
    newBlock.setAttribute('data-question-id', newId);
    newBlock.setAttribute('data-is-new', 'true');
    
    // Обновляем все name атрибуты в инпутах
    newBlock.querySelectorAll('input, select, textarea').forEach(input => {
        if (input.name) {
            input.name = input.name.replace(/Questions\[[-\d]+\]/, `Questions[${newId}]`);
        }
    });
    
    // Обновляем onclick атрибуты
    const addOptionArea = newBlock.querySelector('.add-option-area');
    if (addOptionArea) {
        addOptionArea.setAttribute('onclick', `addOptionToQuestion(${newId})`);
    }
    
    // Обновляем onchange для селекта типа вопроса
    const typeSelect = newBlock.querySelector('.question-type-select');
    if (typeSelect) {
        typeSelect.setAttribute('onchange', `changeQuestionType(this, ${newId})`);
    }
    
    // Снимаем активность со всех блоков
    document.querySelectorAll('.question-block').forEach(block => block.classList.remove('active'));
    
    // Делаем новый блок активным
    newBlock.classList.add('active');
    
    // Вставляем после оригинала
    originalBlock.after(newBlock);
    
    // Прокручиваем к новому блоку
    newBlock.scrollIntoView({ behavior: 'smooth', block: 'center' });
    
    // Перенастраиваем слушателей
    setupBlockActivation();
}

// ===========================================
// 5. ФУНКЦИИ ДЛЯ ДОБАВЛЕНИЯ НОВЫХ ЭЛЕМЕНТОВ
// ===========================================

// Добавить раздел (Section)
function addSection() {
    closeFabMenu();
    
    const tempId = newSectionCounter--;
    const container = document.getElementById('questions-container');
    
    const sectionHtml = `
    <div class="question-block section-block active" data-section-id="${tempId}" data-type="section">
        <div class="section-header">
            <input type="text" class="editor-title-input" 
                   placeholder="Заголовок раздела" 
                   name="Sections[${tempId}].Title"
                   style="font-size: 1.3rem; font-weight: 600;">
            <input type="hidden" name="Sections[${tempId}].Order" value="${Math.abs(tempId)}">
        </div>
        <div class="section-body" style="margin-top: 15px;">
            <textarea class="question-input" 
                      placeholder="Описание раздела (необязательно)" 
                      name="Sections[${tempId}].Description"
                      rows="3"
                      style="width: 100%; resize: vertical;"></textarea>
        </div>
        <div class="question-footer">
            <button type="button" class="btn-icon delete-btn" title="Удалить" onclick="deleteElement(this)">
                <i class="fas fa-trash-alt"></i>
            </button>
            <span style="color: #5f6368; font-size: 0.9rem; margin-left: auto;">
                <i class="fas fa-layer-group"></i> Раздел
            </span>
        </div>
    </div>
    `;
    
    const newBlock = document.createElement('div');
    newBlock.innerHTML = sectionHtml.trim();
    const insertedBlock = newBlock.firstChild;
    
    // Снимаем активность со всех блоков
    document.querySelectorAll('.question-block').forEach(block => block.classList.remove('active'));
    
    // Вставляем в конец контейнера
    container.appendChild(insertedBlock);
    
    // Прокручиваем к новому блоку
    insertedBlock.scrollIntoView({ behavior: 'smooth', block: 'center' });
    insertedBlock.querySelector('.editor-title-input').focus();
    
    setupBlockActivation();
}

// Добавить изображение
function addImage() {
    closeFabMenu();
    
    const tempId = newMediaCounter--;
    const container = document.getElementById('questions-container');
    
    const imageHtml = `
    <div class="question-block media-block active" data-media-id="${tempId}" data-type="image">
        <div class="media-header" style="margin-bottom: 15px;">
            <i class="fas fa-image" style="color: var(--primary-green); font-size: 1.5rem; margin-right: 10px;"></i>
            <input type="text" class="question-input" 
                   placeholder="Название изображения (необязательно)" 
                   name="Media[${tempId}].Title"
                   style="font-size: 1.1rem;">
            <input type="hidden" name="Media[${tempId}].Type" value="Image">
            <input type="hidden" name="Media[${tempId}].Order" value="${Math.abs(tempId)}">
        </div>
        <div class="media-body">
            <div class="image-upload-area" style="border: 2px dashed #dadce0; border-radius: 8px; padding: 30px; text-align: center; background: #f5faf5;">
                <i class="fas fa-cloud-upload-alt" style="font-size: 3rem; color: var(--primary-green); margin-bottom: 15px; display: block;"></i>
                <p style="color: #5f6368; margin-bottom: 10px;">Перетащите изображение сюда или</p>
                <label class="btn-primary" style="display: inline-block; cursor: pointer;">
                    Выбрать файл
                    <input type="file" accept="image/*" style="display: none;" 
                           onchange="handleImageUpload(this, ${tempId})" 
                           name="Media[${tempId}].File">
                </label>
                <input type="text" class="question-input" 
                       placeholder="или введите URL изображения" 
                       name="Media[${tempId}].Url"
                       style="margin-top: 15px;">
            </div>
            <textarea class="question-input" 
                      placeholder="Описание изображения (необязательно)" 
                      name="Media[${tempId}].Description"
                      rows="2"
                      style="margin-top: 15px; width: 100%; resize: vertical;"></textarea>
        </div>
        <div class="question-footer">
            <button type="button" class="btn-icon delete-btn" title="Удалить" onclick="deleteElement(this)">
                <i class="fas fa-trash-alt"></i>
            </button>
            <span style="color: #5f6368; font-size: 0.9rem; margin-left: auto;">
                <i class="fas fa-image"></i> Изображение
            </span>
        </div>
    </div>
    `;
    
    const newBlock = document.createElement('div');
    newBlock.innerHTML = imageHtml.trim();
    const insertedBlock = newBlock.firstChild;
    
    // Снимаем активность со всех блоков
    document.querySelectorAll('.question-block').forEach(block => block.classList.remove('active'));
    
    container.appendChild(insertedBlock);
    insertedBlock.scrollIntoView({ behavior: 'smooth', block: 'center' });
    
    setupBlockActivation();
}

// Добавить видео
function addVideo() {
    closeFabMenu();
    
    const tempId = newMediaCounter--;
    const container = document.getElementById('questions-container');
    
    const videoHtml = `
    <div class="question-block media-block active" data-media-id="${tempId}" data-type="video">
        <div class="media-header" style="margin-bottom: 15px;">
            <i class="fas fa-video" style="color: var(--primary-green); font-size: 1.5rem; margin-right: 10px;"></i>
            <input type="text" class="question-input" 
                   placeholder="Название видео (необязательно)" 
                   name="Media[${tempId}].Title"
                   style="font-size: 1.1rem;">
            <input type="hidden" name="Media[${tempId}].Type" value="Video">
            <input type="hidden" name="Media[${tempId}].Order" value="${Math.abs(tempId)}">
        </div>
        <div class="media-body">
            <div class="video-upload-area" style="border: 2px dashed #dadce0; border-radius: 8px; padding: 30px; text-align: center; background: #f5faf5;">
                <i class="fas fa-play-circle" style="font-size: 3rem; color: var(--primary-green); margin-bottom: 15px; display: block;"></i>
                <p style="color: #5f6368; margin-bottom: 10px;">Вставьте ссылку на YouTube или введите URL</p>
                <input type="text" class="question-input" 
                       placeholder="https://www.youtube.com/watch?v=..." 
                       name="Media[${tempId}].Url"
                       style="margin-top: 10px; width: 100%;">
                <p style="color: #999; font-size: 0.85rem; margin-top: 10px;">
                    Поддерживаются: YouTube, Vimeo, или прямая ссылка на видео
                </p>
            </div>
            <textarea class="question-input" 
                      placeholder="Описание видео (необязательно)" 
                      name="Media[${tempId}].Description"
                      rows="2"
                      style="margin-top: 15px; width: 100%; resize: vertical;"></textarea>
        </div>
        <div class="question-footer">
            <button type="button" class="btn-icon delete-btn" title="Удалить" onclick="deleteElement(this)">
                <i class="fas fa-trash-alt"></i>
            </button>
            <span style="color: #5f6368; font-size: 0.9rem; margin-left: auto;">
                <i class="fas fa-video"></i> Видео
            </span>
        </div>
    </div>
    `;
    
    const newBlock = document.createElement('div');
    newBlock.innerHTML = videoHtml.trim();
    const insertedBlock = newBlock.firstChild;
    
    // Снимаем активность со всех блоков
    document.querySelectorAll('.question-block').forEach(block => block.classList.remove('active'));
    
    container.appendChild(insertedBlock);
    insertedBlock.scrollIntoView({ behavior: 'smooth', block: 'center' });
    
    setupBlockActivation();
}

// Добавить текстовый блок
function addTextBlock() {
    closeFabMenu();
    
    const tempId = newSectionCounter--;
    const container = document.getElementById('questions-container');
    
    const textHtml = `
    <div class="question-block text-block active" data-text-id="${tempId}" data-type="text">
        <div class="text-body">
            <textarea class="question-input" 
                      placeholder="Введите текст или инструкции для респондентов..." 
                      name="TextBlocks[${tempId}].Content"
                      rows="5"
                      style="width: 100%; resize: vertical; font-size: 1rem; line-height: 1.6;"></textarea>
            <input type="hidden" name="TextBlocks[${tempId}].Order" value="${Math.abs(tempId)}">
        </div>
        <div class="question-footer">
            <button type="button" class="btn-icon delete-btn" title="Удалить" onclick="deleteElement(this)">
                <i class="fas fa-trash-alt"></i>
            </button>
            <span style="color: #5f6368; font-size: 0.9rem; margin-left: auto;">
                <i class="fas fa-align-left"></i> Текстовый блок
            </span>
        </div>
    </div>
    `;
    
    const newBlock = document.createElement('div');
    newBlock.innerHTML = textHtml.trim();
    const insertedBlock = newBlock.firstChild;
    
    // Снимаем активность со всех блоков
    document.querySelectorAll('.question-block').forEach(block => block.classList.remove('active'));
    
    container.appendChild(insertedBlock);
    insertedBlock.scrollIntoView({ behavior: 'smooth', block: 'center' });
    insertedBlock.querySelector('textarea').focus();
    
    setupBlockActivation();
}

// Удалить любой элемент (универсальная функция)
function deleteElement(button) {
    if (confirm('Вы уверены, что хотите удалить этот элемент?')) {
        const block = button.closest('.question-block');
        block.remove();
    }
}

// Закрыть FAB меню
function closeFabMenu() {
    const fabMenu = document.getElementById('fab-menu');
    const fabMain = document.getElementById('fab-main');
    
    if (fabMenu) {
        fabMenu.classList.remove('active');
        fabMain.classList.remove('active');
    }
}

// Обработка загрузки изображения
function handleImageUpload(input, mediaId) {
    if (input.files && input.files[0]) {
        const file = input.files[0];
        const reader = new FileReader();
        
        reader.onload = function(e) {
            // Создаем превью изображения
            const uploadArea = input.closest('.image-upload-area');
            uploadArea.innerHTML = `
                <img src="${e.target.result}" style="max-width: 100%; max-height: 400px; border-radius: 4px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);">
                <div style="margin-top: 15px;">
                    <button type="button" class="btn-secondary" onclick="resetImageUpload(this, ${mediaId})">
                        Изменить изображение
                    </button>
                </div>
            `;
        };
        
        reader.readAsDataURL(file);
    }
}

// Сброс загрузки изображения
function resetImageUpload(button, mediaId) {
    const uploadArea = button.closest('.image-upload-area');
    uploadArea.innerHTML = `
        <i class="fas fa-cloud-upload-alt" style="font-size: 3rem; color: var(--primary-green); margin-bottom: 15px; display: block;"></i>
        <p style="color: #5f6368; margin-bottom: 10px;">Перетащите изображение сюда или</p>
        <label class="btn-primary" style="display: inline-block; cursor: pointer;">
            Выбрать файл
            <input type="file" accept="image/*" style="display: none;" 
                   onchange="handleImageUpload(this, ${mediaId})" 
                   name="Media[${mediaId}].File">
        </label>
        <input type="text" class="question-input" 
               placeholder="или введите URL изображения" 
               name="Media[${mediaId}].Url"
               style="margin-top: 15px;">
    `;
}