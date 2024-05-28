using System.Configuration;
using System.Text;
using Newtonsoft.Json;

namespace DiscordAlertsBot
{
    public static class Requests
    {
        private static string Token = ConfigurationManager.ConnectionStrings["UkraineAlertToken"].ConnectionString;

        public static async Task<HttpResponseMessage> GetAlertsJsonAsync()
        {
            await Log("Отправлен запрос получения данных.");
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, "https://api.ukrainealarm.com/api/v3/alerts"))
                {
                    request.Headers.TryAddWithoutValidation("accept", "application/json");
                    request.Headers.TryAddWithoutValidation("Authorization", Token);

                    var response = await httpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        await Log("Сервер успешно вернул данные.");
                        return response;
                    }
                    else
                    {
                        await Log("Сервер не отвечает на запрос получения данных.");
                        throw new Exception("Сервер не отвечает на запрос получения данных.");
                    }
                }
            }
        }

        public static async Task<long> GetAlertsStatusJsonAsync()
        {
            await Log("Отправлен запрос статуса данных.");
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://api.ukrainealarm.com/api/v3/alerts/status"))
                {
                    request.Headers.TryAddWithoutValidation("accept", "application/json");
                    request.Headers.TryAddWithoutValidation("Authorization", Token);

                    var response = await httpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        AlertStatus alertStatus = JsonConvert.DeserializeObject<AlertStatus>(responseContent);
                        await Log("Сервер успешно вернул статус данных.");
                        return alertStatus.LastActionIndex;
                    }
                    else
                    {
                        await Log("Сервер не отвечает на запрос статуса данных.");
                        throw new Exception("Сервер не отвечает на запрос статуса данных.");
                    }
                }
            }
        }

        public class AlertStatus
        {
            public long LastActionIndex { get; set; }
        }

        public static async Task WriteAlertsAsync()
        {
            var regions = await GetAlertsListAsync();

            Console.OutputEncoding = Encoding.UTF8;

            await foreach (var region in regions.ToAsyncEnumerable())
                await Console.Out.WriteLineAsync(region.ToString());
        }

        public static async ValueTask<List<Region>> GetAlertsListAsync()
        {
            var message = await GetAlertsJsonAsync();

            var responseContent = await message.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<Region>>(responseContent);
        }

        private static async Task Log(string text)
        {
            await Console.Out.WriteLineAsync(DateTime.Now.ToString("HH:mm:ss") + ": " + text);
        }
    }
}
