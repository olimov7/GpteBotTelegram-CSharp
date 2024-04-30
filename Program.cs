using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot;

namespace ConsoleApp8
{
    internal class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly TelegramBotClient bot = new TelegramBotClient("7026262938:AAFEH-0HCr9UJTr1LgD6QkUGXIJ9Qqoa-k4");

        public class Choice
        {
            public string Text { get; set; }
        }

        public class TextCompletionResponse
        {
            public Choice[] Choices { get; set; }
        }

        public static string ApiKey = "sk-proj-aJt3bTN180wyjAXJxWC0T3BlbkFJ6JpJV6MgR3WetmNAmiq8";
        public static string Url = "https://api.openai.com/v1/completions";
        public static string Model = "gpt-3.5-turbo-instruct";
        public static int Token = 100;
        public static double Temp = 1.0f;

        static async Task Main(string[] args)
        {
            bot.OnMessage += BotOnMessageReceived;
            bot.StartReceiving();
            Console.WriteLine("Бот запущен");
            Console.ReadLine();
            bot.StopReceiving();
        }

        private static async void BotOnMessageReceived(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var message = e.Message;
            var Text = e.Message.Text;

            if (message == null || string.IsNullOrEmpty(message.Text) || message.Type != Telegram.Bot.Types.Enums.MessageType.Text)
                return;

            string prompt = message.Text;

            // сообщение "печатает..."
            await bot.SendChatActionAsync(message.Chat.Id, Telegram.Bot.Types.Enums.ChatAction.Typing);

            // Обработка азапрос
            string response = await OpenAI(ApiKey, Url, Model, Token, Temp, prompt);
            TextCompletionResponse r = JsonConvert.DeserializeObject<TextCompletionResponse>(response);

            string replyText = "";
            if (Text == "/start" || Text == "Привет" || Text == "привет" || Text == "кто ты" || Text == "Кто ты")
            {
                await bot.SendTextMessageAsync(message.Chat.Id, "Привет! Я умный бот, созданный разработчиком Эмомали Олимовым");
                return; 
            }
            else if (r.Choices != null && r.Choices.Length > 0)
            {
                replyText = r.Choices[0].Text;
            }
            else
            {
                replyText = "Ответ не найден";
            }

            // Отправка ответа
            await bot.SendTextMessageAsync(message.Chat.Id, replyText);
        }

        public static async Task<string> OpenAI(string ApiKey_, string Url_, string Model_, int MaxTokens_, double Temperature_, string prompt)
        {
            var requestBody = new
            {
                model = Model_,
                prompt = prompt,
                max_tokens = MaxTokens_,
                temperature = Temperature_
            };

            string json = JsonConvert.SerializeObject(requestBody);

            var request = new HttpRequestMessage(HttpMethod.Post, Url_);
            request.Headers.Add("Authorization", $"Bearer {ApiKey_}");
            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            try
            {
                var httpResponse = await client.SendAsync(request);
                httpResponse.EnsureSuccessStatusCode(); 
                string responseContent = await httpResponse.Content.ReadAsStringAsync();
                return responseContent;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error sending request: {e.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
    }
}