using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Moves;

public abstract class Skill : Special
{
    public Skill(Character user) : base(user)
    {
    }
}