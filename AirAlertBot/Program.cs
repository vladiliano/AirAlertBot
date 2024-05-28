using System.Configuration;
using Discord.WebSocket;
using Discord;
using System.Text;

namespace DiscordAlertsBot
{
    internal class Program
    {
        public static DiscordSocketClient Client = new DiscordSocketClient();

        public static Dictionary<ulong, Guild> Guilds = new Dictionary<ulong, Guild>();

        public static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public static AlertsInfo _AlertsInfo = null;

        public static async Task Main(string[] args)
        {
            var program = new Program();
            await program.MainAsync();
        }

        private async Task MainAsync()
        {
            try
            {
                var config = new DiscordSocketConfig
                {
                    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
                };
                Client = new DiscordSocketClient(config);

                Client.Ready += Client_Ready;
                Client.Log += Log;
                Client.JoinedGuild += Client_JoinedGuild;
                Client.LeftGuild += Client_LeftGuild;
                Client.ChannelDestroyed += Client_ChannelDestroyed;

                var token = ConfigurationManager.ConnectionStrings["BotToken"].ConnectionString;

                await Client.LoginAsync(TokenType.Bot, token);
                await Client.StartAsync();

                var connectionMonitor = new ConnectionMonitor(Client);
                connectionMonitor.Start();

                await Task.Delay(-1, cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.ToString());
            }
        }

        private async Task Client_ChannelDestroyed(SocketChannel arg)
        {
            try
            {
                if (arg.GetChannelType() == ChannelType.Text)
                {
                    var textChannel = (SocketTextChannel)arg;
                    var currentGuild = Guilds[textChannel.Guild.Id];

                    if (currentGuild._TextChannel.Id.Equals(arg.Id))
                        await currentGuild.CreateTextChannelAsync();
                }
                else if(arg.GetChannelType() == ChannelType.Category)
                {
                    var categoryChannel = (SocketCategoryChannel)arg;
                    var currentGuild = Guilds[categoryChannel.Guild.Id];
                    await currentGuild._TextChannel.DeleteAsync();

                    if (currentGuild._CategoryChannel.Id.Equals(arg.Id))
                        await currentGuild.CreateCategoryChannelAsync();
                }
            }
            catch(Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
            }
        }

        private async Task Log(LogMessage message)
        {
            await Console.Out.WriteLineAsync(message.ToString());
        }

        private async Task Client_Ready()
        {
            try
            {
                var guilds = Client.Guilds.ToAsyncEnumerable();

                await foreach (var guild in guilds)
                    Guilds.Add(guild.Id, new Guild(guild));


                Console.OutputEncoding = Encoding.UTF8;

                _AlertsInfo = new AlertsInfo();
                _AlertsInfo.StartViewAlerts();
            }
            catch(Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.ToString());
            }
        }

        private async Task Client_JoinedGuild(SocketGuild arg)
        {
            try
            {
                Guilds.Add(arg.Id, new Guild(arg));
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
            }
        }

        private async Task Client_LeftGuild(SocketGuild arg)
        {
            try
            {
                var categoryChannel = Guilds[arg.Id]._CategoryChannel;
                var textChannel = Guilds[arg.Id]._TextChannel;

                if (categoryChannel != null)
                    await categoryChannel.DeleteAsync();

                if (textChannel != null)
                    await textChannel.DeleteAsync();

                Guilds.Remove(arg.Id);
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
            }
        }
    }
}
