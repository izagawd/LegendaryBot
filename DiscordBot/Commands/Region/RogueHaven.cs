using BasicFunctionality;
using Entities.LegendaryBot.Entities.BattleEntities.Blessings;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

namespace DiscordBot.Commands.Region;

public class RogueHaven : Region
{
    public override string Name => "Rogue Haven";

    public override IEnumerable<Type> ObtainableCharacters =>
    [
        typeof(Roxy),
        typeof(Takeshi), typeof(CommanderJean)
    ];

    public override CharacterTeam GenerateCharacterTeamFor(Type type, out Character main)
    {
        main = (Character)Activator.CreateInstance(type)!;
        CharacterTeam newTeam = [main];
        switch (main)
        {
            case Roxy:
                main.Blessing = new VitalForce();
                foreach (var i in Enumerable.Range(0, BasicFunctions.GetRandomNumberInBetween(2, 3)))
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
                foreach (var i in Enumerable.Range(0, BasicFunctions.GetRandomNumberInBetween(2, 3)))
                {
                    var createdChar = new Police();
                    newTeam.Add(createdChar);
                }

                break;
        }

        foreach (var i in newTeam) i.SetBotStatsAndLevelBasedOnTier(TierRequirement);

        return newTeam;
    }
}