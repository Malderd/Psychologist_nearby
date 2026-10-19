# Карта модулей проекта Psychologist_nearby

**Автор:** Куликов К. К.

## 1. Назначение
Карта описывает основные модули веб-приложения для психолога, их ответственность и зависимости (в рамках модуля аутентификации).

## 2. Состав модулей

| Модуль | Основные компоненты | Ответственность | Зависимости | Статус |
| :--- | :--- | :--- | :--- | :--- |
| **Модуль валидации** | `IAuthValidator`, `RegexAuthValidator`, `FixedAuthValidator` | Проверка и нормализация данных (email, пароль, ФИО, телефон). | Нет (только стандартная библиотека) | Реализован |
| **Страница регистрации** | `Register.razor` | Сбор данных, вызов валидатора, сохранение в БД. | `IAuthValidator`, `ApplicationDbContext` | Реализован |
| **Страница входа** | `Login.razor` | Сбор данных, проверка пароля, создание Cookie. | `IAuthValidator`, `ApplicationDbContext` | Реализован |
| **Страница профиля** | `PersonalAccount.razor` | Отображение и редактирование данных пользователя. | `IAuthValidator`, `ApplicationDbContext` | Реализован |
| **Слой данных** | `ApplicationDbContext` | Взаимодействие с MySQL через EF Core. | Pomelo MySQL | Реализован |

## 3. Направление зависимостей
Стрелки означают зависимость исходного кода (Потребитель -> Контракт):
*   `Register.razor` -> `IAuthValidator`
*   `Login.razor` -> `IAuthValidator`
*   `PersonalAccount.razor` -> `IAuthValidator`
*   `Program.cs` (точка сборки) -> `RegexAuthValidator` (или `FixedAuthValidator`)
