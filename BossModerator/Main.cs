using System.Globalization;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace BossModerator
{
    [ApiVersion(2, 1)]
    public class BossModerator : TerrariaPlugin
    {
        public override Version Version => new(1, 3);
        public override string Name => "BossModerator";
        public override string Author => "TRANQUILZOIIP - github.com/bbeeeeenn";
        public override string Description => "A plugin that moderates boss spawn.";

        public static readonly string path = Path.Join(TShock.SavePath, "BossModeratorConfig.json");
        private Config Config = new();

        public BossModerator(Main game)
            : base(game) { }

        public override void Initialize()
        {
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            ServerApi.Hooks.NpcSpawn.Register(this, OnNpcSpawn);
            GeneralHooks.ReloadEvent += OnReload;
            ServerApi.Hooks.ServerBroadcast.Register(this, OnServerBroadcast);

            Config = Config.Load();
        }

        // Dispose function
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
                ServerApi.Hooks.NpcSpawn.Deregister(this, OnNpcSpawn);
                GeneralHooks.ReloadEvent -= OnReload;
            }
            base.Dispose(disposing);
        }

        private void OnInitialize(EventArgs args)
        {
            Commands.ChatCommands.Add(new Command("bossmod", CheckAllowed, "boss"));
        }

        // Reload hook function
        private void OnReload(ReloadEventArgs e)
        {
            Config = Config.Load();
        }

        // NpcSpawn hook function
        private void OnNpcSpawn(NpcSpawnEventArgs args)
        {
            NPC npc = Main.npc[args.NpcId];
            if (!npc.boss && npc.FullName.ToLower() != "eater of worlds")
                return;

            DateTime DateNow = DateTime.Now;
            // For the Twins
            // Because they are separate bosses, and I don't want two JSON properties for one boss.
            if (npc.FullName.ToLower() == "retinazer" || npc.FullName.ToLower() == "spazmatism")
            {
                int twins_day_num = Config.Schedules["the twins"];
                if (DateNow < Config.StartDate.AddDays(twins_day_num))
                    PreventSpawn(npc, twins_day_num);
            }
            // For the rest
            if (!Config.Schedules.ContainsKey(npc.FullName.ToLower()))
                return;

            int day_num = Config.Schedules[npc.FullName.ToLower()];
            if (DateNow < Config.StartDate.AddDays(day_num))
                PreventSpawn(npc, day_num);
        }

        private void PreventSpawn(NPC npc, int day_num)
        {
            npc.active = false;
            TShock.Utils.Broadcast(
                $"Boss spawn prevented!\n{npc.FullName} is banned until {Config.StartDate.AddDays(day_num).ToLongDateString()}, {Config.StartDate.AddDays(day_num).ToShortTimeString()}.",
                Color.Chocolate
            );
        }

        // Command function
        private void CheckAllowed(CommandArgs args)
        {
            DateTime DateNow = DateTime.Now;
            TSPlayer player = args.Player;

            List<string> allowed_boss = new() { "Currently Allowed Bosses:" };
            foreach (string boss in Config.Schedules.Keys)
            {
                if (Config.StartDate.AddDays(Config.Schedules[boss]) < DateNow)
                {
                    allowed_boss.Add($"-{CapitalizeEachWord(boss)}");
                }
            }

            string message = string.Join("\n", allowed_boss);

            if (player != null && player.Active)
                player.SendMessage(message, Color.Chocolate);
            else
                TShock.Log.ConsoleInfo(message);
        }

        private void OnServerBroadcast(ServerBroadcastEventArgs args)
        {
            var text = args.Message.ToString();

            if (text.EndsWith(" has awoken!"))
            {
                string bossName = text[..text.IndexOf(" has awoken!")];

                foreach (NPC npc in Main.npc)
                {
                    if (!npc.boss)
                        continue;
                    if (npc.FullName.StartsWith(bossName) && !npc.active)
                    {
                        args.Handled = true;
                    }
                }
            }
        }

        // Utility
        public static string CapitalizeEachWord(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(str.ToLower());
        }
    }
}
