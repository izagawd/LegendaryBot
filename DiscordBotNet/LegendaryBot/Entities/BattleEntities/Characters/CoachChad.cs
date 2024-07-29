using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DSharpPlus.Entities;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
public class GigaPunch : BasicAttack
{
    public override string GetDescription(Character character) => "Punch is thrown gigaly";
    
    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext, out AttackTargetType attackTargetType, out string? text)
    {
        attackTargetType = AttackTargetType.SingleTarget;
        text = "Hrraghh!";
        target.Damage(new DamageArgs(User.Attack * 1.7f,new MoveDamageSource(moveUsageContext))
        {

            ElementToDamageWith = User.Element,
            CriticalDamage = User.CriticalDamage,
            CriticalChance = User.CriticalChance,

            DamageText =
                $"{User.NameWithAlphabet} smiles chadly, and punches {target.NameWithAlphabet} in a cool way and dealt $ damage!"

        });

    }
}

public class MuscleFlex : Ultimate
{
  
    public override string GetDescription(Character character) => "Flexes muscles";
    

    public override IEnumerable<Character> GetPossibleTargets()
    {
        yield return User;
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext, out AttackTargetType attackTargetType, out string? text)
    {
        User.CurrentBattle.AddBattleText($"{User.NameWithAlphabet}... flexed his muscles?");
        text = "Hmph!";
        attackTargetType = AttackTargetType.None;

    }

    public override int MaxCooldown => 1;
}

public class ThumbsUp : Skill
{

    public override string GetDescription(Character character) => "Gives the enemy a thumbs up!";
    

    public override IEnumerable<Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team&& !i.IsDead);
    }

    protected override void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext, out AttackTargetType attackTargetType, out string? text)
    {
        User.CurrentBattle.AddBattleText($"{User.NameWithAlphabet} is cheering {target.NameWithAlphabet} on!");


        attackTargetType = AttackTargetType.None;
        text = $"{User.NameWithAlphabet} gave {target.NameWithAlphabet} a thumbs up!";


    }

    public override int MaxCooldown => 1;
}
public class CoachChad : Character
{
    public override Rarity Rarity => Rarity.FourStar;

    public CoachChad()
    {
        TypeId = 11;
        BasicAttack = new GigaPunch(){User = this};
        Skill = new ThumbsUp(){User = this};
        Ultimate  = new MuscleFlex(){User = this};
      
    }

    public override bool CanSpawnNormally => false;

    public override DiscordColor Color => DiscordColor.Purple;



    [BattleEventListenerMethod]
    private void HandleRevive(CharacterDeathEventArgs deathEventArgs)
    {
        
        if (deathEventArgs.Killed != this) return;
        Revive();
        
    }
    [BattleEventListenerMethod]
    private void HandleTurnEnd(TurnEndEventArgs turnEnd)
    {
        if (turnEnd.Character != this) return;
        RecoverHealth(100);

    }


}