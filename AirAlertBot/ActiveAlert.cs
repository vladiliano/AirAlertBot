using Discord;

namespace DiscordAlertsBot
{
    public class ActiveAlert
    {
        public string RegionId { get; set; }
        public string RegionType { get; set; }
        public string Type { get; set; }
        public DateTime LastUpdate { get; set; }
        public override string ToString()
        {
            return $"Alert Type: {Type}\n" +
                   $"Alert Last Update: {LastUpdate}\n";
        }
    }

    public class Region
    {
        public string RegionId { get; set; }
        public string RegionType { get; set; }
        public string RegionName { get; set; }
        public DateTime LastUpdate { get; set; }
        public List<ActiveAlert> ActiveAlerts { get; set; }

        public override string ToString()
        {
            return $"Region ID: {RegionId}\n" +
                   $"RegionType: {RegionType}\n" +
                   $"RegionName: {RegionName}\n" +
                   $"LastUpdate: {LastUpdate}\n" +
                   string.Join('\n', ActiveAlerts.Select(x => x.ToString()));
        }

        public string GetAlertType()
        {
            switch (ActiveAlerts[0].Type)
            {
                case "UNKNOWN":
                    return "Невідома";
                case "AIR":
                    return "Балістична";
                case "ARTILLERY":
                    return "Артилерійська";
                case "URBAN_FIGHTS":
                    return "Вуличних боїв";
                case "CHEMICAL":
                    return "Хімічна";
                case "NUCLEAR":
                    return "Ядерна";
                case "INFO":
                    return "Невідома";
            }
            return "Невідома";
        }

        public Embed GetEmbedAlert()
        {
            return new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithName("Ukraine Alerts Bot")
                    .WithIconUrl("https://upload.wikimedia.org/wikipedia/commons/thumb/a/a4/UASA_logo.png/192px-UASA_logo.png"))
                .WithTitle(RegionName)
                .WithFields(new EmbedFieldBuilder()
                    {
                        Name = "Повітряна тривога!",
                        Value = GetAlertType() + " загроза"
                    })
                .WithFooter(new EmbedFooterBuilder()
                    .WithIconUrl("https://upload.wikimedia.org/wikipedia/commons/thumb/a/a4/UASA_logo.png/192px-UASA_logo.png")
                    .WithText($"Станом на {LastUpdate.AddHours(3).ToString("HH:mm   dd/MM/yyyy")}"))
                .WithColor(Discord.Color.Red)
                .Build();
        }

        public Embed GetEmbedEndAlert()
        {
            return new EmbedBuilder()
               .WithAuthor(new EmbedAuthorBuilder()
                   .WithName("Ukraine Alerts Bot")
                   .WithIconUrl("https://upload.wikimedia.org/wikipedia/commons/thumb/a/a4/UASA_logo.png/192px-UASA_logo.png"))
               .WithTitle(RegionName)
               .WithFields(new EmbedFieldBuilder()
               {
                   Name = "Відбій повітряної тривоги!",
                   Value = "Загроза минула"
               })
               .WithFooter(new EmbedFooterBuilder()
                   .WithIconUrl("https://upload.wikimedia.org/wikipedia/commons/thumb/a/a4/UASA_logo.png/192px-UASA_logo.png")
                   .WithText($"Станом на {DateTime.UtcNow.AddHours(3).ToString("HH:mm   dd/MM/yyyy")}"))
               .WithColor(Discord.Color.Green)
               .Build();
        }

        public string GetStringAlert()
        {
            return DateTime.Now.ToString("HH:mm:ss") + $": {RegionName}, Повітряна тривога! {GetAlertType()} загроза. Станом на {LastUpdate.AddHours(3)}";
        }

        public string GetStringEndAlert()
        {
            return DateTime.Now.ToString("HH:mm:ss") + $":{RegionName}, відбій повітряної тривоги! Станом на {DateTime.UtcNow.AddHours(3)}";
        }
    }
}
