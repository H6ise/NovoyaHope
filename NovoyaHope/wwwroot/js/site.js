document.addEventListener('DOMContentLoaded', () => {
    console.log('Site JavaScript loaded successfully.');

    // --- 1. Обработка частичного представления входа (если используется) ---
    // Показываем модальное окно или форму, если есть ошибки валидации
    handleAuthFormErrors();

    // --- 2. Обработка кнопки выхода ---
    document.querySelector('.logout-link')?.addEventListener('click', (e) => {
        e.preventDefault();
        // Находим скрытую форму выхода и отправляем ее
        document.getElementById('logoutForm')?.submit();
    });
});

function handleAuthFormErrors() {
    // Эта функция может быть полезна, если вы отображаете формы входа/регистрации
    // в модальном окне или частичном представлении и хотите показать их при ошибке.
    const validationSummary = document.querySelector('.validation-summary-errors');
    if (validationSummary && validationSummary.children.length > 0) {
        console.warn('Authentication form submitted with errors.');
        // Если вы используете модальное окно, здесь нужно его открыть.
        // showModal('authModal');
    }
}