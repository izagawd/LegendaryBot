using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.ModifierInterfaces;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.Rewards;
using DiscordBotNet.LegendaryBot.StatusEffects;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Caching.Memory;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;
using UserExperienceReward = DiscordBotNet.LegendaryBot.Rewards.UserExperienceReward;

namespace DiscordBotNet.LegendaryBot.BattleSimulatorStuff;

public enum BattleDecision
{
    
    Forfeit, Ultimate, BasicAttack, Skill, Info,Other
}


public class BattleSimulator
{


    public static DiscordButtonComponent basicAttackButton = new(ButtonStyle.Secondary, nameof(BasicAttack), null,emoji: new DiscordComponentEmoji("⚔️"));
    public static DiscordButtonComponent skillButton = new(ButtonStyle.Secondary, nameof(Skill), null, emoji: new DiscordComponentEmoji("🪄"));
    public static DiscordButtonComponent ultimateButton = new(ButtonStyle.Secondary, nameof(Ultimate), null, emoji: new DiscordComponentEmoji("⚡"));
    public static DiscordButtonComponent forfeitButton = new(ButtonStyle.Danger, "Forfeit", "Forfeit");

    public static DiscordButtonComponent proceed = new(ButtonStyle.Success, "Proceed", "Proceed");


    protected static MemoryCache _cachedResizedForAvatarsMemoryCache = new(new MemoryCacheOptions());
    
    
    protected static MemoryCacheEntryOptions EntryOptions { get; } = new()
    {
        SlidingExpiration = new TimeSpan(0,30,0),
        PostEvictionCallbacks = { new PostEvictionCallbackRegistration(){EvictionCallback = BasicFunctionality.DisposeEvictionCallback} }
    };
    protected static MemoryCacheEntryOptions ExpiredEntryOptions { get; } = new()
    {
        SlidingExpiration = new TimeSpan(0,30,0),
        PostEvictionCallbacks = { new PostEvictionCallbackRegistration(){EvictionCallback = BasicFunctionality.DisposeEvictionCallback} }
    };

    private bool _battleBegun = false;
    private  async Task<Image<Rgba32>> GetAvatarAsync(string url)
    {
        if(_cachedResizedForAvatarsMemoryCache.TryGetValue(url, out Image<Rgba32> characterImageToDraw))
        {
            return characterImageToDraw.Clone();
        }
        characterImageToDraw = await BasicFunctionality.GetImageFromUrlAsync(url);
        characterImageToDraw.Mutate(mutator =>
        {
            mutator.Resize(30, 30);
        });
        var entryOptions = EntryOptions;
        if (!url.Contains(Website.DomainName)) entryOptions = ExpiredEntryOptions;
        _cachedResizedForAvatarsMemoryCache.Set(url, characterImageToDraw, entryOptions);
        return characterImageToDraw.Clone();
    }

    
    public async Task<Image<Rgba32>> GetCombatImageAsync()
    {

        var stop = new Stopwatch(); stop.Start();
        var heightToUse = CharacterTeams.Select(i => i.Count).Max() * 160;
        var image = new Image<Rgba32>(500, heightToUse);
        int xOffSet = 70;
        int widest = 0;
        int length = 0;
        int yOffset = 0;
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
        $"battle simulator image time: {stop.Elapsed.TotalMilliseconds}".Print();
        return image;
    }



    static BattleSimulator()
    {
        foreach (var i in Assembly.GetExecutingAssembly().GetTypes().Where(j => !j.IsAbstract && j.GetInterfaces().Contains(typeof(IBattleEventListener))))
        {
            _methodsCache[i] = i.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(j => j.GetCustomAttribute<BattleEventListenerMethodAttribute>() is not null)
                .ToDictionary(j => j, j => j.GetCustomAttribute<BattleEventListenerMethodAttribute>())!;
        }

        List<MethodInfo> invalidMethods = [];

        foreach (var i in _methodsCache.SelectMany(i => i.Value.Keys))
        {
            if(i.GetParameters().Length != 1 ||!i.GetParameters()[0].ParameterType.IsRelatedToType(typeof(BattleEventArgs)) )
                invalidMethods.Add(i);

        }

        var stringBuilder = new StringBuilder();
        if (invalidMethods.Count > 0)
        {
            stringBuilder.Append(
                $"The following methods need to have one parameter, " +
                $"and that one parameter should be a type or subtype of {nameof(BattleEventArgs)},\n"
                +$"since it uses the {nameof(BattleEventListenerMethodAttribute)} to listen to events:\n");

            foreach (var i in invalidMethods)
            {
                stringBuilder.Append($"Method \"{i.Name}\" from class \"{i.DeclaringType}\"");
            }

            stringBuilder.ToString().Print();
            Environment.Exit(1);
        }
    }

    private static ConcurrentDictionary<Type, Dictionary<MethodInfo,BattleEventListenerMethodAttribute>> _methodsCache = [];
    
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
        public MethodInfo MethodInfo { get; }
        
        /// <summary>
        /// The BattleEventListener attribute the method has
        /// </summary>
        public BattleEventListenerMethodAttribute Attribute { get;}
        public BattleEventListenerMethodContainer(IBattleEventListener entity,MethodInfo methodInfo,
            BattleEventListenerMethodAttribute attribute)
        {

            Entity = entity;
            MethodInfo = methodInfo;
            Attribute = attribute;
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
                yield return new BattleEventListenerMethodContainer(i, j.Key, j.Value);
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
        foreach (var i in GetAllEventMethods()
                     .Where(k => eventArgs.GetType().IsRelatedToType(k.MethodInfo.GetParameters()[0].ParameterType))
                     .OrderByDescending(j => j.Attribute.Priority))
        {
            i.MethodInfo.Invoke(i.Entity, [eventArgs]);
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
    /// Stops the battle, leaving no winners
    /// </summary>
    public void Stop(Character stopper)
    {
        _winners = CharacterTeams.First(i => i != stopper.Team);
        _stopped = true;
    }
  

    private DiscordMessage _message;
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

    private async Task HandleDisplayBattleInfoAsync( ComponentInteractionCreateEventArgs interactionCreateEventArgs)
    {
        try
        {
            var interaction = interactionCreateEventArgs.Interaction;
            var gottenValue = interactionCreateEventArgs.Values.First();
            var characterToDisplayBattleInfo = Characters.First(i => i.NameWithAlphabetIdentifier == gottenValue);


            var descriptionStringBuilder = new StringBuilder();


            descriptionStringBuilder.Append($"Combat Readiness: {characterToDisplayBattleInfo.CombatReadiness.Round()}%\n\n");
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
            if (characterToDisplayBattleInfo.StatusEffects.Any())
            {
                descriptionStringBuilder.Append("Status Effects\n\n");

                Dictionary<StatusEffect, int> statusCounts = [];
                foreach (var i in statusEffectsCopy)
                {
                    var instance = statusCounts.Keys
                        .FirstOrDefault(j => j.GetType() == i.GetType());
                    if (instance is null)
                    {
                        instance = i;
                        statusCounts[i] = 1;
                    }
                    else
                    {
                        statusCounts[instance]++;
                    }
                }

                foreach (var i in statusCounts)
                {
                    var effect = i.Key;
                    descriptionStringBuilder.Append(
                        $"Name: {effect}. Type: {effect.EffectType} Count: {i.Value}\nDescription: {effect.Description}\n\n");
                }
            }


            var embed = new DiscordEmbedBuilder()
                .WithAuthor(characterToDisplayBattleInfo.NameWithAlphabetIdentifier, iconUrl: characterToDisplayBattleInfo.ImageUrl)
                .WithTitle($"{characterToDisplayBattleInfo} [{characterToDisplayBattleInfo.AlphabetIdentifier}]'s description")
                .WithColor(characterToDisplayBattleInfo.Color)
                .WithDescription(descriptionStringBuilder.ToString());



            await interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                    .AddEmbed(embed)
                    .AsEphemeral());

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

    }


    protected static DiscordButtonComponent yes = new DiscordButtonComponent(ButtonStyle.Success, "yes", "YES");
    
    protected static DiscordButtonComponent no = new DiscordButtonComponent(ButtonStyle.Success, "no", "NO");
    private async Task HandleForfeitAsync(DiscordInteraction interaction)
    {
        
        try
        {
            var team = CharacterTeams
                .OfType<PlayerTeam>().First(i => i.UserDataId == (long)interaction.User.Id);
            var embed = new DiscordEmbedBuilder()
                .WithTitle("For real?")
                .WithColor(DiscordColor.Blue)
                .WithDescription("Are you sure you want to forfeit?");

            await interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                    .AddEmbed(embed)
                    .AsEphemeral()
                    .AddComponents(yes, no));

            var message = await interaction.GetOriginalResponseAsync();

            var result = await message.WaitForButtonAsync(i => i.User.Id == interaction.User.Id);
            if (result.TimedOut) return;
            if (result.Result.Id == "yes")
            {
                _forfeited = team;
                await CancellationTokenSource.CancelAsync();

            }

            await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);
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
            _battleSimulator.pauseCount++;
          
        }

        private bool disposed = false;

        public void Dispose()
        {
            if(disposed) return;

            _battleSimulator.pauseCount--;
            disposed = true;

            if(_battleSimulator.IsEventsPaused) return;
            _battleSimulator.pauseCount = 0;
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
    private int pauseCount = 0;
    public bool IsEventsPaused => pauseCount > 0;

    /// <summary>
    /// Battle events will be paused until this scope is disposed
    /// </summary>
    public PauseBattleEventsInstance PauseBattleEventScope => new(this);

    public IEnumerable<int> bruh()
    {
        yield return 3;
        
    }

    private async Task CheckForForfeitOrInfoAsync()
    {
        using (CancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
        {
            var interactivityResult = await _message.WaitForAnyComponentInteractionAsync(e =>
            {
                if (!CharacterTeams.Any(i => i.TryGetUserDataId == (long)e.User.Id)) return false;
                BattleDecision localDecision;
                var didParse = Enum.TryParse(e.Id, out localDecision);
                if (!didParse) return false;
                if (localDecision == BattleDecision.Forfeit)
                {
                    Task.Run(() => HandleForfeitAsync(e.Interaction));
                    return false;
                } 
                if (localDecision == BattleDecision.Info)
                {
                    
                    Task.Run(() => HandleDisplayBattleInfoAsync(e));
                    return false;
                }
                

                return false;
            }, CancellationTokenSource);
            if(interactivityResult.TimedOut) return;
            await interactivityResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
        }
    }
    public int Turn { get; set; } = 0;

    public TimeSpan TimeOutTimeSpan { get;  set; } = TimeSpan.FromMinutes(1.5);
    /// <summary>
    /// Initiates a new battle between two teams, by editing the provided message
    /// </summary>
    public  Task<BattleResult> StartAsync(DiscordMessage message)
    {
        return StartAsync(messageInput : message, interaction: null);
    }
    
   /// <summary>
   /// Initiates a new battle between two teams, by editing the provided message with responding to the interaction
   /// </summary>
   /// <param name="isInteractionEdit">is true, edits the interaction message. if not, sends a follow up message</param>
   /// <returns></returns>
    public  Task<BattleResult> StartAsync( DiscordInteraction interaction, bool isInteractionEdit = true)
    {
        return StartAsync(messageInput: null,
            interaction: interaction, editInteraction: isInteractionEdit);
    }




    private List<AdditionalBattleText> _battleTextInstances = ["Have fun!"];

    public void AddAdditionalBattleText(AdditionalBattleText additionalBattleTextInstance)
    {
        if(additionalBattleTextInstance is null) return;
       _battleTextInstances.Add(additionalBattleTextInstance);
    }


    /// <summary>
    /// Initiates a new battle between two teams, by sending a message to a channel
    /// </summary>
    public  Task<BattleResult> StartAsync(DiscordChannel channel)
    {
        return StartAsync(messageInput: null,interaction: null, channel: channel);
    }
    public Alphabet GetAlphabetIdentifier(Character character)
    {
         return (Alphabet)Characters.ToList().IndexOf(character);
    }

 
    private CancellationTokenSource CancellationTokenSource;
    protected async Task<BattleResult> StartAsync(
        DiscordMessage? messageInput = null, DiscordInteraction? interaction = null,
        DiscordChannel? channel = null, bool editInteraction = true)
    {
        pauseCount = 0;
        _stopped = false;
        _message = null;
        Team1.CurrentBattle = this;
        Team2.CurrentBattle = this;
        foreach (var i in CharacterTeams)
        {
            foreach (var j in i)
            {
                j.Team = i;
                SetupCharacterForThisBattle(j);
            }
        }
        
        CharacterTeam? timedOut = null;

        
        Character? target = null; 
        
        // If you want the bot to update a message using an interactivity result instead of without, use this

        while (true)
        {
            var stop = new Stopwatch();
            stop.Start();
            
            Turn += 1;
            if (!_battleBegun)
            {
                _battleBegun = true;
                InvokeBattleEvent(new BattleBeginEventArgs());
            }
            bool extraTurnGranted = false;
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
            "yo".Print();
            if (!extraTurnGranted)
            {
                ActiveCharacter = BasicFunctionality.RandomChoice(Characters.Where(i => i.CombatReadiness >= 100 && !i.IsDead));

            }

            if (_mainText is null)
                _mainText = $"{ActiveCharacter}'s turn";

            
            ActiveCharacter.CombatReadiness = 0;
            using (PauseBattleEventScope)
            {
                foreach (var i in ActiveCharacter.StatusEffects)
                {

                    //this code executes for status effects that occur just before the beginning of the turn
                    if (i.ExecuteStatusEffectBeforeTurn)
                    {
                        i.PassTurn();
                        if (i.Duration <= 0) ActiveCharacter.RemoveStatusEffect(i);
                    }
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

            DiscordEmbedBuilder embedToEdit = new DiscordEmbedBuilder()
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

            var components = new List<DiscordComponent>();
            if (!(!ActiveCharacter.Team.IsPlayerTeam || ActiveCharacter.IsOverriden) 
                && shouldDoTurn
                && _winners is null && !_stopped)
            {
                components.Add(basicAttackButton);

                if (ActiveCharacter.Skill is not null &&  ActiveCharacter.Skill.CanBeUsed())
                {
                    components.Add(skillButton);
                }
                if (ActiveCharacter.Ultimate is not null && ActiveCharacter.Ultimate.CanBeUsed())
                {
                    components.Add(ultimateButton);
                }
            }
            components.Add(forfeitButton);
            var infoSelect = new DiscordSelectComponent("Info", "View info of character",
                GetSelectComponentOptions());
            messageBuilder
                .AddComponents(components)
                .AddComponents(infoSelect);

            stop.Stop();
            if(interaction is not null)
            {
                
                if (editInteraction)
                {
                    await interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                        new DiscordInteractionResponseBuilder(messageBuilder));
                    _message = await interaction.GetOriginalResponseAsync();
                }
                else
                {
                    _message = await interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder(messageBuilder));
                }
            }
            else if(_message is not null)
            {
                _message = await _message.ModifyAsync(messageBuilder);
            }
            else if (messageInput is not null)
            {
                _message = await messageInput.ModifyAsync(messageBuilder);
            }
            else if(channel is not null)
            {
                _message = await channel.SendMessageAsync(messageBuilder);
            }
            else
            {
                throw new Exception("No way to display battle");
            }
            stop.Start();
            messageInput = null;
            interaction = null;
            channel = null;
            editInteraction = true;


            _mainText = null;
            if (_winners is not null) 
            {
                await Task.Delay(5000); break;
            }

            var battleDecision =BattleDecision.Other;
 
            StatusEffect? mostPowerfulStatusEffect = null;
            var copy = ActiveCharacter.StatusEffects;
            if (copy.Any())
                mostPowerfulStatusEffect = copy.OrderByDescending(i => i.OverrideTurnType).First();
        
            if (!shouldDoTurn) await Task.Delay(5000);
 
            else if ( mostPowerfulStatusEffect is not null &&  mostPowerfulStatusEffect.OverrideTurnType > 0 )
            {

                UsageResult overridenUsage  = mostPowerfulStatusEffect.OverridenUsage(ActiveCharacter,ref target!, ref battleDecision, UsageType.NormalUsage);
                if (overridenUsage.Text is not null) _mainText = overridenUsage.Text;
                await CheckForForfeitOrInfoAsync();
            }
            
            else if (!ActiveCharacter.Team.IsPlayerTeam)
            {
               ActiveCharacter.NonPlayerCharacterAi(ref target!, ref battleDecision);
               await CheckForForfeitOrInfoAsync();
            }
            else
            {
                InteractivityResult<ComponentInteractionCreateEventArgs> results;
                using (CancellationTokenSource = new CancellationTokenSource(TimeOutTimeSpan))
                {
                    results = await _message.WaitForAnyComponentInteractionAsync(e =>
                    {
                        if (!CharacterTeams.Any(i => i.TryGetUserDataId == (long)e.User.Id)) return false;
                        BattleDecision localDecision;
                        var didParse = Enum.TryParse(e.Id, out localDecision);
                        if (!didParse) return false;
                        if (localDecision == BattleDecision.Forfeit)
                        {
                            Task.Run(() => HandleForfeitAsync(e.Interaction));
                            return false;
                        
                        }
                        if (localDecision == BattleDecision.Info)
                        {
                            Task.Run(() => HandleDisplayBattleInfoAsync(e));
                            return false;
                        }
                        if ((long)e.User.Id == ActiveCharacter.Team.TryGetUserDataId)
                        {
                            battleDecision = localDecision;
                            return true;
                        }
                        return false;
                    }, CancellationTokenSource);
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
                if ( ActiveCharacter[battleDecision] is Move theMove)
                {
                    possibleTargets.AddRange(theMove.GetPossibleTargets());
                    foreach (var i in possibleTargets)
                    {
                        bool isEnemy = i.Team != ActiveCharacter.Team;
                        enemiesToSelect.Add(new DiscordSelectComponentOption(i.GetNameWithAlphabetIdentifier(isEnemy), i.GetNameWithAlphabetIdentifier(isEnemy)));
                    }
                        
                }
                if (enemiesToSelect.Any())
                {

                    DiscordSelectComponent selectMoveTarget = new("targeter",$"Select your target for {battleDecision}", enemiesToSelect);
                    var responseBuilder = new DiscordInteractionResponseBuilder()
                        .AddComponents(selectMoveTarget)
                        .AddComponents(infoSelect)
                        .AddComponents(forfeitButton)
                        .AddEmbed(embedToEdit);
                    await results.Result.Interaction.CreateResponseAsync(
                        InteractionResponseType.UpdateMessage,
                        responseBuilder);
                    _message = await results.Result.Interaction.GetOriginalResponseAsync();

                    InteractivityResult<ComponentInteractionCreateEventArgs> interactivityResult;
                    using (CancellationTokenSource = new CancellationTokenSource(TimeOutTimeSpan))
                    {
                        interactivityResult = await  _message.WaitForAnyComponentInteractionAsync(e =>
                        {
                            if ((long) e.User.Id == ActiveCharacter.Team.TryGetUserDataId 
                                && e.Id == selectMoveTarget.CustomId)
                            {
                                target = Characters
                                    .First(i => i.GetNameWithAlphabetIdentifier(i.Team != ActiveCharacter.Team) == e.Values.First().ToString());
                                return true;
                            }
                            if (!CharacterTeams.Any(i => i.TryGetUserDataId == (long)e.User.Id)) return false;
                            var localDecision = BattleDecision.Other;
                            var didParse = Enum.TryParse(e.Id, out localDecision);
                            if (!didParse) return false;

                            if (localDecision == BattleDecision.Forfeit)
                            {
                                Task.Run(() => HandleForfeitAsync(e.Interaction));
                                return false;
                            }

                            if (localDecision == BattleDecision.Info)
                            {
                                Task.Run(() => HandleDisplayBattleInfoAsync(e));
                                return false;
                            }
                            
                            return false;
                        }, CancellationTokenSource);
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
            
                await Task.Delay(5000); break;
                
            }
            var move = ActiveCharacter[battleDecision];

  
            
            var moveResult =  move?.Utilize(target, UsageType.NormalUsage);

            if (moveResult?.Text is not null)
            {
                _mainText = moveResult.Text;
            }




            foreach (var i in ActiveCharacter.MoveList)
            {
                if (i is Special special && special.Cooldown > 0)
                {
                    special.Cooldown -= 1;
                }
            }

            using (PauseBattleEventScope)
            {
                foreach (var i in ActiveCharacter.StatusEffects)
                {

                    if (i.ExecuteStatusEffectAfterTurn)
                    {
                        i.PassTurn();
                        if (i.Duration <= 0) ActiveCharacter.RemoveStatusEffect(i);
                    }
                }
            }
            
            InvokeBattleEvent(new TurnEndEventArgs(ActiveCharacter));
       

        }

        var losers = CharacterTeams.First(i => i != _winners);
       
       
        var expToGain= losers.Sum(i =>BattleFunctionality.ExpGainFormula(i.Level) * i.ExpIncreaseScale);
        var coinsToGain = losers.Sum(i => (i.Level + 60) * 100)/_winners.Count;

        List<Reward> rewards = [new CoinsReward(coinsToGain), new UserExperienceReward(expToGain)];
        foreach (var i in losers)
        {
            if (i.IsDead) rewards.AddRange(i.DroppedRewards);
        }
        


        return new BattleResult
        {
            Stopped = _stopped,
            ExpToGain = expToGain,
            BattleRewards = rewards,
            Turns = Turn,
            Forfeited = _forfeited,
            Winners = _winners,
            TimedOut = timedOut,
            Message = _message
        };
    }


}