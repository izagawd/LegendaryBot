using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Caching.Memory;
using SixLabors.ImageSharp.PixelFormats;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
public class FourthWallBreaker: BasicAttack
{
    public override string GetDescription(CharacterPartials.Character character) =>  "Damages the enemy by breaking the fourth wall";
    

    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {
        return new UsageResult(this)
        {
            DamageResults = [
                target.Damage(new DamageArgs(this)
                {
                    ElementToDamageWith = User.Element,
                    CriticalChance = User.CriticalChance,
                    CriticalDamage = User.CriticalDamage,
                    Caster = User,
                    DamageText =
                        $"Breaks the fourth wall, causing {target.NameWithAlphabetIdentifier} to cringe, and making them receive $ damage!",
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
        DamageResult? damageResult;
        using (User.CurrentBattle.PauseBattleEventScope)
        {
            
            damageResult = target.Damage(      new DamageArgs(this)
            {
                ElementToDamageWith = User.Element,
                CriticalChance = User.CriticalChance,
                CriticalDamage = User.CriticalDamage,
                Damage = User.Attack * 2.4f,
                Caster = User,
                DamageText =$"{User.NameWithAlphabetIdentifier} threw a fireball at {target.NameWithAlphabetIdentifier} and dealt $ damage!",
            });
            if (BasicFunctionality.RandomChance(10))
            {
                target.AddStatusEffect(new Burn(User),User.Effectiveness);
            }
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
    public override string GetDescription(CharacterPartials.Character character) =>$"Ignites the enemy with 3 burns. {IgniteChance}% chance each";
    

    public int IgniteChance  => 100;
    public override IEnumerable<CharacterPartials.Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team&& !i.IsDead);
    }

    protected override UsageResult UtilizeImplementation(CharacterPartials.Character target, UsageType usageType)
    {
        User.CurrentBattle.AddAdditionalBattleText($"{User.NameWithAlphabetIdentifier} " +
               
                                                    $"attempts to make a human torch out of {target.NameWithAlphabetIdentifier}!");

        List<StatusEffect> burns = [];
        for (int i = 0; i < 3; i++)
        {
            if (BasicFunctionality.RandomChance(IgniteChance))
            {
                burns.Add(new Burn(User));
            }
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

    protected override IEnumerable<StatType> AscensionStatIncrease =>
        [StatType.Attack, StatType.Attack, StatType.Speed, StatType.CriticalChance, StatType.CriticalDamage];
    public override bool IsInStandardBanner => false;
    public override Rarity Rarity { get; protected set; } = Rarity.FiveStar;


    protected override float BaseSpeedMultiplier => 1.1f;

    [NotMapped]
    public DiscordUser DiscordUser { get; set; }



    public Player()
    {
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
    public override string ImageUrl => _imageUrl;
    public override Task<Image<Rgba32>> GetDetailsImageAsync(bool loadBuild)
    {
        
        return GetDetailsImageAsync(discordUser: null,loadBuild);
    }
    public async Task<Image<Rgba32>> GetDetailsImageAsync(DiscordUser? discordUser,bool loadBuild)
    {
        await LoadPlayerDataAsync(discordUser);
        return await base.GetDetailsImageAsync(loadBuild);
    }
    public async Task<Image<Rgba32>> GetDetailsImageAsync(ClaimsPrincipal claimsUser,bool loadBuild)
    {
        LoadPlayerData(claimsUser);
        return await base.GetDetailsImageAsync(loadBuild);
    }
    public async Task LoadPlayerDataAsync(DiscordUser? discordUser = null)
    {
        if (discordUser is not null)
        {
            DiscordUser = discordUser;
        } else if (DiscordUser is null)
        {
            DiscordUser = await Bot.Client.GetUserAsync((ulong)UserDataId);
        }
        _name = DiscordUser.Username;
        _imageUrl = DiscordUser.AvatarUrl;
        if (UserData is not null)
        {
            Color = UserData.Color;
        } 
    }
    public void LoadPlayerData(ClaimsPrincipal claimsUser)
    {
        _name = claimsUser.GetDiscordUserName();
        _imageUrl = claimsUser.GetDiscordUserAvatarUrl();
        if (UserData is not null)
        {
            Color = UserData.Color;
        }
    }


    private string _name = "player";
    public override string Name => _name;



}