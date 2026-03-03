function formatDecimalInput(input) {
    // При потере фокуса форматируем значение
    input.addEventListener('blur', function() {
        let value = this.value.trim();
        
        // Убираем все пробелы
        value = value.replace(/\s/g, '');
        
        // Если пусто - оставляем как есть
        if (!value) return;
        
        // Если целое число - добавляем .00
        if (/^\d+$/.test(value)) {
            this.value = value + '.00';
        }
        // Если число с точкой или запятой - нормализуем
        else if (/^\d+[.,]\d*$/.test(value)) {
            // Заменяем запятую на точку
            value = value.replace(',', '.');
            
            // Добавляем нули если нужно
            const parts = value.split('.');
            if (parts[1].length === 0) {
                this.value = parts[0] + '.00';
            } else if (parts[1].length === 1) {
                this.value = parts[0] + '.' + parts[1] + '0';
            } else {
                this.value = value;
            }
        }
    });
    
    // При вводе разрешаем только цифры, точку и запятую
    input.addEventListener('input', function(e) {
        this.value = this.value.replace(/[^\d.,]/g, '');
        
        // Разрешаем только одну точку или запятую
        const hasComma = this.value.includes(',');
        const hasDot = this.value.includes('.');
        
        if (hasComma && hasDot) {
            // Если есть и точка, и запятая - оставляем последний символ
            const lastComma = this.value.lastIndexOf(',');
            const lastDot = this.value.lastIndexOf('.');
            if (lastComma > lastDot) {
                this.value = this.value.replace(/\./g, '');
            } else {
                this.value = this.value.replace(/,/g, '');
            }
        }
    });
}

// Применяем ко всем полям с классом .decimal-input
document.addEventListener('DOMContentLoaded', function() {
    document.querySelectorAll('input[type="number"][step="0.01"], .decimal-input').forEach(formatDecimalInput);
});