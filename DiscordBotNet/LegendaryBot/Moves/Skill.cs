using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Moves;


public abstract class Skill : Special
{
    protected Skill(Character user) : base(user)
    {
    }


}