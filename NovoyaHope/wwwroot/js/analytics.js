// Analytics.js - Инициализация графиков Chart.js для результатов опросов
// Улучшенная версия с поддержкой всех типов вопросов и улучшенной визуализацией

document.addEventListener('DOMContentLoaded', function() {
    // Инициализируем графики для всех вопросов
    initializeCharts();
});

// Настройка Chart.js для улучшения четкости
Chart.defaults.font.family = "'Roboto', 'Segoe UI', sans-serif";
Chart.defaults.font.size = 14;
Chart.defaults.responsive = true;
Chart.defaults.maintainAspectRatio = false;
// Улучшение четкости для Retina дисплеев
Chart.defaults.devicePixelRatio = window.devicePixelRatio || 1;

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

// Круговая диаграмма для SingleChoice, Dropdown, RadioGrid
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
                borderWidth: 3,
                borderColor: '#ffffff',
                hoverBorderWidth: 4,
                hoverOffset: 8
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            animation: {
                animateRotate: true,
                animateScale: true,
                duration: 1000,
                easing: 'easeOutQuart'
            },
            plugins: {
                legend: {
                    position: 'right',
                    labels: {
                        padding: 20,
                        font: {
                            size: 13,
                            weight: '500'
                        },
                        usePointStyle: true,
                        pointStyle: 'circle',
                        color: '#1a1a1a'
                    },
                    onHover: function(event, legendItem) {
                        event.native.target.style.cursor = 'pointer';
                    },
                    onLeave: function(event, legendItem) {
                        event.native.target.style.cursor = 'default';
                    }
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    padding: 12,
                    titleFont: {
                        size: 14,
                        weight: '600'
                    },
                    bodyFont: {
                        size: 13
                    },
                    callbacks: {
                        label: function(context) {
                            const label = context.label || '';
                            const value = context.parsed || 0;
                            const total = context.dataset.data.reduce((a, b) => a + b, 0);
                            const percentage = total > 0 ? ((value / total) * 100).toFixed(1) : 0;
                            return `${label}: ${value} ответов (${percentage}%)`;
                        }
                    }
                }
            }
        }
    });
}

// Столбчатая диаграмма для MultipleChoice, CheckboxGrid, Dropdown
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
                backgroundColor: generateColors(data.length, 'bar', data),
                borderColor: 'rgba(46, 125, 50, 1)',
                borderWidth: 2,
                borderRadius: 6,
                borderSkipped: false
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            animation: {
                duration: 1000,
                easing: 'easeOutQuart'
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        stepSize: 1,
                        precision: 0,
                        font: {
                            size: 12
                        },
                        color: '#5f6368'
                    },
                    grid: {
                        color: 'rgba(0, 0, 0, 0.05)',
                        drawBorder: false
                    }
                },
                x: {
                    ticks: {
                        font: {
                            size: 12
                        },
                        color: '#5f6368',
                        maxRotation: 45,
                        minRotation: 0
                    },
                    grid: {
                        display: false
                    }
                }
            },
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    padding: 12,
                    titleFont: {
                        size: 14,
                        weight: '600'
                    },
                    bodyFont: {
                        size: 13
                    },
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
                backgroundColor: 'rgba(76, 175, 80, 0.15)',
                borderWidth: 3,
                fill: true,
                tension: 0.4,
                pointRadius: 6,
                pointHoverRadius: 8,
                pointBackgroundColor: 'rgba(46, 125, 50, 1)',
                pointBorderColor: '#ffffff',
                pointBorderWidth: 3,
                pointHoverBackgroundColor: 'rgba(46, 125, 50, 0.8)',
                pointHoverBorderColor: '#ffffff',
                pointHoverBorderWidth: 3
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            animation: {
                duration: 1000,
                easing: 'easeOutQuart'
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        stepSize: 1,
                        precision: 0,
                        font: {
                            size: 12
                        },
                        color: '#5f6368'
                    },
                    grid: {
                        color: 'rgba(0, 0, 0, 0.05)',
                        drawBorder: false
                    }
                },
                x: {
                    ticks: {
                        font: {
                            size: 12
                        },
                        color: '#5f6368'
                    },
                    grid: {
                        display: false
                    }
                }
            },
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    padding: 12,
                    titleFont: {
                        size: 14,
                        weight: '600'
                    },
                    bodyFont: {
                        size: 13
                    },
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

// Гистограмма для Rating с градиентом
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
                backgroundColor: generateRatingColors(data.length, data),
                borderColor: 'rgba(46, 125, 50, 1)',
                borderWidth: 2,
                borderRadius: 6,
                borderSkipped: false
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            animation: {
                duration: 1000,
                easing: 'easeOutQuart'
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        stepSize: 1,
                        precision: 0,
                        font: {
                            size: 12
                        },
                        color: '#5f6368'
                    },
                    grid: {
                        color: 'rgba(0, 0, 0, 0.05)',
                        drawBorder: false
                    }
                },
                x: {
                    ticks: {
                        font: {
                            size: 12
                        },
                        color: '#5f6368'
                    },
                    grid: {
                        display: false
                    }
                }
            },
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    padding: 12,
                    titleFont: {
                        size: 14,
                        weight: '600'
                    },
                    bodyFont: {
                        size: 13
                    },
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
        // Расширенная зеленая палитра для круговых диаграмм
        const baseColors = [
            'rgba(76, 175, 80, 0.85)',      // Green
            'rgba(129, 199, 132, 0.85)',   // Light Green
            'rgba(56, 142, 60, 0.85)',     // Dark Green
            'rgba(102, 187, 106, 0.85)',   // Medium Green
            'rgba(27, 94, 32, 0.85)',       // Darker Green
            'rgba(200, 230, 201, 0.85)',   // Very Light Green
            'rgba(165, 214, 167, 0.85)',   // Light Medium Green
            'rgba(139, 195, 74, 0.85)',     // Lime Green
            'rgba(67, 160, 71, 0.85)',      // Medium Dark Green
            'rgba(104, 159, 56, 0.85)',     // Olive Green
            'rgba(156, 204, 101, 0.85)',    // Light Lime
            'rgba(85, 139, 47, 0.85)'       // Forest Green
        ];
        
        for (let i = 0; i < count; i++) {
            colors.push(baseColors[i % baseColors.length]);
        }
    } else if (type === 'bar') {
        // Для столбчатых диаграмм используем градиент
        for (let i = 0; i < count; i++) {
            const intensity = 0.6 + (i / count) * 0.4; // От 0.6 до 1.0
            colors.push(`rgba(${Math.round(76 * intensity)}, ${Math.round(175 * intensity)}, ${Math.round(80 * intensity)}, 0.75)`);
        }
    }
    
    return colors;
}

// Генерация цветов для рейтинга с градиентом от светлого к темному
function generateRatingColors(count, values) {
    const colors = [];
    const maxValue = Math.max(...values, 1);
    
    for (let i = 0; i < count; i++) {
        // Градиент от желтого через зеленый к темно-зеленому
        const ratio = (i + 1) / count;
        let r, g, b;
        
        if (ratio < 0.33) {
            // От желтого к желто-зеленому
            const localRatio = ratio / 0.33;
            r = Math.round(255 - (255 - 255) * localRatio);
            g = Math.round(193 + (76 - 193) * localRatio);
            b = Math.round(7 + (80 - 7) * localRatio);
        } else if (ratio < 0.66) {
            // От желто-зеленого к зеленому
            const localRatio = (ratio - 0.33) / 0.33;
            r = Math.round(255 - (76 - 255) * localRatio);
            g = Math.round(76 + (175 - 76) * localRatio);
            b = Math.round(80 + (80 - 80) * localRatio);
        } else {
            // От зеленого к темно-зеленому
            const localRatio = (ratio - 0.66) / 0.34;
            r = Math.round(76 - (56 - 76) * localRatio);
            g = Math.round(175 - (142 - 175) * localRatio);
            b = Math.round(80 - (60 - 80) * localRatio);
        }
        
        colors.push(`rgba(${r}, ${g}, ${b}, 0.8)`);
    }
    
    return colors;
}

