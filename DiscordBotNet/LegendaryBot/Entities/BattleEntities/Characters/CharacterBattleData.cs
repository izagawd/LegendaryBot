using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Moves;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;


public class CharacterMoves
{
    public Character Character { get; }
    public CharacterMoves(Character character)
    {
        Character = character;
    }
    private BasicAttack? _basicAttack;
    private Skill? _skill;
    private Ultimate? _ultimate;
    public BasicAttack BasicAttack => _basicAttack ??= Character.GenerateBasicAttack();
    public Ultimate? Ultimate => _ultimate ??= Character.GenerateUltimate();
    public Skill? Skill => _skill ??= Character.GenerateSkill();
}
public class CharacterBattleData
{
    public int SuperPoints;
    public Character Character { get; set; }

    public CharacterBattleData(Character character)
    {
        Character = character;
    }
    private CharacterStats? _stats;
    private CharacterMoves? _characterMoves;
    public CharacterStats Stats => _stats ??= new CharacterStats();
    public CharacterMoves CharacterMoves => _characterMoves ??= new CharacterMoves(Character);

}