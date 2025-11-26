// Глобальный счетчик для временных ID новых, несохраненных вопросов
let newQuestionCounter = -1;
let newOptionCounter = -1;

document.addEventListener('DOMContentLoaded', () => {
    console.log('Constructor JavaScript loaded.');
    setupBlockActivation();
});

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
                <option value="ShortText">Текст (строка)</option>
                <option value="ParagraphText">Текст (абзац)</option>
                <option value="Scale">Шкала (1-5)</option>
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

    if (type === 'ShortText' || type === 'ParagraphText') {
        // Для текстовых полей
        const inputTag = type === 'ShortText' ?
            `<input type="text" class="input-line" placeholder="Текст ответа..." disabled>` :
            `<textarea class="input-line" rows="3" placeholder="Текст ответа..." disabled></textarea>`;

        container.innerHTML = `<div style="padding: 10px 0; color: var(--gray-text); font-style: italic;">${inputTag}</div>`;

    } else if (type === 'SingleChoice' || type === 'MultipleChoice' || type === 'Scale') {
        // Для вопросов с вариантами
        const defaultOptions = (type === 'Scale') ? ['1', '2', '3', '4', '5'] : ['Вариант 1'];

        defaultOptions.forEach((text, index) => {
            optionsHtml += getOptionHtml(questionId, text);
        });

        // Добавляем все варианты и зону "Добавить вариант"
        container.innerHTML = optionsHtml + `
            <div class="add-option-area" onclick="addOptionToQuestion(${questionId})">
                <input type="radio" disabled style="margin-right: 10px;">
                <span class="add-option-link">Добавить вариант</span>
            </div>
        `;
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