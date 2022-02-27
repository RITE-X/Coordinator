using Coordinator.App;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

var botClient = new TelegramBotClient(BotSettingsInit.Token);

using var cts = new CancellationTokenSource();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = { } // receive all update types
};
botClient.StartReceiving(
    HandleUpdateAsync,
    HandleErrorAsync,
    receiverOptions,
    cancellationToken: cts.Token);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");

while (true)
{
    if (Console.ReadLine() != "stop") continue;
    cts.Cancel();
    break;
}


async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    try
    {
        if (update.Type != UpdateType.Message)
            return;


        var chatId = update.Message.Chat.Id;
        var message = update.Message;

        if (message.Text == "/start")
        {
            await botClient.SendTextMessageAsync(chatId,
                "Солдат из Беларуси и России! Это не твоя война. Ты не выбирал эту войну и агрессию. " +
                "Мы знаем что тебя доставили на Украинскую землю силой и обманом. Мы не хотим тебя убивать." +
                " Бросай оружие и сохрани себе жизнь!");

            await botClient.SendTextMessageAsync(chatId,
                "Что делать если против воли насильно попал на территорию Украины?!\n\n • Избегай участия в боевых действиях\n • Не стреляй\n • Сложи оружие и обратись к военнослужащим Украинской Армии\n • Отправь сюда фото с геолокацией\n • С поднятыми руками иди по направлению к ближайшему населенному пункту");
            
            

            // await botClient.SendTextMessageAsync(chatId, "https://t.me/surrender_and_survive");
            return;
        }

        if (chatId == BotSettingsInit.AdminChatId)
        {
            if (message.ReplyToMessage is null)
            {
                await botClient.SendTextMessageAsync(BotSettingsInit.AdminChatId,
                    "Используйте кнопку ответить, для отправки сообщений конкретным пользователям",
                    cancellationToken: cancellationToken);
                return;
            }
                
            if (message.ReplyToMessage.ForwardFrom is null)
            {
                await botClient.SendTextMessageAsync(BotSettingsInit.AdminChatId,
                    "Вы не можете ответить на сообщение пользователя со скрытым профилем",
                    cancellationToken: cancellationToken);
                return;
            }


            var replyToMessage = message.ReplyToMessage;
            await botClient.SendTextMessageAsync(replyToMessage.ForwardFrom.Id, message.Text,
                cancellationToken: cancellationToken);
        }
        else
        {
            if (message.Chat.Type != ChatType.Private)
                return;

            await botClient.ForwardMessageAsync(BotSettingsInit.AdminChatId, chatId, message.MessageId,
                cancellationToken: cancellationToken);
            await botClient.SendTextMessageAsync(chatId, "Ваше сообщение доставлено, ожидайте ответа координатора",
                cancellationToken: cancellationToken);
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
}

Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var errorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(errorMessage);
    return Task.CompletedTask;
}