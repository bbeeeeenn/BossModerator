using IL.Terraria.GameContent.RGB;
using Newtonsoft.Json;

namespace BossModerator
{
    public class Config
    {
        public DateTime StartDate = DateTime.Now;
        public Dictionary<string, int> Schedules = new()
        {
            { "king slime", 0 },
            { "deerclops", 0 },
            { "duke fishron", 0 },
            { "empress of light", 0 },
            { "eye of cthulhu", 1 },
            { "queen bee", 2 },
            { "eater of worlds", 3 },
            { "brain of cthulhu", 3 },
            { "skeletron", 4 },
            { "wall of flesh", 5 },
            { "queen slime", 5 },
            { "the twins", 6 },
            { "skeletron prime", 7 },
            { "the destroyer", 8 },
            { "plantera", 9 },
            { "golem", 10 },
            { "lunatic cultist", 11 },
            { "moon lord", 12 },
        };

        public Config Load()
        {
            if (!File.Exists(BossModerator.path))
            {
                File.WriteAllText(
                    BossModerator.path,
                    JsonConvert.SerializeObject(this, Formatting.Indented)
                );
                return new Config();
            }
            ;
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(BossModerator.path));
        }
    }
}
