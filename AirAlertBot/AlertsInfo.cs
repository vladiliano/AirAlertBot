using Discord;

namespace DiscordAlertsBot
{
    public class AlertsInfo
    {
        private List<Region> PastInfo = new List<Region>();
        private long PastId = 0;

        public void StartViewAlerts()
        {
            TimeSpan initialDelay = TimeSpan.FromSeconds(60 - DateTime.Now.Second);

            TimerCallback timerCallback = new TimerCallback(ExecuteActionAsync);
            Timer timer = new Timer(timerCallback, null, initialDelay, TimeSpan.FromMinutes(1));

            TimerCallback timerUpdateGuildCallback = new TimerCallback(UpdateGuildAsync);
            Timer timerGuildUpdate = new Timer(timerUpdateGuildCallback, null, initialDelay, TimeSpan.FromMinutes(30));

            Console.ReadLine();
        }

        private async void UpdateGuildAsync(object? state)
        {
            try
            {
                var guilds = Program.Guilds.Values.ToAsyncEnumerable();

                await foreach (var guild in guilds)
                    await guild._TextChannel.ModifyAsync(x => x.Flags = ChannelFlags.None);
            }
            catch(Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.ToString());
            }
        }

        private async void ExecuteActionAsync(object? state)
        {
            try
            {
                var presentId = await Requests.GetAlertsStatusJsonAsync();

                if (PastId != presentId)
                {
                    await Log("Внесение изменений.");
                    var presentInfo = await Requests.GetAlertsListAsync();

                    PastId = presentId;

                    await Task.WhenAll(
                        CompareAndNotifyAlertChangesAsync(presentInfo,PastInfo, true),
                        CompareAndNotifyAlertChangesAsync(PastInfo, presentInfo, false));

                    PastInfo = presentInfo;

                    await Log("Конец внесения изменений.");
                }
                else await Log("Без изменений");
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.ToString());
            }
        }

        private async Task CompareAndNotifyAlertChangesAsync(List<Region> regions1, List<Region> regions2, bool airAlert)
        {
            for (int i = 0; i < regions1.Count; i++)
            {
                var currentRegion = regions1[i];
                if (!regions2.Any(x => x.RegionId == currentRegion.RegionId))
                {
                    await Console.Out.WriteLineAsync(airAlert? currentRegion.GetStringAlert() : currentRegion.GetStringEndAlert());
                    await SendAlertToAllGuildsAsync(currentRegion, airAlert);
                }
            }
        }

        private async Task SendAlertToAllGuildsAsync(Region region, bool airAlert)
        {
            Embed alertEmbed = airAlert ? region.GetEmbedAlert() : region.GetEmbedEndAlert();

            var guilds = Program.Guilds.Values.ToAsyncEnumerable();

            List<Task> tasks = new List<Task>();

            await foreach (var guild in guilds)
            {
                tasks.Add(guild._TextChannel.SendMessageAsync(embed: alertEmbed));

                tasks.Add(guild.ConnectBotToVoiceAndTurnOnAlarmAsync(region.RegionId, region.GetAlertType(), airAlert));
            }
            await Log("Отправка текста и воспроизведение аудио.");
            tasks.ForEach(async guild => guild.Wait());
            await Log("Конец отправки текста и воспроизведения аудио.");
        }

        private async Task Log(string text)
        {
            await Console.Out.WriteLineAsync(DateTime.Now.ToString("HH:mm:ss") + ": " + "Обновление данных - " + text);
        }
    }
}
