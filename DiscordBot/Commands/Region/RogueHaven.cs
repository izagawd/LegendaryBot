using BasicFunctionality;
using Entities.LegendaryBot;
using Entities.LegendaryBot.Entities.BattleEntities.Blessings;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Rewards;

namespace DiscordBot.Commands.Region;

public class RogueHaven : Region
{
    public override string Name => "Rogue Haven";

    public override IEnumerable<Type> ObtainableCharacters =>
    [
        typeof(Roxy),
        typeof(Takeshi), typeof(CommanderJean)
    ];

    public override IEnumerable<Reward> GetRewardsAfterBattle(Character main, Tier combatTier)
    {
        throw new NotImplementedException();
    }

    public override Team GenerateCharacterTeamFor(Type type, out Character main, Tier combatTier)
    {
        main = (Character)Activator.CreateInstance(type)!;
        NonPlayerTeam newTeam = [main];
        switch (main)
        {
            case Roxy:
                main.Blessing = new VitalForce();
                foreach (var i in Enumerable.Range(0,GetSmallFryEnemyCount(combatTier)))
                {
                    var createdChar = new Delinquent();
                    newTeam.Add(createdChar);
                }

                break;
            case Takeshi:
                main.Blessing = new VitalForce();
                break;

            case CommanderJean:
                main.Blessing = new HeadStart();
                foreach (var i in Enumerable.Range(0, GetSmallFryEnemyCount(combatTier)))
                {
                    var createdChar = new Police();
                    newTeam.Add(createdChar);
                }

                break;
        }

        foreach (var i in newTeam) i.SetBotStatsAndLevelBasedOnTier(combatTier);

        return newTeam;
    }
}