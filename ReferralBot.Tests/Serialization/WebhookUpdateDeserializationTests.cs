using System.Text.Json;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ReferralBot.Tests.Serialization;

/// <summary>
/// Регресс на пойманный баг: входящий Update от Telegram приходит в snake_case
/// (first_name, message_id, ...) и должен десериализоваться через JsonBotAPI.Options.
/// Стандартный сериализатор ASP.NET Core не мапил first_name → FirstName, поле выходило
/// пустым, валидация модели рубила запрос 400 ещё до BotService. Тест фиксирует, что
/// «правильный» сериализатор корректно читает поля апдейта.
/// </summary>
public class WebhookUpdateDeserializationTests
{
    private const string StartUpdateJson =
        """
        {
          "update_id": 721953790,
          "message": {
            "message_id": 1705,
            "from": { "id": 850843978, "is_bot": false, "first_name": "Ivan", "username": "Vanchestery", "language_code": "ru" },
            "chat": { "id": 850843978, "first_name": "Ivan", "username": "Vanchestery", "type": "private" },
            "date": 1780775668,
            "text": "/start",
            "entities": [ { "offset": 0, "length": 6, "type": "bot_command" } ]
          }
        }
        """;

    [Fact]
    public void Deserialize_StartUpdate_PopulatesSnakeCaseFields()
    {
        var update = JsonSerializer.Deserialize<Update>(StartUpdateJson, JsonBotAPI.Options);

        update.Should().NotBeNull();
        update!.Message.Should().NotBeNull();
        update.Message!.From.Should().NotBeNull();
        update.Message.From!.FirstName.Should().Be("Ivan");     // first_name → FirstName
        update.Message.Text.Should().Be("/start");
        update.Message.Chat.Id.Should().Be(850843978);
        update.Message.Entities.Should().ContainSingle(e => e.Type == MessageEntityType.BotCommand);
    }

    [Fact]
    public void Deserialize_StartUpdate_IsRecognizedAsMessageUpdate()
    {
        var update = JsonSerializer.Deserialize<Update>(StartUpdateJson, JsonBotAPI.Options);

        update!.Type.Should().Be(UpdateType.Message);
    }
}
