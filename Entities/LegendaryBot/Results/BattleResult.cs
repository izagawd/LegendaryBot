using DSharpPlus.Entities;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;

namespace Entities.LegendaryBot.Results;

public class BattleResult
{
    public bool Stopped { get; init; }

    public required DiscordMessage Message { get; init; }


    public required CharacterTeam Winners { get; init; }

    public required int Turns { get; init; }
    public required CharacterTeam? TimedOut { get; init; }
    public CharacterTeam? Forfeited { get; init; }
}