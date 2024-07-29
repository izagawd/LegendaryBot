using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.StatusEffects;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

public partial class Character
{
    
    class StatusEffectInflictBattleText : BattleText
    {
        private StatusEffectInflictResult _effectInflictResult;
        private List<StatusEffect> _statusEffects = [];
        private List<Character> _affectedCharacters;

        public override string Text
        {
            get
            {
                var noun = "has";

                if (_statusEffects.Count > 1)
                    noun = "have";
                var concatenated = BasicFunctionality.CommaConcatenator(_affectedCharacters
                    .Select(i => i.NameWithAlphabet));

                Dictionary<StatusEffect, int> countTracker = [];

                foreach (var i in _statusEffects)
                {
                    var theStatus = countTracker.Keys.FirstOrDefault(j => j.GetType() == i.GetType());
                    if (theStatus is null)
                    {
                        countTracker[i] = 1;
                    }
                    else
                    {
                        countTracker[theStatus]++;
                    }
                }

                var statusEffectsString = BasicFunctionality.CommaConcatenator(countTracker.Select(i =>
                {
                    var toUse = i.Key.Name;
                    if (i.Value > 1) toUse += $" x{i.Value}";
                    return toUse;
                }));
                switch (_effectInflictResult)
                {
                    case StatusEffectInflictResult.Succeeded:
                        return $"{statusEffectsString} " +
                               $"{noun} been inflicted on {concatenated}!";
                    case StatusEffectInflictResult.Resisted:
                        return $"{concatenated} resisted {statusEffectsString}!";
                    case StatusEffectInflictResult.Optimized:
                        return $"{statusEffectsString} {noun} been optimized on {concatenated}!";
                    default:
                        return $"{statusEffectsString} " +
                               $"failed to be inflicted on {concatenated}!";
                }

            }
        }

        public StatusEffectInflictBattleText(Character character, StatusEffectInflictResult effectInflictResult, params StatusEffect[] statusEffects)
        {
            _affectedCharacters = [character];
            _statusEffects = [..statusEffects];
            _effectInflictResult = effectInflictResult;
        }
        protected StatusEffectInflictBattleText(){}
        public override BattleText? Merge(BattleText battleTextInstance)
        {
            if (battleTextInstance is not StatusEffectInflictBattleText statusEffectBattleText) return null;
            if (_effectInflictResult != statusEffectBattleText._effectInflictResult) return null;
            if (_statusEffects.Count != statusEffectBattleText._statusEffects.Count) return null;
            foreach (var i in Enumerable.Range(0,_statusEffects.Count))
            {
                if (_statusEffects[i].GetType() !=  statusEffectBattleText._statusEffects[i].GetType()) return null;
            }
            if (_affectedCharacters.Intersect(statusEffectBattleText._affectedCharacters).Any()) return null;
            return new StatusEffectInflictBattleText()
            {
                _effectInflictResult =  _effectInflictResult,
                _statusEffects =  _statusEffects,
                _affectedCharacters = [.._affectedCharacters, ..statusEffectBattleText._affectedCharacters]
            };

        }
    }
    class CombatReadinessChangeBattleText : BattleText
    {


        private float _combatReadinessChangeAmount;
        private List<Character> _affectedCharacters;
        public CombatReadinessChangeBattleText(Character character,float increaseAmount)
        {
            _affectedCharacters = [character];
            _combatReadinessChangeAmount = increaseAmount;
        }

        public override string Text
        {
            get
            {
                var noun = "has";

                if (_affectedCharacters.Count > 1)
                    noun = "have";
                var thingDone = "decreased";

                if (_combatReadinessChangeAmount >= 0)
                {
                    thingDone = "increased";
                }

                return BasicFunctionality.CommaConcatenator(_affectedCharacters
                           .Select(i => i.NameWithAlphabet))
                       + $" {noun} their combat readiness {thingDone} by {Math.Abs(_combatReadinessChangeAmount)}%!";
            }
        }

        protected CombatReadinessChangeBattleText(){}
        public override BattleText? Merge(BattleText battleTextInstance)
        {
            if (battleTextInstance is not CombatReadinessChangeBattleText combatReadinessChangeBattleText)
                return null;
            if (combatReadinessChangeBattleText._combatReadinessChangeAmount != _combatReadinessChangeAmount)
                return null;
            if (_affectedCharacters.Intersect(combatReadinessChangeBattleText._affectedCharacters).Any())
                return null;
            return new CombatReadinessChangeBattleText()
            {
                _combatReadinessChangeAmount = _combatReadinessChangeAmount,
                _affectedCharacters = [.._affectedCharacters, ..combatReadinessChangeBattleText._affectedCharacters]
            };
        }
    }
    class  ExtraTurnBattleText : BattleText
    {
        private List<Character> _extraTurners;
        public override string Text {             get
        {
            var noun = "has";

            if (_extraTurners.Count > 1)
                noun = "have";
            return BasicFunctionality.CommaConcatenator(_extraTurners
                    .Select(i => i.NameWithAlphabet)) + $" {noun} been granted an extra turn!";
        }
        
        }

        public ExtraTurnBattleText(Character extraTurnCharacter)
        {
            _extraTurners = [extraTurnCharacter];
        }

        public ExtraTurnBattleText()
        {
            
        }
        public override BattleText? Merge(BattleText battleTextInstance)
        {
            if (battleTextInstance is not ExtraTurnBattleText extraTurnBattleText) return null;
            if (_extraTurners.Intersect(extraTurnBattleText._extraTurners).Any()) return null;
            return new ExtraTurnBattleText()
            {
                _extraTurners = [.._extraTurners, ..extraTurnBattleText._extraTurners]
            };
        }
    }
    class  ReviveBattleText : BattleText
    {
        private List<Character> _revivedCharacters;

        public override string Text
        {
            get
            {
                var noun = "has";

                if (_revivedCharacters.Count > 1)
                    noun = "have";
                return BasicFunctionality.CommaConcatenator(_revivedCharacters
                        .Select(i => i.NameWithAlphabet)) + $" {noun} been revived!";
            }
        }

        public ReviveBattleText(Character revivedCharacter)
        {
            _revivedCharacters = [revivedCharacter];
            
        }
        protected ReviveBattleText(){}
        public override BattleText? Merge(BattleText battleTextInstance)
        {
            if (battleTextInstance is not ReviveBattleText reviveBattleText) return null;
            if (_revivedCharacters.Intersect(reviveBattleText._revivedCharacters).Any()) return null;
            return new ReviveBattleText()
            {
                _revivedCharacters = [.._revivedCharacters, ..reviveBattleText._revivedCharacters]
            };
        }
    }
    class  DeathBattleText : BattleText
    {
        public override string Text
        {
            get
            {
                var noun = "has";

                if (_deadCharacters.Count > 1)
                    noun = "have";
                return BasicFunctionality.CommaConcatenator(_deadCharacters
                    .Select(i => i.NameWithAlphabet)) + $" {noun} died!";
            }
        }


        private List<Character> _deadCharacters;
        public DeathBattleText(Character deadCharacter)
        {
            _deadCharacters = [deadCharacter];
        }
        protected DeathBattleText(){}
        public override BattleText? Merge(BattleText battleTextInstance)
        {
            if (battleTextInstance is not DeathBattleText deathHolder) return null;
            if (_deadCharacters.Intersect(deathHolder._deadCharacters).Any()) return null;
            return new DeathBattleText()
            {
                _deadCharacters = [.._deadCharacters, ..deathHolder._deadCharacters]
            };

        }
    }
}