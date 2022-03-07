using Coordinator.App;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

var botClient = new TelegramBotClient(BotSettingsInit.Token);

using var cts = new CancellationTokenSource();

var database =
    new Database(
        "Server=based.c9vilnnbkwfj.eu-central-1.rds.amazonaws.com;Encrypt=false;Database=Maxi;User Id=devbase;Password=sexwithnig22;");


var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = new[] {UpdateType.Message}
};
botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cts.Token);

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
        var userId = update.Message.From.Id;
        var firstName = update.Message.From.FirstName;


        var message = update.Message;

        switch (message.Text)
        {
            case "/start":
                await botClient.SendTextMessageAsync(chatId,
                    "Солдат из Беларуси и России! Это не твоя война. Ты не выбирал эту войну и агрессию. " +
                    "Мы знаем что тебя доставили на Украинскую землю силой и обманом. Мы не хотим тебя убивать." +
                    " Бросай оружие и сохрани себе жизнь!");

                await botClient.SendTextMessageAsync(chatId,
                    "Что делать если против воли насильно попал на территорию Украины?!\n\n • Избегай участия в боевых действиях\n • Не стреляй\n • Сложи оружие и обратись к военнослужащим Украинской Армии\n • Отправь сюда фото с геолокацией\n • С поднятыми руками иди по направлению к ближайшему населенному пункту");

                return;
            case "/stop":
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

            var replyToMessage = message.ReplyToMessage;
            var selectedUser = await database.SelectUserByMessage(replyToMessage.MessageId);

            if (selectedUser is null)
            {
                await botClient.SendTextMessageAsync(BotSettingsInit.AdminChatId, "Ошибка действия",
                    cancellationToken: cancellationToken);
                return;
            }

            if (message.Text is null)
            {
                await botClient.SendTextMessageAsync(BotSettingsInit.AdminChatId,
                    "Вы не можете отправить пустое сообщение", cancellationToken: cancellationToken);
                return;
            }


            await botClient.SendTextMessageAsync(selectedUser.TelegramId, message.Text,
                cancellationToken: cancellationToken);
        }
        else
        {
            if (message.Chat.Type != ChatType.Private)
                return;

            var selectedUser = await database.SelectUser(userId);

            if (selectedUser is null)
                await database.InsertUser(userId, firstName);


            int sentMessageId ;



            switch (message.Type)
            {
                case MessageType.Text:
                    sentMessageId = (await botClient.SendTextMessageAsync(BotSettingsInit.AdminChatId, $"Отправлено: {firstName}\n\n{message.Text}", cancellationToken: cancellationToken)).MessageId;
                    break;
                case MessageType.Location:
                    sentMessageId = (await botClient.SendVenueAsync(BotSettingsInit.AdminChatId,message.Location.Latitude, message.Location.Longitude,$"Отправлено: {firstName}" ,"")).MessageId;
                    break;
                case MessageType.Photo:
                    sentMessageId = (await botClient.CopyMessageAsync(BotSettingsInit.AdminChatId, userId,message.MessageId,$"Отправлено: {firstName}\n\n{message.Caption}")).Id;
                    break;
                default:
                    await botClient.SendTextMessageAsync(chatId, "Тип сообщения не поддерживается",
                        cancellationToken: cancellationToken);
                    return;
            }

            await database.InsertMessage(userId, sentMessageId);
         
            
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