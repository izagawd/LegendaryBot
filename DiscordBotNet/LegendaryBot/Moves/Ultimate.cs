using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Moves;

public abstract class Ultimate : Special
{
    protected Ultimate(Character user) : base(user)
    {
    }
}