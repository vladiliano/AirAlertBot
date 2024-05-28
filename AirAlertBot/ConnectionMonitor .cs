using Discord.WebSocket;
using Discord;

namespace DiscordAlertsBot
{
    internal class ConnectionMonitor : IDisposable
    {
        private static readonly TimeSpan ReconnectInitialDelay = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan ReconnectDelay = TimeSpan.FromSeconds(3);
        private static readonly TimeSpan ReconnectTimeout = TimeSpan.FromSeconds(20);

        private readonly DiscordSocketClient _discordClient;
        private bool _stopped;
        private Task _reconnectTask;

        public ConnectionMonitor(DiscordSocketClient discordClient)
        {
            _discordClient = discordClient;
            _stopped = false;
        }

        public void Start()
        {
            _discordClient.Disconnected += OnDisconnect;
        }

        public void Stop()
        {
            _stopped = true;
        }

        private async Task OnDisconnect(Exception arg)
        {
            Log($"Discord client disconnected.");

            if (_reconnectTask != null) return;
            _reconnectTask = Task.Run(async () =>
            {
                await Task.Delay(ReconnectInitialDelay);
                await Reconnect();
                _reconnectTask = null;
            });
        }

        public async Task Reconnect()
        {
            if (_stopped) return;

            var isFirstRun = true;

            while (_discordClient.ConnectionState != ConnectionState.Connected)
            {
                if (isFirstRun == false)
                    await Task.Delay(ReconnectDelay);

                isFirstRun = false;

                Log("Attempting to reconnect bot...");

                var timeoutTask = Task.Delay(ReconnectTimeout);
                var connectTask = Task.Run(async () =>
                {
                    await _discordClient.StartAsync();
                    if (_discordClient.ConnectionState != ConnectionState.Connected)
                        await Task.Delay(TimeSpan.FromSeconds(5));
                });
                var task = await Task.WhenAny(timeoutTask, connectTask);

                if (task == timeoutTask)
                {
                    Log($"Bot reconnect timed out (took longer than {ReconnectTimeout.TotalSeconds} seconds). Trying again...");
                    await _discordClient.StopAsync();
                }
                else if (connectTask.IsFaulted)
                {
                    Log(connectTask.Exception);
                    Log($"Bot reconnect failed ({connectTask.Exception}). Trying again...");
                }
            }

            Log($"Bot reconnected successfully!");
        }

        public void Log(string message)
        {
            Console.WriteLine($"[ConnectionMonitor] {message}");
        }

        public void Log(Exception exception)
        {
            Console.WriteLine(exception.ToString());
        }

        public void Dispose()
        {
            if (_reconnectTask != null)
            {
                _reconnectTask.Dispose();
                _reconnectTask = null;
            }
        }
    }
}
