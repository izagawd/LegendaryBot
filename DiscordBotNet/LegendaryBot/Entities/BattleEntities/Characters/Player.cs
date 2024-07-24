using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
public class FourthWallBreaker: BasicAttack
{
    public override string GetDescription(CharacterPartials.Character character) =>  "Damages the enemy by breaking the fourth wall";
    

    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {
        return new UsageResult(this)
        {
            DamageResults = [
                target.Damage(new DamageArgs(this, usageType)
                {
                    ElementToDamageWith = User.Element,
                    CriticalChance = User.CriticalChance,
                    CriticalDamage = User.CriticalDamage,
                    DamageDealer = User,
                    DamageText =
                        $"Breaks the fourth wall, causing {target.NameWithAlphabet} to cringe, and making them receive $ damage!",
                    Damage = User.Attack * 1.7f

                })
            ],
            User = User,
            TargetType = TargetType.SingleTarget,
            Text = "It's the power of being a real human",
            UsageType = usageType

        };
    }
}

public class FireBall : Skill
{
    public override string GetDescription(CharacterPartials.Character character) => "Throws a fire ball at the enemy with a 40% chance to inflict burn";
    

    public override IEnumerable<CharacterPartials.Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team && !i.IsDead);
    }

    public override int MaxCooldown=> 2;


    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {
        
        var damageResult = target.Damage(      new DamageArgs(this, usageType)
        {
            ElementToDamageWith = User.Element,
            CriticalChance = User.CriticalChance,
            CriticalDamage = User.CriticalDamage,
            Damage = User.Attack * 2.4f,
            DamageDealer = User,
            DamageText =$"{User.NameWithAlphabet} threw a fireball at {target.NameWithAlphabet} and dealt $ damage!",
        });
        if (BasicFunctionality.RandomChance(10))
        {
            target.AddStatusEffect(new Burn(){Caster = User,Duration = 1},User.Effectiveness);
        }
        

        return new UsageResult(this)
        {
            UsageType = usageType,
            TargetType = TargetType.SingleTarget,
            User = User,
            DamageResults =[damageResult]
        };
    }
}
public class Ignite : Ultimate
{
    public override int MaxCooldown => 4;
    public override string GetDescription(CharacterPartials.Character character) =>$"Ignites the enemy with 3 burns for 2 turns!";
    

 
    public override IEnumerable<CharacterPartials.Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team&& !i.IsDead);
    }

    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {
        User.CurrentBattle.AddAdditionalBattleText($"{User.NameWithAlphabet} " +
               
                                                    $"attempts to make a human torch out of {target.NameWithAlphabet}!");

        List<StatusEffect> burns = [];
        
        for (var i = 0; i < 3; i++)
        {
            
            burns.Add(new Burn(){ Caster = User, Duration = 2});
            
        }

        target.AddStatusEffects(burns,User.Effectiveness);
        return new UsageResult(this)
        {
            UsageType = usageType,
            TargetType = TargetType.SingleTarget,
            Text = "Ignite!",
            User = User
        };
    }
    
}
public class PlayerDatabaseConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.Property(i => i.Element)
            .HasColumnName(nameof(Player.Element));

    }
}

public struct PlayerCachedData
{
     string CharacterUrl;
    string Name;
}



public class Player : CharacterPartials.Character
{
    public override bool CanSpawnNormally => false;

    protected override IEnumerable<StatType> LevelMilestoneStatIncrease =>
        [StatType.Attack, StatType.Attack, StatType.Speed, StatType.CriticalChance, StatType.CriticalDamage];

    public override bool CanBeTraded => false;
    protected override float BaseAttackMultiplier => 1.05f;

    public override Rarity Rarity => Rarity.FiveStar;


    protected override float BaseSpeedMultiplier => 1.1f;


    private int count = 1;



    public Player()
    {
        TypeId = 1;
        Element = Element.Fire;
        BasicAttack = new FourthWallBreaker(){User = this};
        Skill = new FireBall(){User = this};
        Ultimate = new Ignite(){User = this};
      
    }



    public void SetElement(Element element)
    {
        Element = element;
    }

    
    private string _imageUrl;

    
    
    public override string ImageUrl => $"{Website.DomainName}/battle_images/characters/Player{UserData?.Gender}.png";


    public override string Name => UserData is not null? UserData.Name : base.Name;



}