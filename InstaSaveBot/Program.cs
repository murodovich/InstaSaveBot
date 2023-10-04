using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using VideoLibrary;

var botClient = new TelegramBotClient("6526288026:AAEz-ygq6xirm8hya4Rz1yqkuuxz7Xx2spw");

using CancellationTokenSource cts = new();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
ReceiverOptions receiverOptions = new()
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
};

botClient.StartReceiving(
updateHandler: HandleUpdateAsync,
pollingErrorHandler: HandlePollingErrorAsync,
receiverOptions: receiverOptions,
cancellationToken: cts.Token
);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();


async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{

    // Only process Message updates: https://core.telegram.org/bots/api#message
    if (update.Message is not { } message)
        return;
    // Only process text messages
    if (message.Text is not { } messageText)
        return;

    var chatId = message.Chat.Id;
    var firstName = message.From!.FirstName;
    var username = message.From.Username;

    Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

    try
    {
        if (messageText == "/start")
        {
            string startMessage = $"Assalomu alaykum, {firstName} (@{username})!\n\n" +
                "Siz instagramdan video va rasm yuklaydigan botga hush kelibsiz. Quyidagi shartlarga amal qiling:\n" +
                "Yuborayotgan link faqat instagram linki bo'lishi kerak.!\n";

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: startMessage,
                cancellationToken: cancellationToken
            );
        }
        else if (messageText.Contains("instagram"))
        {
            Console.WriteLine("Instagram");

            string replacedMessage = messageText.Replace("www.", "dd");

            //Message message1 = await botClient.SendTextMessageAsync(chatId, replacedMessage);

            Message mes = await botClient.SendVideoAsync(
                chatId: chatId,
                video: InputFile.FromUri(replacedMessage),
                supportsStreaming: true,
                cancellationToken: cancellationToken
            );
        }
        else if (messageText.Contains("you"))
        {
            Console.WriteLine("You Tube");
            YouTube youTube = new YouTube();
            var youtubeVideo = youTube.GetVideo(messageText).Stream();

            await botClient.SendVideoAsync(
                chatId: chatId,
                video: InputFile.FromStream(youtubeVideo),
                supportsStreaming: true,
                cancellationToken: cancellationToken
                );
        }
        else if (messageText.Contains("instagram") || messageText.Contains("utm") || messageText.Contains("img"))
        {
            Console.WriteLine("Image");
            string replacedPhotoMessage = messageText.Replace("www.", "dd");

            Message msg = await botClient.SendPhotoAsync(
                chatId: chatId,
                photo: InputFile.FromUri(replacedPhotoMessage),
                caption: "<b></b>. <i>Source</i>: <a href=\"https://t.me/pandanewdevbot\" > pandanewbot</a>",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        }
        else
        {
            Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "You said:\n" + messageText,
            cancellationToken: cancellationToken);
        }
    }
    catch (Exception ex)
    {
        await Console.Out.WriteLineAsync($"Xatolik: {ex.Message}");
    }



}




Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
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