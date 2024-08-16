using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;

//best arg to listen to for counter attacks or extra attacks
public class CharacterPostUseMoveEventArgs : BattleEventArgs
{
    public CharacterPostUseMoveEventArgs(MoveUsageResult moveUsageResult)
    {
        MoveUsageResult = moveUsageResult;
    }

    public Character User => MoveUsageResult.Move.User;
    public Move Move => MoveUsageResult.Move;
    public MoveUsageResult MoveUsageResult { get; }

    public IEnumerable<DamageResult> DamageResults => MoveUsageResult.DamageResults;
}