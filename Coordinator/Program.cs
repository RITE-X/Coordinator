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
    if (update.Type != UpdateType.Message)
        return;


    var chatId = update.Message.Chat.Id;
    var message = update.Message;

    if (message.Text == "/start")
    {
    }

    if (chatId == BotSettingsInit.AdminChatId)
    {
        if (message.ReplyToMessage is null)
            return;
        
        if (message.ReplyToMessage.ForwardFrom is null)
        {
            await botClient.SendTextMessageAsync(BotSettingsInit.AdminChatId, "Ви не можете відповісти на повідомлення користувача з прихованим профілем",
                cancellationToken: cancellationToken);
            return;
        }


        var replyToMessage = message.ReplyToMessage;
        await botClient.SendTextMessageAsync(replyToMessage.ForwardFrom.Id, message.Text, cancellationToken: cancellationToken);
    }
    else
    {
        await botClient.ForwardMessageAsync(BotSettingsInit.AdminChatId, chatId, message.MessageId,
            cancellationToken: cancellationToken);
    }
}

Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}