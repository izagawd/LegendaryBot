using DiscordBotNet.LegendaryBot.Results;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public interface IDetonatable
{
    /// <param name="detonator">
    ///     The character that initiated the detonation.
    ///     It will be the character that inflicted the debuff if the detonation was triggered automatically
    /// </param>
    /// <returns>The damage result of the detonation</returns>
    public DamageResult? Detonate(Character detonator);
}