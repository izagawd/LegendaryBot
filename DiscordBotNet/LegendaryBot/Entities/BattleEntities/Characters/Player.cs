using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;
using DSharpPlus.Entities;
using SixLabors.ImageSharp.PixelFormats;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
public class FourthWallBreaker: BasicAttack
{
    public override string GetDescription(Character character) =>  "Damages the enemy by breaking the fourth wall";
    

    protected override UsageResult HiddenUtilize(Character target, UsageType usageType)
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
    public override string GetDescription(Character character) => "Throws a fire ball at the enemy with a 40% chance to inflict burn";
    

    public override IEnumerable<Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team && !i.IsDead);
    }

    public override int MaxCooldown=> 2;
  
    protected override UsageResult HiddenUtilize(Character target, UsageType usageType)
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
    public override string GetDescription(Character character) =>$"Ignites the enemy with 3 burns. {IgniteChance}% chance each";
    

    public int IgniteChance  => 100;
    public override IEnumerable<Character> GetPossibleTargets()
    {
        return User.CurrentBattle.Characters.Where(i => i.Team != User.Team&& !i.IsDead);
    }

    protected override UsageResult HiddenUtilize(Character target, UsageType usageType)
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
public class Player : Character
{



    public override bool IsInStandardBanner => false;
    public override Rarity Rarity { get; protected set; } = Rarity.FiveStar;

    
    [NotMapped]
    public DiscordUser DiscordUser { get; set; }
    [NotMapped]
    private Ultimate FireUltimate { get; } = new Ignite();
    [NotMapped]
    private Skill fireSkill { get; } = new FireBall();
    public override Skill? Skill
    {
        get
        {
            switch (Element)
            {
                case Element.Fire:
                    return fireSkill;
                default:
                    return fireSkill;
            }
        }

    }

    public override BasicAttack BasicAttack { get; } = new FourthWallBreaker();


    public override Ultimate? Ultimate
    {
        get
        {
            switch (Element)
            {
                case Element.Fire:
                    return FireUltimate;
                default:
                    return FireUltimate;
            }
        }

    }


    public void SetElement(Element element)
    {
        Element = element;
        if (CurrentBattle is not null) 
            CurrentBattle.SetupCharacterForThisBattle(this);
    }

 
    public override string ImageUrl { get; protected set; }
    public override Task<Image<Rgba32>> GetDetailsImageAsync(bool loadBuild)
    {
        
        return GetDetailsImageAsync(discordUser: null,loadBuild);
    }
    public async Task<Image<Rgba32>> GetDetailsImageAsync(DiscordUser? discordUser,bool loadBuild)
    {
        await LoadAsync(discordUser, loadBuild);
        return await base.GetDetailsImageAsync(loadBuild);
    }
    public async Task<Image<Rgba32>> GetDetailsImageAsync(ClaimsPrincipal claimsUser,bool loadBuild)
    {
        await LoadAsync(claimsUser, loadBuild);
        return await base.GetDetailsImageAsync(loadBuild);
    }
    public async Task LoadAsync(ClaimsPrincipal claimsUser, bool build = true)
    {
        await base.LoadAsync(build);
        Name = claimsUser.GetDiscordUserName();
        ImageUrl = claimsUser.GetDiscordUserAvatarUrl();
        if (UserData is not null)
        {
            Color = UserData.Color;
        }
    }
    public async Task LoadAsync(DiscordUser? discordUser, bool build = true)
    {
        await base.LoadAsync(build);
        if (discordUser is not null)
        {
            DiscordUser = discordUser;
        } else if (DiscordUser is null)
        {
            DiscordUser = await Bot.Client.GetUserAsync((ulong)UserDataId);
        }

        Name = DiscordUser.Username;
        ImageUrl = DiscordUser.AvatarUrl;
        if (UserData is not null)
        {
            Color = UserData.Color;
        } 
    }
    public override Task LoadAsync(bool loadGear)
    {
        return LoadAsync(discordUser: null,loadGear);
    }
    public override string Name { get; protected set; }



}