using BasicFunctionality;
using Entities.LegendaryBot.Entities.BattleEntities.Blessings;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

namespace DiscordBot.Commands.Region;

public class BurgsKingdom : Region
{
    public override string Name => "Burgs Kingdom";

    public override IEnumerable<Type> ObtainableCharacters =>
    [
        typeof(RoyalKnight),
        typeof(Blast), typeof(Thana), typeof(Cheerleader)
    ];

    public override Team GenerateCharacterTeamFor(Type type, out Character main)
    {
        main = (Character)Activator.CreateInstance(type)!;
        NonPlayerTeam newTeam = [main];
        switch (main)
        {
            case Blast:
                main.Blessing = new GoingAllOut();
                foreach (var i in Enumerable.Range(0, BasicFunctions.GetRandomNumberInBetween(2, 3)))
                {
                    var createdChar = new Slime();
                    newTeam.Add(createdChar);
                }

                break;
            case RoyalKnight:
                main.Blessing = new VitalForce();
                foreach (var i in Enumerable.Range(0, BasicFunctions.GetRandomNumberInBetween(2, 3)))
                {
                    var createdChar = new Police();
                    newTeam.Add(createdChar);
                }

                break;
            case Cheerleader:
                main.Blessing = new HeadStart();
                foreach (var i in Enumerable.Range(0, BasicFunctions.GetRandomNumberInBetween(2, 3)))
                {
                    var createdChar = new Jock();
                    newTeam.Add(createdChar);
                }

                break;
            case Thana:
                main.Blessing = new HeadStart();
                foreach (var i in Enumerable.Range(0, BasicFunctions.GetRandomNumberInBetween(2, 3)))
                {
                    var createdChar = new Skeleton();
                    newTeam.Add(createdChar);
                }

                break;
        }

        foreach (var i in newTeam) i.SetBotStatsAndLevelBasedOnTier(TierRequirement);

        return newTeam;
    }
}