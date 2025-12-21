// Analytics.js - Инициализация графиков Chart.js для результатов опросов

document.addEventListener('DOMContentLoaded', function() {
    // Инициализируем графики для всех вопросов
    initializeCharts();
});

function initializeCharts() {
    // Находим все контейнеры с графиками
    const chartContainers = document.querySelectorAll('[data-chart-type]');
    
    chartContainers.forEach(container => {
        const chartType = container.getAttribute('data-chart-type');
        const questionId = container.getAttribute('data-question-id');
        const canvasId = `chart-${questionId}`;
        
        // Создаем canvas элемент, если его еще нет
        let canvas = container.querySelector('canvas');
        if (!canvas) {
            canvas = document.createElement('canvas');
            canvas.id = canvasId;
            container.appendChild(canvas);
        }
        
        // Получаем данные из data-атрибутов
        const labelsJson = container.getAttribute('data-labels');
        const dataJson = container.getAttribute('data-values');
        
        if (!labelsJson || !dataJson) {
            console.warn('Отсутствуют данные для графика:', questionId);
            return;
        }
        
        try {
            const labels = JSON.parse(labelsJson);
            const data = JSON.parse(dataJson);
            
            // Создаем график в зависимости от типа
            switch (chartType) {
                case 'pie':
                    createPieChart(canvasId, labels, data, questionId);
                    break;
                case 'bar':
                    createBarChart(canvasId, labels, data, questionId);
                    break;
                case 'line':
                    createLineChart(canvasId, labels, data, questionId);
                    break;
                case 'bar-rating':
                    createRatingChart(canvasId, labels, data, questionId);
                    break;
                default:
                    console.warn('Неизвестный тип графика:', chartType);
            }
        } catch (error) {
            console.error('Ошибка при парсинге данных графика:', error);
        }
    });
}

// Круговая диаграмма для SingleChoice
function createPieChart(canvasId, labels, data, questionId) {
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;
    
    new Chart(ctx, {
        type: 'pie',
        data: {
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: generateColors(data.length, 'pie'),
                borderWidth: 2,
                borderColor: '#ffffff'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
                legend: {
                    position: 'right',
                    labels: {
                        padding: 15,
                        font: {
                            size: 14
                        }
                    }
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            const label = context.label || '';
                            const value = context.parsed || 0;
                            const total = context.dataset.data.reduce((a, b) => a + b, 0);
                            const percentage = total > 0 ? ((value / total) * 100).toFixed(1) : 0;
                            return `${label}: ${value} (${percentage}%)`;
                        }
                    }
                }
            }
        }
    });
}

// Столбчатая диаграмма для MultipleChoice
function createBarChart(canvasId, labels, data, questionId) {
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;
    
    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Количество ответов',
                data: data,
                backgroundColor: 'rgba(76, 175, 80, 0.7)',
                borderColor: 'rgba(46, 125, 50, 1)',
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        stepSize: 1,
                        precision: 0
                    }
                }
            },
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            const value = context.parsed.y || 0;
                            const total = context.dataset.data.reduce((a, b) => a + b, 0);
                            const percentage = total > 0 ? ((value / total) * 100).toFixed(1) : 0;
                            return `Ответов: ${value} (${percentage}%)`;
                        }
                    }
                }
            }
        }
    });
}

// Линейный график для Scale
function createLineChart(canvasId, labels, data, questionId) {
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;
    
    new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Количество ответов',
                data: data,
                borderColor: 'rgba(46, 125, 50, 1)',
                backgroundColor: 'rgba(76, 175, 80, 0.2)',
                borderWidth: 3,
                fill: true,
                tension: 0.4,
                pointRadius: 5,
                pointHoverRadius: 7,
                pointBackgroundColor: 'rgba(46, 125, 50, 1)',
                pointBorderColor: '#ffffff',
                pointBorderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        stepSize: 1,
                        precision: 0
                    }
                }
            },
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            return `Ответов: ${context.parsed.y}`;
                        }
                    }
                }
            }
        }
    });
}

// Гистограмма для Rating
function createRatingChart(canvasId, labels, data, questionId) {
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;
    
    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Количество оценок',
                data: data,
                backgroundColor: generateColors(data.length, 'bar', data),
                borderColor: 'rgba(46, 125, 50, 1)',
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        stepSize: 1,
                        precision: 0
                    }
                }
            },
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            const value = context.parsed.y || 0;
                            const total = context.dataset.data.reduce((a, b) => a + b, 0);
                            const percentage = total > 0 ? ((value / total) * 100).toFixed(1) : 0;
                            return `Оценок: ${value} (${percentage}%)`;
                        }
                    }
                }
            }
        }
    });
}

// Генерация цветов для графиков
function generateColors(count, type, values) {
    const colors = [];
    
    if (type === 'pie') {
        // Для круговых диаграмм используем зеленую палитру
        const baseColors = [
            'rgba(76, 175, 80, 0.8)',   // Green
            'rgba(129, 199, 132, 0.8)', // Light Green
            'rgba(56, 142, 60, 0.8)',   // Dark Green
            'rgba(102, 187, 106, 0.8)', // Medium Green
            'rgba(27, 94, 32, 0.8)',    // Darker Green
            'rgba(200, 230, 201, 0.8)', // Very Light Green
            'rgba(165, 214, 167, 0.8)', // Light Medium Green
            'rgba(139, 195, 74, 0.8)'   // Lime Green
        ];
        
        for (let i = 0; i < count; i++) {
            colors.push(baseColors[i % baseColors.length]);
        }
    } else if (type === 'bar') {
        // Для рейтинга используем градиент от светлого к темному
        for (let i = 0; i < count; i++) {
            const intensity = 0.5 + (i / count) * 0.5; // От 0.5 до 1.0
            colors.push(`rgba(${Math.round(76 * intensity)}, ${Math.round(175 * intensity)}, ${Math.round(80 * intensity)}, 0.7)`);
        }
    }
    
    return colors;
}

