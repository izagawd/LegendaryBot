using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Results;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class VolcanicEruptionCharging : StatusEffect
{
    public override string Description =>
        "Makes the affected charge up a powerful attack that's released at the end of the effect's duration";


    public VolcanicEruptionCharging( Character caster) : base(caster)
    {
     
    }
    public override StatusEffectType EffectType => StatusEffectType.Buff;
    public override OverrideTurnType OverrideTurnType => OverrideTurnType.ControlDecision;
    public override int MaxStacks => 1;

    public override UsageResult OverridenUsage(Character affected,ref Character target, ref BattleDecision decision, UsageType usageType)
    {
        decision = BattleDecision.Other;
        if(Duration == 1)
        {
            List<DamageResult> damageResults = [];
            foreach (var i in affected.CurrentBattle.Characters.Where(j => j.Team != affected.Team))
            {

                var damageResult =  i.Damage(                new DamageArgs(this)
                {
                    ElementToDamageWith = Caster.Element,
                    CriticalChance = Caster.CriticalChance,
                    CriticalDamage = Caster.CriticalDamage,
                    Damage = affected.Attack * 1.7f * 2,
                    Caster = affected,
                    CanCrit = true,
                    DamageText =$"{affected} shot out a very powerful blast that dealt $ damage to {i}!"
                });
                if(damageResult is not null)
                    damageResults.Add(damageResult);

            }
            return new UsageResult(this)
            { 
                Text = "Blast!", 
                DamageResults = damageResults,
                UsageType = UsageType.NormalUsage,
                TargetType = TargetType.AOE,
                User = affected
            };
        }

        affected.CurrentBattle.AddAdditionalBattleText(
            $"{affected} is charging up a very powerful attack. {Duration - 1} more turns till it is released");
        return new UsageResult(this)
        {
            TargetType = TargetType.None,
            UsageType = usageType,
            Text = "Charging!",
            User = affected
        };
    }
    public override void PassTurn()
    {
 
        if (Affected.StatusEffects.Any(i => (int)i.OverrideTurnType > (int)OverrideTurnType))
        {
            Affected.RemoveStatusEffect(this);
            return;
        }

        base.PassTurn();

    }

}