using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

namespace DiscordBotNet.LegendaryBot.Commands;

public class BurgsKingdom : Region
{
    public override CharacterTeam GenerateCharacterTeamFor(Type type, out Character main)
    {
        main = (Character) Activator.CreateInstance(type)!;
        CharacterTeam newTeam = [main];
        switch (main)
        {
            case Blast:
                main.Blessing = new GoingAllOut();
                foreach (var i in Enumerable.Range(0,BasicFunctionality.GetRandomNumberInBetween(2,3)))
                {
                    var createdChar = new Slime();
                    newTeam.Add(createdChar);
                }
                break;
            case RoyalKnight:
                main.Blessing = new VitalForce();
                foreach (var i in Enumerable.Range(0,BasicFunctionality.GetRandomNumberInBetween(2,3)))
                {
                    var createdChar = new Police();
                    newTeam.Add(createdChar);
                }
                break;
            case Cheerleader:
                main.Blessing = new HeadStart();
                foreach (var i in Enumerable.Range(0,BasicFunctionality.GetRandomNumberInBetween(2,3)))
                {
                    var createdChar = new Jock();
                    newTeam.Add(createdChar);
                }
                break;
            case Thana:
                main.Blessing = new HeadStart();
                foreach (var i in Enumerable.Range(0,BasicFunctionality.GetRandomNumberInBetween(2,3)))
                {
                    var createdChar = new Skeleton();
                    newTeam.Add(createdChar);
                }
                break;
            
        }

        foreach (var i in newTeam)
        {
            i.SetBotStatsAndLevelBasedOnTier(TierRequirement);
        }

        return newTeam;
    }
    
    public override string Name => "Burgs Kingdom";

    public override IEnumerable<Type> ObtainableCharacters =>
    [
        typeof(RoyalKnight),
        typeof(Blast),  typeof(Thana), typeof(Cheerleader)
    ];
}