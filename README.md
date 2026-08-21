# TelegramReferralBot

[![CI](https://github.com/Vanchestery/TelegramReferralBot/actions/workflows/ci.yml/badge.svg)](https://github.com/Vanchestery/TelegramReferralBot/actions/workflows/ci.yml)
![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?style=flat-square&logo=postgresql)
![Telegram](https://img.shields.io/badge/Telegram.Bot-22-26A5E4?style=flat-square&logo=telegram)
![Tests](https://img.shields.io/badge/tests-10%20passing-success?style=flat-square)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

Telegram-бот партнёрской («Пригласи друга») программы для онлайн-курсов.
Пользователь выбирает курсы, получает реферальные ссылки и бонусы-кэшбек; партнёрам
доступен личный кабинет и покупка со скидкой по персональному промокоду.

Pet-проект уровня portfolio: многослойная архитектура, async-движок страниц,
интеграция со Stepik API, REST для платёжной системы, 10 unit-тестов.

---

## Highlights (для ревьюера / HR)

| | |
|---|---|
| **Stack** | ASP.NET Core 9 · EF Core · PostgreSQL · Telegram.Bot 22 · Polly · Serilog |
| **Patterns** | Page engine (`IPage` + stack) · webhook · Stepik cache · payment REST webhooks |
| **Quality** | 10 xUnit/Moq tests · snake_case Update deserialization · Polly retry для Stepik |
| **Integrations** | Stepik public API · ngrok webhook · Docker Compose |

---

## Скриншоты

> Скрины добавь в `docs/screenshots/` — см. [инструкцию](docs/screenshots/README.md).

| Каталог курсов | Карточка курса | Личный кабинет |
|----------------|----------------|----------------|
| *(catalog.png)* | *(course.png)* | *(partner.png)* |

---

## Возможности

- **Каталог курсов из Stepik** — список курсов школы и карточка курса (обложка, описание, цена), данные тянутся из Stepik API с кэшированием.
- **Покупка со скидкой** — кнопка ведёт на оплату с персональным промокодом партнёра (`/a/{course}/pay?promo=...`); если промокода нет — обычная оплата (graceful fallback).
- **Реферальная программа** — реферальные ссылки, бонусы/кэшбек, статусы и уровни, личный кабинет партнёра.
- **REST API** — приём вебхуков платёжной системы (оплаты/возвраты), выдача промокодов; защита по API-ключу.
- **Telegram webhook** — приём апдейтов через `SetWebhook` + контроллер.
- **UX** — индикатор «печатает…» / «отправляет фото…» во время запросов, чистая навигация со стеком страниц и кнопкой «Назад».
- **Фоновая рассылка** — ежедневная статистика партнёрам (`BackgroundService`).

## Архитектура

Многослойная (3-tier), Web не обращается к БД напрямую — только через сервисы Core.

| Проект | Назначение |
|--------|-----------|
| `ReferralBot.Core` | Доменные модели, сервисы, интерфейсы, AutoMapper-профили |
| `ReferralBot.Db` | EF Core: `DbContext`, entities, storage-слой, миграции (PostgreSQL) |
| `TelegramReferralBot` | ASP.NET Core: webhook + REST-контроллеры, движок страниц бота, Stepik-клиент, DI, конфигурация |
| `ReferralBot.Tests` | Unit-тесты (xUnit + Moq + FluentAssertions) |

**Движок страниц**: каждая страница реализует `IPage` (`ViewAsync`/`HandleAsync`),
навигация — через стек страниц в контексте пользователя; состояние персистится в БД.

## Технологии

.NET 9 · ASP.NET Core · EF Core 9 + PostgreSQL (Docker) · Telegram.Bot 22 ·
AutoMapper · Serilog · Polly (retry для Stepik) · `IMemoryCache` ·
xUnit / Moq / FluentAssertions.

## Запуск (локально)

**Требуется:** .NET 9 SDK, Docker Desktop, [ngrok](https://ngrok.com) (для webhook),
global tool `dotnet-ef`.

1. **База данных** (PostgreSQL в Docker):
   ```bash
   docker compose up -d db
   ```
2. **Секреты** — задать через user-secrets для проекта `TelegramReferralBot`:
   ```bash
   dotnet user-secrets set "REF_BOT_KEY" "<токен бота от @BotFather>" --project TelegramReferralBot
   dotnet user-secrets set "REF_BOT_WEBHOOK_URL" "https://<ваш-ngrok>.ngrok-free.dev" --project TelegramReferralBot
   dotnet user-secrets set "BOT_USERNAME" "<username бота>" --project TelegramReferralBot
   dotnet user-secrets set "ADMIN_TELEGRAM_ID" "<ваш Telegram ID>" --project TelegramReferralBot
   dotnet user-secrets set "POSTGRES_REFERRALBOT_DB" "Host=localhost;Port=5434;Database=referralbot;Username=postgres;Password=postgres" --project TelegramReferralBot
   dotnet user-secrets set "STEPIK_TEACHER_ID" "596721262" --project TelegramReferralBot
   ```
   Опционально: `STEPIK_CLIENT_ID` / `STEPIK_CLIENT_SECRET` (для авторизованных запросов к Stepik), `PARTNERS_API_KEY` (для REST API).
3. **Миграции**:
   ```bash
   dotnet ef database update --project ReferralBot.Db --startup-project TelegramReferralBot
   ```
4. **Туннель** (webhook требует публичный HTTPS):
   ```bash
   ngrok http 7171
   ```
   URL туннеля прописать в `REF_BOT_WEBHOOK_URL` (см. шаг 2).
5. **Запуск**: F5 в Visual Studio или
   ```bash
   dotnet run --project TelegramReferralBot
   ```
   В логах должно появиться `Webhook configured successfully`. Откройте бота в Telegram и отправьте `/start`.

> Полный список переменных окружения — в [`.env.example`](.env.example). Для прод-развёртывания есть `docker-compose.yml` (бот + БД).

## Тесты

```bash
dotnet test
```

10 unit-тестов (xUnit + Moq): каталог курсов (`CourseService` — фильтрация, кэш, маппинг),
промокоды (`PromoCodeService`), регресс на десериализацию входящего `Update` от Telegram.

## Заметки по реализации

- **Webhook + Telegram.Bot 22**: входящий `Update` приходит в snake_case, поэтому в контроллере он десериализуется через `JsonBotAPI.Options` (а не дефолтным сериализатором ASP.NET Core) — иначе валидация модели рубит запрос с 400.
- **Stepik**: список курсов и детали — публичные эндпоинты (`courses?teacher=`, `courses/{id}`), токен опционален; обложка скачивается из `cover`.
- **Устойчивость**: запросы к Stepik обёрнуты в Polly-retry; ошибки сети не роняют отправку ответа.

## CI/CD

GitHub Actions в `.github/workflows/ci.yml`: push/PR → restore → build (Release) → test.

## Лицензия

[MIT](LICENSE) © 2026.

---

**Связь:** [GitHub профиль](https://github.com/Vanchestery)
