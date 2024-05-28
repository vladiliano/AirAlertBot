using Discord;
using Discord.Audio;
using Discord.WebSocket;
using NAudio.Wave;

namespace DiscordAlertsBot
{
    internal class Guild
    {
        public readonly SocketGuild _Guild;
        public ITextChannel _TextChannel { get; private set; }
        public ICategoryChannel _CategoryChannel { get; private set; }

        private const string _audioPath = @"C:\Users\vladik\source\repos\AirAlertBot\AirAlertBot\audio\";

        public Guild(SocketGuild guild)
        {
            _Guild = guild;
            CreateCategoryChannelAsync().Wait();
            CreateTextChannelAsync().Wait();
        }

        public async Task CreateTextChannelAsync()
        {
            if (!GuildContainsTextChannel())
            {
                _TextChannel = await _Guild.CreateTextChannelAsync("🇺🇦-повітряні-тривоги", x => x.CategoryId = _CategoryChannel.Id);
                await _TextChannel.SendMessageAsync("Вітаю!");
            }
            else _TextChannel = _Guild.TextChannels.FirstOrDefault(x => x.Name == "🇺🇦-повітряні-тривоги");
        }

        public async Task CreateCategoryChannelAsync()
        {
            if (!GuildContainsCategoryChannel())
                _CategoryChannel = await _Guild.CreateCategoryChannelAsync("UKRAINE ALERTS");
            else _CategoryChannel = _Guild.CategoryChannels.FirstOrDefault(x => x.Name == "UKRAINE ALERTS");
        }

        public async Task ConnectBotToVoiceAndTurnOnAlarmAsync(string regionId, string alertType, bool airAlert)
        {
            var voiceChannel = GetVoiceChannelWithMostUser();

            if (voiceChannel == null || voiceChannel.Equals(_Guild.AFKChannel))
                return;

            var audioClient = await GetAudioClient(voiceChannel);

            if (audioClient == null)
                return;

            try
            {
                var RegionAudioFilePath = airAlert
                    ? GetRegionAlertAudioPath(regionId)
                    : GetRegionEndAlertAudioPath(regionId);

                if (airAlert)
                {
                    var alertTypeAudioFilePath = GetAlertTypeAudioPath(alertType);
                    await StartPlayingAudioAsync(audioClient, RegionAudioFilePath);
                    await StartPlayingAudioAsync(audioClient, alertTypeAudioFilePath);
                }
                else await StartPlayingAudioAsync(audioClient, RegionAudioFilePath);
            }
            catch (Exception ex)
            {
                await Log($"{ex} \n voiceChannel: {voiceChannel}");
            }
            finally
            {
                await voiceChannel.DisconnectAsync();
            }
        }

        private async Task<IAudioClient> GetAudioClient(SocketVoiceChannel voiceChannel)
        {
            IAudioClient audioClient = null;
            try
            {
                audioClient = await voiceChannel.ConnectAsync();
            }
            catch (Exception ex)
            {
                await Log($"voiceChannel: {voiceChannel} \n {ex}");
                return null;
            }
            return audioClient;
        }

        public async Task StartPlayingAudioAsync(IAudioClient audioClient, string audioFilePath)
        {
            if (audioFilePath == null || audioClient == null)
            {
                await Log("Путь к аудиофайлу или клиент аудио равен null.");
                return;
            }

            if (!File.Exists(audioFilePath))
            {
                await Log($"Файл не найден: {audioFilePath}");
                return;
            }

            var cancellationToken = Program.cancellationTokenSource.Token;
            var audioFormat = new WaveFormat(108000, 16, 1);

            try
            {
                using (var audioOutStream = audioClient.CreatePCMStream(AudioApplication.Mixed))
                using (var audioFile = new AudioFileReader(audioFilePath))
                using (var resampler = new MediaFoundationResampler(audioFile, audioFormat))
                {
                    resampler.ResamplerQuality = 60;
                    audioFile.Volume = 0.3f;

                    byte[] buffer = new byte[audioFile.Length];
                    int bytesRead;

                    while ((bytesRead = resampler.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        if (audioClient.ConnectionState != ConnectionState.Connected)
                            break;

                        await audioOutStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                    }

                    await audioOutStream.FlushAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                await Log("Задача была отменена");
            }
            catch (Exception ex)
            {
                await Log($"Ошибка при воспроизведении аудио: {ex.Message}\nСтек вызовов: {ex.StackTrace}");
            }
        }

        public SocketVoiceChannel GetVoiceChannelWithMostUser() => 
            _Guild.VoiceChannels
           .Where(x => x.ConnectedUsers.Count != 0)
           .OrderBy(x => x.ConnectedUsers.Count)
           .FirstOrDefault();

        private string GetRegionAlertAudioPath(string regionId)
        {
            switch (regionId)
            {
                case "3":
                    return _audioPath + @"\Увага Хмельницька об.mp3";
                case "4":
                    return _audioPath + @"\Увага Вінницька обла.mp3";
                case "5":
                    return _audioPath + @"\Увага Рівненська обл.mp3";
                case "8":
                    return _audioPath + @"\Увага Волинська обла.mp3";
                case "9":
                    return _audioPath + @"\Увага Дніпропетровсь.mp3";
                case "10":
                    return _audioPath + @"\Увага Житомирська об.mp3";
                case "11":
                    return _audioPath + @"\Увага Закарпатська о.mp3";
                case "12":
                    return _audioPath + @"\Увага Запорізька обл.mp3";
                case "13":
                    return _audioPath + @"\Увага Івано Франківс.mp3";
                case "14":
                    return _audioPath + @"\Увага Київська облас.mp3";
                case "15":
                    return _audioPath + @"\Увага Кіровоградська.mp3";
                case "16":
                    return _audioPath + @"\Увага Луганська обла.mp3";
                case "17":
                    return _audioPath + @"\Увага Миколаївська о.mp3";
                case "18":
                    return _audioPath + @"\Увага Одеська област.mp3";
                case "19":
                    return _audioPath + @"\Увага Полтавська обл.mp3";
                case "20":
                    return _audioPath + @"\Увага Сумська област.m4a";
                case "21":
                    return _audioPath + @"\Увага Тернопільська .m4a";
                case "22":
                    return _audioPath + @"\Увага Харківська обл.m4a";
                case "23":
                    return _audioPath + @"\Увага Херсонська обл.m4a";
                case "24":
                    return _audioPath + @"\Увага Черкаська обла.m4a";
                case "25":
                    return _audioPath + @"\Увага Чернігівська о.m4a";
                case "26":
                    return _audioPath + @"\Увага Чернівецька об.m4a";
                case "27":
                    return _audioPath + @"\Увага Львівська обла.m4a";
                case "28":
                    return _audioPath + @"\Увага Донецька облас.m4a";
                case "31":
                    return _audioPath + @"\Увага Місто Київ Ого.m4a";
                case "351":
                    return _audioPath + @"\Увага Місто Нікополь.m4a";
                case "9999":
                    return _audioPath + @"\Увага Кримська Автон.m4a";
                default:
                    return null;
            }
        }

        private string GetAlertTypeAudioPath(string alertType)
        {
            switch (alertType)
            {
                case "Невідома":
                    return _audioPath + @"\Невідома загроза .m4a";
                case "Балістична":
                    return _audioPath + @"\Загроза застосування.m4a";
                case "Артилерійська":
                    return _audioPath + @"\Загроза застосування (1).m4a";
                case "Вуличних боїв":
                    return _audioPath + @"\Загроза вуличних бої.m4a";
                case "Хімічна":
                    return _audioPath + @"\Загроза застосування (2).m4a";
                case "Ядерна":
                    return _audioPath + @"\Загроза застосування (3).m4a";
                default:
                    return _audioPath + @"\Невідома загроза .m4a";
            }
        }

        private string GetRegionEndAlertAudioPath(string regionId)
        {
            switch (regionId)
            {
                case "3":
                    return _audioPath + @"\Хмельницька область .m4a";
                case "4":
                    return _audioPath + @"\Вінницька область За.m4a";
                case "5":
                    return _audioPath + @"\Рівненська область З.m4a";
                case "8":
                    return _audioPath + @"\Волинська область За.m4a";
                case "9":
                    return _audioPath + @"\Дніпропетровська обл.m4a";
                case "10":
                    return _audioPath + @"\Житомирська область .m4a";
                case "11":
                    return _audioPath + @"\Закарпатська область.m4a";
                case "12":
                    return _audioPath + @"\Запорізька область З.m4a";
                case "13":
                    return _audioPath + @"\Івано Франківська об.m4a";
                case "14":
                    return _audioPath + @"\Київська область Заг.m4a";
                case "15":
                    return _audioPath + @"\Кіровоградська облас.m4a";
                case "16":
                    return _audioPath + @"\Луганська область За.m4a";
                case "17":
                    return _audioPath + @"\Миколаївська область.m4a";
                case "18":
                    return _audioPath + @"\Одеська область Загр.m4a";
                case "19":
                    return _audioPath + @"\Полтавська область З.m4a";
                case "20":
                    return _audioPath + @"\Сумська область Загр.m4a";
                case "21":
                    return _audioPath + @"\Тернопільська област.m4a";
                case "22":
                    return _audioPath + @"\Харківська область З.m4a";
                case "23":
                    return _audioPath + @"\Херсонська область З.m4a";
                case "24":
                    return _audioPath + @"\Черкаська область За.m4a";
                case "25":
                    return _audioPath + @"\Чернігівська область.m4a";
                case "26":
                    return _audioPath + @"\Чернівецька область .m4a";
                case "27":
                    return _audioPath + @"\Львівська область.m4a";
                case "28":
                    return _audioPath + @"\Донецька область Заг.m4a";
                case "31":
                    return _audioPath + @"\Місто Київ Загроза м.m4a";
                case "351":
                    return _audioPath + @"\Місто Нікополь та Ні.m4a";
                case "9999":
                    return _audioPath + @"\Кримська Автономна Р.m4a";
                default:
                    return null;
            }
        }

        public bool GuildContainsTextChannel() => _Guild.TextChannels.Any(x => x.Name == "🇺🇦-повітряні-тривоги");

        public bool GuildContainsCategoryChannel() => _Guild.CategoryChannels.Any(x => x.Name == "UKRAINE ALERTS");

        private static async Task Log(string text)
        {
            await Console.Out.WriteLineAsync(DateTime.Now.ToString("HH:mm:ss") + ": " + text);
        }
    }
}
