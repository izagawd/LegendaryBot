using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.ModifierInterfaces;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.BattleSimulatorStuff;

public enum BattleDecision
{
    
     Ultimate, BasicAttack, Skill, Other
}


public partial class BattleSimulator
{


    private DiscordButtonComponent _basicAttackButton = new(DiscordButtonStyle.Secondary, nameof(BasicAttack), null,emoji: new DiscordComponentEmoji("⚔️"));
    private  DiscordButtonComponent _skillButton = new(DiscordButtonStyle.Secondary, nameof(Skill), null, emoji: new DiscordComponentEmoji("🪄"));
    private  DiscordButtonComponent _ultimateButton = new(DiscordButtonStyle.Secondary, nameof(Ultimate), null, emoji: new DiscordComponentEmoji("⚡"));
    private  static DiscordButtonComponent _forfeitButton = new(DiscordButtonStyle.Danger, "forfeit", "Forfeit");




    protected static ConcurrentDictionary<string,Image<Rgba32>> CachedResizedForAvatars = new();
    
    


    private  async Task<Image<Rgba32>> GetAvatarAsync(string url)
    {
        if(CachedResizedForAvatars.TryGetValue(url, out var characterImageToDraw))
        {
            return characterImageToDraw.Clone();
        }
        characterImageToDraw = await BasicFunctionality.GetImageFromUrlAsync(url);
        characterImageToDraw.Mutate(mutator =>
        {
            mutator.Resize(30, 30);
        });
        CachedResizedForAvatars[url] = characterImageToDraw;
        return characterImageToDraw.Clone();
    }

    
    public async Task<Image<Rgba32>> GetCombatImageAsync()
    {

     
        var heightToUse = CharacterTeams.Select(i => i.Count).Max() * 160;
        var image = new Image<Rgba32>(500, heightToUse);
        var xOffSet = 70;
        var widest = 0;
        var length = 0;
        var yOffset = 0;
        IImageProcessingContext imageCtx = null!;
        image.Mutate(ctx => imageCtx = ctx);
 
        foreach (var i in CharacterTeams)
        {

            foreach (var j in i)
            {
                using var characterCombatImage = await j.GetImageForCombatAsync();
                if (characterCombatImage.Width > widest)
                {
                    widest = characterCombatImage.Width;
                }
                imageCtx.DrawImage(characterCombatImage, new Point(xOffSet, yOffset), new GraphicsOptions());
                yOffset += characterCombatImage.Height + 15;
                if (yOffset > length)
                {
                    length = yOffset;
                }
            }
            yOffset = 0;
            xOffSet += widest + 75;
        }

        var combatReadinessLineTRectangle = new Rectangle(30, 0, 3, length);
        imageCtx
            .BackgroundColor(Color.Gray)
            .Draw(Color.Black, 8, combatReadinessLineTRectangle)
            .Fill(Color.White, combatReadinessLineTRectangle);   

        foreach (var i in Characters
                     .Where(i => !i.IsDead && ActiveCharacter != i)
                     .OrderBy(i => i.CombatReadiness))
        {
            
            using var characterImageToDraw = await GetAvatarAsync(i.ImageUrl);
            Color circleBgColor;
            if (i.Team == Team2) 
                circleBgColor = Color.DarkRed;
            else 
                circleBgColor = Color.DarkBlue;
            
            characterImageToDraw.Mutate(j =>
            
                j.BackgroundColor(circleBgColor)
                .ConvertToAvatar()
            );
            Color circleColor;
            if (i.Team == Team2)
                circleColor = Color.Red;
            else
                circleColor = Color.Blue;
            
            var characterImagePoint =
                new Point(((combatReadinessLineTRectangle.X + (combatReadinessLineTRectangle.Width / 2.0))
                           - (characterImageToDraw.Width / 2.0)).Round(),
                    (i.CombatReadiness * length / 100).Round());
  
            var circlePolygon = new EllipsePolygon(characterImageToDraw.Width / 2.0f + characterImagePoint.X,
                characterImageToDraw.Height / 2.0f + characterImagePoint.Y,
                characterImageToDraw.Height / 2.0f);
            imageCtx
                .DrawImage(characterImageToDraw, characterImagePoint,new GraphicsOptions())
                .Draw(circleColor, 2, circlePolygon);
        }

        imageCtx.EntropyCrop();
   
        return image;
    }



    public static void Idk(BattleEventArgs idk){}
    static BattleSimulator()
    {
        SetupBattleEventDelegatorStuff();
    }
    class EventMethodDetails
    {
        public BattleEventMethod BattleEventMethod { get; }
        public BattleEventListenerMethodAttribute Attribute { get; }
        public Type ParameterType { get; }
        public EventMethodDetails(BattleEventMethod battleEventMethod, BattleEventListenerMethodAttribute attribute, Type parameterType)
        {
            BattleEventMethod = battleEventMethod;
            Attribute = attribute;
            ParameterType = parameterType;
        }
    }
    private static ConcurrentDictionary<Type, List<EventMethodDetails>> _methodsCache = [];
    
    /// <summary>
    /// Used to get entities connected to this battle simulator 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private IEnumerable<T> GetConnectedEntities<T>()
    {
        if (this is T thisAsT) yield return thisAsT;
        foreach (var i in Characters)
        {
            if (i is T characterAsT) yield return characterAsT;
            foreach (var j in i.MoveList.OfType<T>())
            {
                yield return j;
            }

            foreach (var j in i.StatusEffects.OfType<T>())
            {
                yield return j;
            }
            if (i.Blessing is T blessingAsT) yield  return blessingAsT;
        }
    }
    struct BattleEventListenerMethodContainer
    {
        /// <summary>
        /// The entity to invoke the method with
        /// </summary>
        public IBattleEventListener Entity { get; }

        /// <summary>
        /// The method to invoke
        /// </summary>
        public EventMethodDetails EventMethodDetails { get; }
        public BattleEventListenerMethodContainer(IBattleEventListener entity,EventMethodDetails eventMethodDetails)
        {

            Entity = entity;
            EventMethodDetails = eventMethodDetails;
        }
        
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private IEnumerable<BattleEventListenerMethodContainer> GetAllEventMethods()
    {
        foreach (var i in GetConnectedEntities<IBattleEventListener>())
        {
            foreach (var j in 
                     _methodsCache[i.GetType()])
            {
                yield return new BattleEventListenerMethodContainer(i, j);
            }
        }
    }
    /// <summary>
    /// 
    /// This will be used to invoke an event if it happens
    /// 
    /// </summary>
    /// <param name="eventArgs">The argument instance of the battle event</param>
    /// <typeparam name="T">the type of argument of the battle event</typeparam>
    public void InvokeBattleEvent<T>(T eventArgs) where T : BattleEventArgs
    {
        
        if (IsEventsPaused)
        {
            _queuedBattleEvents.Add(eventArgs);
            return;
        }

        DynamicMethod method;

      
        
        var eventArgsType = eventArgs.GetType();
        foreach (var i in GetAllEventMethods()
                     .Where(k => eventArgsType.IsRelatedToType(k.EventMethodDetails.ParameterType))
                     .OrderByDescending(j => j.EventMethodDetails.Attribute.Priority))
        {
        
            i.EventMethodDetails.BattleEventMethod.Invoke(i.Entity,eventArgs);

        }
        

    }


    public  IEnumerable<StatsModifierArgs> GetAllStatsModifierArgsInBattle()
    {
        foreach (var i in GetConnectedEntities<IStatsModifier>())
        {
            foreach (var j in i.GetAllStatsModifierArgs())
            {
                yield return j;
            }
        }
    }

    public IEnumerable<CharacterTeam> CharacterTeams
    {
        get
        {
            yield return Team1;
            yield return Team2;
        }
    }
    /// <summary>
    /// Call this for a character that you want to add to a battle,
    /// or a change in blessing, or move is made to a character. Does not need to be manually called for
    /// characters that started with this battle
    /// </summary>
    /// <param name="character"></param>
    public void SetupCharacterForThisBattle(Character character)
    {
        if (!CharacterTeams.Any(i => i.Contains(character)))
            throw new Exception("Character must be in a team that is in this battle to be set up");
        if (character.Blessing is not null)
            character.Blessing.Character = character;
        foreach (var i in character.MoveList)
        {
            i.User = character;
        }
        
    }

    private string? _mainText = "battle begins";

    /// <summary>
    /// The character who is currently taking their turn
    /// </summary>
    public Character ActiveCharacter { get; protected set; }



    /// <summary>
    /// All the characters in the battle
    /// </summary>
    public IEnumerable<Character> Characters => Team1.Union(Team2);
    /// <summary>
    /// Creates a new battle between two teams
    /// </summary>
    public CharacterTeam Team1 { get; }
    public CharacterTeam Team2 { get; }
    
    public BattleSimulator(CharacterTeam team1, CharacterTeam team2)
    {
        if (team1.Count == 0 || team2.Count == 0)
        {
            throw new Exception("one of the teams has no fighters");
        }

        Team1 = team1;
        Team2 = team2;



    }

    private CharacterTeam? _winners;



    /// <summary>
    /// Sets the winning team by checking if all the characters in a team are dead
    /// </summary>
    public void CheckForWinnerIfTeamIsDead()
    {
        if(_winners is not null) return;
        
        foreach (var i in CharacterTeams)
        {
            if (!i.All(j => j.IsDead)) continue;
            _winners = CharacterTeams.First(k => k != i);
            break;
        }
    }


    private bool _stopped = false;
    



    /// <summary>
    /// The team that has forfeited
    /// </summary>
    private CharacterTeam? _forfeited;
    IEnumerable<DiscordSelectComponentOption> GetSelectComponentOptions()
    {
        List<DiscordSelectComponentOption> toSelect = [];
        foreach (var i in Characters)
        {
            toSelect.Add(new DiscordSelectComponentOption(i.NameWithAlphabetIdentifier, 
                i.NameWithAlphabetIdentifier));
        }

        return toSelect.ToArray();
    }

    private async Task HandleDisplayBattleInfoAsync( ComponentInteractionCreatedEventArgs interactionCreateEventArgs)
    {
        try
        {
            var interaction = interactionCreateEventArgs.Interaction;
            var gottenValue = interactionCreateEventArgs.Values.First();
            var characterToDisplayBattleInfo = Characters.FirstOrDefault(i => i.NameWithAlphabetIdentifier == gottenValue);
            if (characterToDisplayBattleInfo is null)
            {
                await interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent("Could not find character selected for some reason. Try again!")
                        .AsEphemeral());
                return;
            }

            var descriptionStringBuilder = new StringBuilder();


            descriptionStringBuilder.Append($"Element: {characterToDisplayBattleInfo.Element} • Combat Readiness: {characterToDisplayBattleInfo.CombatReadiness.Round()}%\n\n");
            foreach (var i in characterToDisplayBattleInfo.MoveList)
            {
                var moveTypeName = "Basic Attack :crossed_swords:";

                if (i is Skill)
                    moveTypeName = "Skill :magic_wand:";
                else if (i is Ultimate)
                {
                    moveTypeName = "Ultimate :zap:";
                }

                descriptionStringBuilder.Append(
                    $"{moveTypeName}: {i}.\nDescription: {i.GetDescription(characterToDisplayBattleInfo)}");
                if (i is Special special)
                {
                    descriptionStringBuilder.Append(
                        $"\nMax Cooldown: {special.MaxCooldown}. Current Cooldown: {special.Cooldown}");
                }

                descriptionStringBuilder.Append("\n\n");

            }

            var statusEffectsCopy = characterToDisplayBattleInfo.StatusEffects;
            if (statusEffectsCopy.Any())
            {
                descriptionStringBuilder.Append("Status Effects\n\n");

                foreach (var effect in statusEffectsCopy)
                {
                    descriptionStringBuilder.Append(
                        $"Name: {effect} | Type: {effect.EffectType} | Duration: {effect.Duration}\n");
                }

                descriptionStringBuilder.Append('\n');
            }
            descriptionStringBuilder
                .Append("Stats\n");
            var shouldNewLine = false;
            foreach (var i in Enum.GetValues<StatType>())
            {
                descriptionStringBuilder.Append(
                    $"{BasicFunctionality.Englishify(i.ToString())}: {characterToDisplayBattleInfo.GetStatFromType(i)}");
                
                
                switch (i)
                {
                    case StatType.CriticalChance:
                    case StatType.CriticalDamage:
                    case StatType.Resistance:
                    case StatType.Effectiveness:
                        descriptionStringBuilder.Append("%");
                        break;
               
                }
                if (shouldNewLine)
                {
                    descriptionStringBuilder.Append("\n");
                    shouldNewLine = false;
                }
                else
                {
                    descriptionStringBuilder.Append(" | ");
                    shouldNewLine = true;
                }
                
            }
          

            var embed = new DiscordEmbedBuilder()
                .WithAuthor(characterToDisplayBattleInfo.NameWithAlphabetIdentifier, iconUrl: characterToDisplayBattleInfo.ImageUrl)
                .WithTitle($"{characterToDisplayBattleInfo} [{characterToDisplayBattleInfo.AlphabetIdentifier}]'s description")
                .WithColor(characterToDisplayBattleInfo.Color)
                .WithDescription(descriptionStringBuilder.ToString());



            await interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                    .AddEmbed(embed)
                    .AsEphemeral());

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

    }
    /// <summary>
    /// whether or not to set health to max health for all characters at start of battle
    /// </summary>
    public bool SetToMaxHealthAtStart { get; set; } = true;

    protected static DiscordButtonComponent yes = new DiscordButtonComponent(DiscordButtonStyle.Success, "yes", "YES");
    
    protected static DiscordButtonComponent no = new DiscordButtonComponent(DiscordButtonStyle.Success, "no", "NO");
    private async Task HandleForfeitAsync(DiscordInteraction interaction)
    {
        
        try
        {
            var team = CharacterTeams
                .OfType<PlayerTeam>().First(i => i.UserDataId == interaction.User.Id);
            var embed = new DiscordEmbedBuilder()
                .WithTitle("For real?")
                .WithColor(DiscordColor.Blue)
                .WithDescription("Are you sure you want to forfeit?");

            await interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                    .AddEmbed(embed)
                    .AsEphemeral()
                    .AddComponents(yes, no));

            var message = await interaction.GetOriginalResponseAsync();

            var result = await message.WaitForButtonAsync(i => i.User.Id == interaction.User.Id, 
                new TimeSpan(0,0,30));
            if (result.TimedOut) return;
            if (result.Result.Id == "yes")
            {
                if(_forfeited is null)
                    _forfeited = team;
                if (_interruptionCancellationTokenSource is not null && !_interruptionCancellationTokenSource.IsCancellationRequested)
                {

                    await _interruptionCancellationTokenSource.CancelIfNotDisposedAsync();

                }
      

            }

            await result.Result.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage);
      
        }
        catch(Exception e)
        {
            Console.WriteLine(e.ToString());
        }

    }
    public class PauseBattleEventsInstance : IDisposable
    {
        private BattleSimulator _battleSimulator;
        public PauseBattleEventsInstance(BattleSimulator battleSimulator)
        {
            _battleSimulator = battleSimulator;
            _battleSimulator._pauseCount++;
        }

        private bool _disposed = false;

        public void Dispose()
        {
            if(_disposed) return;

            _battleSimulator._pauseCount--;
            _disposed = true;

            if(_battleSimulator.IsEventsPaused) return;
            _battleSimulator._pauseCount = 0;
            foreach (var i in _battleSimulator._queuedBattleEvents.ToArray())
            {
                if (_battleSimulator.IsEventsPaused) break;
                if(!_battleSimulator._queuedBattleEvents.Contains(i)) continue;
                _battleSimulator._queuedBattleEvents.Remove(i);
                _battleSimulator.InvokeBattleEvent(i);
            }
       
        }
    }


    private List<BattleEventArgs> _queuedBattleEvents = [];
    private int _pauseCount = 0;
    public bool IsEventsPaused => _pauseCount > 0;

    /// <summary>
    /// Battle events will be paused until this scope is disposed
    /// </summary>
    protected PauseBattleEventsInstance PauseBattleEventScope => new(this);

    

    private void CheckForForfeitOrInfoTillEndOfBattle(DiscordMessage message,CancellationToken token)
    {
       
        _ = message.WaitForSelectAsync(args =>
        {
            if (args.Id == "info")
            {
                _ = HandleDisplayBattleInfoAsync(args);
            }
            return false;
        }, token);
        _ = message.WaitForButtonAsync(args =>
        {
            if (args.Id == "forfeit" &&  CharacterTeams.Any(i => i.TryGetUserDataId == args.User.Id))
            {
                _ = HandleForfeitAsync(args.Interaction);
                return false;
            }
            return false;
        }, token);
    }
    public int Turn { get; set; } = 0;

    public TimeSpan TimeOutTimeSpan { get;  set; } = TimeSpan.FromMinutes(1.5);
    /// <summary>
    /// Initiates a new battle between two teams, by editing the provided message
    /// </summary>
    public  Task<BattleResult> StartAsync(DiscordMessage message)
    {
        return StartAsync(message : message, interaction: null);
    }

    /// <summary>
    /// Initiates a new battle between two teams, by editing the provided message with responding to the interaction
    /// </summary>
    /// <param name="isInteractionEdit">is true, edits the interaction message. if not, sends a follow up message</param>
    /// <param name="interaction"></param>
    /// <returns></returns>
    public  Task<BattleResult> StartAsync( DiscordInteraction interaction)
    {
        return StartAsync(message: null,
            interaction: interaction);
    }




    private List<AdditionalBattleText> _battleTextInstances = ["Have fun!"];

    public void AddAdditionalBattleText(AdditionalBattleText additionalBattleTextInstance)
    {
        if (additionalBattleTextInstance is null) throw new ArgumentNullException("Inputted argument is null");
       _battleTextInstances.Add(additionalBattleTextInstance);
    }


    /// <summary>
    /// Initiates a new battle between two teams, by sending a message to a channel
    /// </summary>
    public  Task<BattleResult> StartAsync(DiscordChannel channel)
    {
        return StartAsync(message: null,interaction: null, channel: channel);
    }
    public Alphabet GetAlphabetIdentifier(Character character)
    {
         return (Alphabet)Characters.ToList().IndexOf(character);
    }

    /// <summary>
    /// Canceled if you want to interrupt waiting for an action from a player/bot in battle.
    /// mainly used for forfeit, so it's cancelled when someone forfeit to immediately process forfeiting
    /// </summary>
    private CancellationTokenSource _interruptionCancellationTokenSource;

    /// <summary>
    /// How long it will take for AI to perform their attack when its their turn, and for some other stuff,
    /// <br/> like when a character cant move, it will be the delay until the next turn is processed, probably
    /// </summary>
    public int WaitDelay { get; set; } = 5000;
    
    
  
    ~BattleSimulator()
    {
        // done just in case the game cancellation token source was still here
        // so it won't be chilling in the heap
        if (_gameCancellationTokenSource is not null)
        {
            
            _gameCancellationTokenSource.CancelIfNotDisposed();
           
  
            _gameCancellationTokenSource.Dispose();
        }
    }

  
    private event Action OtherFollowUpActions;

    private event Action CounterAttackFollowUpActions;


    private bool _canAddFollowUpAction = false;
    /// <summary>
    /// 
    /// If necessary, dont forget to check if the caster or target is dead, or if caster is stunned or something
    /// before doing what needs to be done in the inputted action <br>
    /// it is preferred for counter attacks to be done here. counter attacks will execute before anything else
    /// </summary>
    /// <param name="action"></param>
    /// <returns>true if succeeded, false if failed, meaning it wasnt the right time to add a follow up action</returns>
    public bool RegisterFollowUpAction(Action action, bool isCounterAttack = false)
    {
        if (!_canAddFollowUpAction) return false;
        if (isCounterAttack)
        {
            CounterAttackFollowUpActions += action;
        }
        else
        {
            OtherFollowUpActions += action;
        }

        return true;
    }


    /// <summary>
    /// Cancelling this token means interrupting a game in some way. should be used if eg: one of the teams forfeits
    /// </summary>
    private CancellationTokenSource _gameCancellationTokenSource = null!;
    
    protected async Task<BattleResult> StartAsync(
        DiscordMessage? message = null, DiscordInteraction? interaction = null,
        DiscordChannel? channel = null)
    {
        // its possible that there was a cancellation token not disposed of at the start of the battle. rare, but i drop
        //this code just in case
        if (_gameCancellationTokenSource is not null)
        {

            await _gameCancellationTokenSource.CancelIfNotDisposedAsync();
     
                
            _gameCancellationTokenSource.Dispose();
        }
        _gameCancellationTokenSource = new CancellationTokenSource();
        var firstLoop = true;
        _pauseCount = 0;
        _stopped = false;
        _canAddFollowUpAction = false;
        Team1.CurrentBattle = this;
        Team2.CurrentBattle = this;
        foreach (var i in CharacterTeams)
        {
            foreach (var j in i)
            {
                j.Team = i;
                if (SetToMaxHealthAtStart)
                    j.Health = j.MaxHealth;
                SetupCharacterForThisBattle(j);
            }
        }
        
        CharacterTeam? timedOut = null;

        
        Character? target = null; 
        
        // If you want the bot to update a message using an interactivity result instead of without, use this
        InvokeBattleEvent(new BattleBeginEventArgs());
        ImmutableArray<DiscordButtonComponent> components =  [_basicAttackButton, _skillButton, _ultimateButton, _forfeitButton];

        while (true)
        {
 
           
            Turn += 1;
            var extraTurnGranted = false;
            var extraTurners =
                Characters.Where(i => i.ShouldTakeExtraTurn)
                    .ToArray();
            
            if (extraTurners.Any())
            {
                ActiveCharacter = BasicFunctionality.RandomChoice(extraTurners.AsEnumerable());
                ActiveCharacter.ShouldTakeExtraTurn = false;
                extraTurnGranted = true;
            }
            while (!Characters.Any(i => i.CombatReadiness >= 100 && !i.IsDead) && !extraTurnGranted)
            {
                foreach (var j in Characters)
                {
                    if(!j.IsDead) j.CombatReadiness +=  0.0025f * j.Speed;
                }
            }
            
            if (!extraTurnGranted)
            {
                ActiveCharacter = BasicFunctionality.RandomChoice(Characters.Where(i => i.CombatReadiness >= 100 && !i.IsDead));

            }

            if (_mainText is null)
                _mainText = $"{ActiveCharacter}'s turn";

            
            ActiveCharacter.CombatReadiness = 0;
            
            foreach (var i in ActiveCharacter.StatusEffects)
            {

                //this code executes for status effects that occur just before the beginning of the turn
                if (i.ExecuteStatusEffectBeforeTurn && ActiveCharacter.StatusEffects.Contains(i))
                {
                    i.PassTurn();
                    if (i.Duration <= 0) ActiveCharacter.RemoveStatusEffect(i);
                }
            }
        

            InvokeBattleEvent(new TurnStartEventArgs(ActiveCharacter));
            var shouldDoTurn = !ActiveCharacter.IsDead;
            if (!shouldDoTurn)
                AddAdditionalBattleText($"{ActiveCharacter.NameWithAlphabetIdentifier} cannot take their turn because they died in the process of taking their turn!");
            
            
            
            var additionalText =AdditionalBattleText
                .Combine(_battleTextInstances)
                .Select(i => i.Text).Join("\n");
            _battleTextInstances.Clear();

            if (additionalText.Length == 0) additionalText = "No definition";
            else if (additionalText.Length > 1024)
                additionalText = additionalText.Substring(0, 1021) + "...";

            var embedToEdit = new DiscordEmbedBuilder()
                .WithTitle("**BATTLE!!!**")
                .WithAuthor($"{ActiveCharacter.Name} [{ActiveCharacter.AlphabetIdentifier}]", iconUrl: ActiveCharacter.ImageUrl)
                .WithColor(ActiveCharacter.Color)
                .AddField(_mainText, additionalText)
                .WithImageUrl("attachment://battle.png");

  
            using var combatImage = await GetCombatImageAsync();
    
            await using var stream = new MemoryStream();
            await combatImage.SaveAsPngAsync(stream);
       
            stream.Position = 0;

            var messageBuilder =new DiscordMessageBuilder()
                .AddEmbed(embedToEdit.Build())
                .AddFile("battle.png", stream);

            CheckForWinnerIfTeamIsDead();

            foreach (var i in components
                         .Where(i => i != _forfeitButton))
            {
                i.Disable();
            }
            if (!(!ActiveCharacter.Team.IsPlayerTeam || ActiveCharacter.IsOverriden) 
                && shouldDoTurn
                && _winners is null && !_stopped)
            {
                _basicAttackButton.Enable();
               
                if (ActiveCharacter.Skill is not null &&  ActiveCharacter.Skill.CanBeUsed())
                {
                    _skillButton.Enable();
                }
                if (ActiveCharacter.Ultimate is not null && ActiveCharacter.Ultimate.CanBeUsed())
                {
                    _ultimateButton.Enable();
                }
            }
            
            var infoSelect = new DiscordSelectComponent("info", "View info of character",
                GetSelectComponentOptions());
            messageBuilder
                .AddComponents(components)
                .AddComponents(infoSelect);

           
            if(interaction is not null)
            {
                if (interaction.ResponseState != DiscordInteractionResponseState.Replied)
                {
                    await interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage,
                        new DiscordInteractionResponseBuilder(messageBuilder));
                    message = await interaction.GetOriginalResponseAsync();
                }
                else
                {
                    message = await interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder(messageBuilder));
                }
                interaction = null;
            }
            else if (message is not null)
            {
                message = await message.ModifyAsync(messageBuilder);
            }
            else if(channel is not null)
            {
                message = await channel.SendMessageAsync(messageBuilder);
            }
            else
            {
                throw new Exception("No way to display battle");
            }

            channel = message.Channel;
            if (firstLoop)
            {
                firstLoop = false;
                CheckForForfeitOrInfoTillEndOfBattle(message,_gameCancellationTokenSource.Token);
            }
       



            _mainText = null;
            if (_forfeited is not null)
            {
                _winners = CharacterTeams.First(i => i != _forfeited);
            }
            if (_winners is not null) 
            {
                await Task.Delay(WaitDelay); break;
            }

            var battleDecision =BattleDecision.Other;
 
            StatusEffect? mostPowerfulStatusEffect = null;
           
          
            mostPowerfulStatusEffect = ActiveCharacter.StatusEffects.OrderByDescending(i => i.OverrideTurnType).FirstOrDefault();


            _canAddFollowUpAction = true;
    
            if (!shouldDoTurn)
            {
                using (_interruptionCancellationTokenSource = new CancellationTokenSource())
                {
                    await BasicFunctionality.DelayWithTokenNoError(WaitDelay,
                        _interruptionCancellationTokenSource.Token);
                }

            }
            else if (mostPowerfulStatusEffect is not null && mostPowerfulStatusEffect.OverrideTurnType > 0)
            {
                var overridenUsage = mostPowerfulStatusEffect.OverridenUsage(ref target!,
                    ref battleDecision, UsageType.NormalUsage);
                if (overridenUsage.Text is not null) _mainText = overridenUsage.Text;
                using (_interruptionCancellationTokenSource = new CancellationTokenSource())
                {
                    await BasicFunctionality.DelayWithTokenNoError(WaitDelay,
                        _interruptionCancellationTokenSource.Token);
                }
            }

            else if (!ActiveCharacter.Team.IsPlayerTeam)
            {
                ActiveCharacter.NonPlayerCharacterAi(ref target!, ref battleDecision);


                using (_interruptionCancellationTokenSource = new CancellationTokenSource())
                {
                    await BasicFunctionality.DelayWithTokenNoError(WaitDelay,
                        _interruptionCancellationTokenSource.Token);
                }
            }
            else
            {
                InteractivityResult<ComponentInteractionCreatedEventArgs> results;
                using (_interruptionCancellationTokenSource = new CancellationTokenSource(TimeOutTimeSpan))
                {
                    results = await message.WaitForButtonAsync(e =>
                    {
                        if (!CharacterTeams.Any(i => i.TryGetUserDataId == e.User.Id)) return false;
                        if (!Enum.TryParse(e.Id, out BattleDecision localDecision)) return false;
                        if (e.User.Id == ActiveCharacter.Team.TryGetUserDataId
                            && ((IEnumerable<BattleDecision>)
                            [
                                BattleDecision.BasicAttack, BattleDecision.Skill, BattleDecision.Ultimate,
                            ]).Contains(localDecision) && (ActiveCharacter[localDecision]?.CanBeUsed()).GetValueOrDefault(false))
                        {
                            battleDecision = localDecision;
                            return true;
                        }

                        return false;
                    }, _interruptionCancellationTokenSource.Token);
                }

                if (_forfeited is not null)
                {
                    _winners = CharacterTeams.First(i => i != _forfeited);
                    break;
                }

                if (results.TimedOut)
                {
                    timedOut = ActiveCharacter.Team;
                    _winners = CharacterTeams.First(i => i != ActiveCharacter.Team);
                    break;
                }

                interaction = results.Result.Interaction;


                List<DiscordSelectComponentOption> enemiesToSelect = [];
                List<Character> possibleTargets = [];
                if (ActiveCharacter[battleDecision] is Move theMove)
                {
                    possibleTargets.AddRange(theMove.GetPossibleTargets());
                    foreach (var i in possibleTargets)
                    {
                        var isEnemy = i.Team != ActiveCharacter.Team;
                        enemiesToSelect.Add(new DiscordSelectComponentOption(
                            i.GetNameWithAlphabetIdentifier(isEnemy), i.GetNameWithAlphabetIdentifier(isEnemy)));
                    }

                }

                if (enemiesToSelect.Any())
                {

                    DiscordSelectComponent selectMoveTarget = new("targeter",
                        $"Select your target for {battleDecision}", enemiesToSelect);
                    var responseBuilder = new DiscordInteractionResponseBuilder()
                        .AddComponents(selectMoveTarget)
                        .AddComponents(infoSelect)
                        .AddComponents(_forfeitButton)
                        .AddEmbed(embedToEdit);
                    await results.Result.Interaction.CreateResponseAsync(
                        DiscordInteractionResponseType.UpdateMessage,
                        responseBuilder);
                    message = await results.Result.Interaction.GetOriginalResponseAsync();

                    InteractivityResult<ComponentInteractionCreatedEventArgs> interactivityResult;
                    using (_interruptionCancellationTokenSource = new CancellationTokenSource(TimeOutTimeSpan))
                    {
                        interactivityResult = await message.WaitForSelectAsync(e =>
                        {
                            if (e.User.Id == ActiveCharacter.Team.TryGetUserDataId
                                && e.Id == selectMoveTarget.CustomId)
                            {
                                var localTarget = Characters
                                    .First(i => i.GetNameWithAlphabetIdentifier(i.Team != ActiveCharacter.Team) ==
                                                e.Values.First().ToString());
                                if (possibleTargets.Contains(localTarget))
                                {
                                    target = localTarget;
                                    return true;
                                }
                            }

                            return false;
                        }, _interruptionCancellationTokenSource.Token);
                    }

                    if (_forfeited is not null)
                    {
                        _winners = CharacterTeams.First(i => i != _forfeited);
                        break;
                    }

                    if (interactivityResult.TimedOut)
                    {
                        timedOut = ActiveCharacter.Team;
                        _winners = CharacterTeams.First(i => i != ActiveCharacter.Team);
                        break;
                    }

                    interaction = interactivityResult.Result.Interaction;

                }
            }
            if (_forfeited is not null)
            {
                _winners = CharacterTeams.First(i => i != _forfeited);
                break;
            }
            if (_winners is not null)
            {
                break;
            }
            var move = ActiveCharacter[battleDecision];
            var moveResult =  move?.Utilize(target!, UsageType.NormalUsage);
            if (moveResult?.Text is not null)
            {
                _mainText = moveResult.Text;
            }

     
            
            // loops, taking counter attacks as top priority. if a counter attack is executed after doing other follow up
            // action, will do the counter attack straight up
            while (CounterAttackFollowUpActions is not null || OtherFollowUpActions is not null)
            {
                while (CounterAttackFollowUpActions is not null)
                {
                    foreach (var i in CounterAttackFollowUpActions.GetInvocationList())
                    {
                        
                        var asAction = (Action)i;
                        asAction();
                        CounterAttackFollowUpActions -= asAction;
                    }
                }
                if (OtherFollowUpActions is not null)
                {
                    foreach (var i in OtherFollowUpActions.GetInvocationList())
                    {
                        var asAction = (Action)i;
                        asAction();
                        OtherFollowUpActions -= asAction;
                        if (CounterAttackFollowUpActions is not null)
                        {
                            break;
                        }
                    }
                }

            }

            _canAddFollowUpAction = false;


            foreach (var i in ActiveCharacter.MoveList)
            {
                if (i is Special special && special.Cooldown > 0)
                {
                    special.Cooldown -= 1;
                }
            }

 
            foreach (var i in ActiveCharacter.StatusEffects)
            {
                if (i.ExecuteStatusEffectAfterTurn && ActiveCharacter.StatusEffects.Contains(i))
                {
                    i.PassTurn();
                    if (i.Duration <= 0) ActiveCharacter.RemoveStatusEffect(i);
                }
            }
        
            
            InvokeBattleEvent(new TurnEndEventArgs(ActiveCharacter));
       

        }
        // disposes the game cancellation token, since the game has ended so this token won't be chilling in the heap
      
        await _gameCancellationTokenSource.CancelAsync();
        
        _gameCancellationTokenSource.Dispose();
        _gameCancellationTokenSource = null!;
        return new BattleResult
        {
            Stopped = _stopped,
           

            Turns = Turn,
            Forfeited = _forfeited,
            Winners = _winners,
            TimedOut = timedOut,
            Message = message
        };
    }


}