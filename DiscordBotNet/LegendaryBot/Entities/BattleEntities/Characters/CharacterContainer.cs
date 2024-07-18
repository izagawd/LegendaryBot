using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Entities.Items;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;





public class CharacterContainer : InventoryEntityContainer<Character>
{
    public override void MergeDuplicates()
    {
        var characters = List.ToArray();
        List.Clear();
        foreach (var i in characters)
        {
            var already = List.FirstOrDefault(j => j.GetType() == i.GetType());
            if (already is not null)
            {
                already.DupeCount += i.DupeCount + 1;
            }
            else
            {
                Add(i);
            }
        }
    }
}