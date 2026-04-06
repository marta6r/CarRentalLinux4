#!/bin/bash

# Ждем, пока создастся папка
sleep 2

# Копируем базу данных если она есть в проекте
if [ -f /app/carrental.db ]; then
    cp /app/carrental.db /app/data/carrental.db
    echo "База данных скопирована"
else
    echo "База данных не найдена, будет создана при первом запуске"
fi