\# Карта модулей проекта Psychologist\_nearby



Автор: Паэгле Д. Д.



\## 1. Назначение



Карта описывает основные модули веб-приложения для психолога, их ответственность и зависимости.



\## 2. Состав модулей



| Модуль | Основные компоненты | Ответственность | Статус |

|---|---|---|---|

| Интерпретатор результатов | `IScoreInterpreter`, `RangeScoreInterpreter`, `FixedScoreInterpreter` | Выбор интерпретации по набранному баллу, подмена реализации | Реализован |

| Страница тестирования | `Testing.razor` | Потребитель: показывает вопросы, собирает ответы, вызывает интерпретатор | Реализован |

| Сервис тестирования | `TestingService` | Чтение тестов из БД, сохранение результатов | Реализован |

| Модели данных | `Test`, `TestQuestion`, `TestAnswer`, `TestInterpretation`, `TestResult` | Модели для EF Core | Реализован |

| Слой данных | `ApplicationDbContext` | Доступ к MySQL через EF Core | Реализован |



\## 3. Зависимости



\- `Testing.razor` → `IScoreInterpreter` (контракт)

\- `Testing.razor` → `TestingService`

\- `RangeScoreInterpreter` → `Test`, `TestInterpretation`

\- `FixedScoreInterpreter` → `TestInterpretation`

\- `TestingService` → `ApplicationDbContext`

\- `ApplicationDbContext` → `Test`, `TestResult` и др.



\## 4. Легенда



\- \*\*Стрелка A → B\*\* — A зависит от контракта B.

\- \*\*Статус «Реализован»\*\* — работает в текущей версии.

\- \*\*Статус «План»\*\* — запланирован на будущее.

