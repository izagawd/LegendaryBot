using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text;
using BasicFunctionality;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Entities.LegendaryBot.BattleEvents.EventArgs;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.ModifierInterfaces;
using Entities.LegendaryBot.Moves;
using Entities.LegendaryBot.Results;
using Entities.LegendaryBot.StatusEffects;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using CharacterPartials_Character =
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace Entities.LegendaryBot.BattleSimulatorStuff;

public enum BattleDecision : byte
{
    Ultimate,
    BasicAttack,
    Skill,
    Other
}

public partial class BattleSimulator
{
    private static readonly DiscordButtonComponent
        ForfeitButton = new(DiscordButtonStyle.Danger, "forfeit", "Forfeit");


    protected static ConcurrentDictionary<string, Image<Rgba32>> CachedResizedForAvatars = new();
    private static readonly ConcurrentDictionary<Type, List<EventMethodDetails>> MethodsCache = [];

    protected static readonly DiscordButtonComponent Yes = new(DiscordButtonStyle.Success, "yes", "YES");

    protected static readonly DiscordButtonComponent No = new(DiscordButtonStyle.Success, "no", "NO");


    private readonly DiscordButtonComponent _basicAttackButton = new(DiscordButtonStyle.Secondary, nameof(BasicAttack),
        "", emoji: new DiscordComponentEmoji("⚔️"));


    private readonly List<BattleText> _battleTextInstances = ["Have fun!"];

    private readonly DiscordButtonComponent _skillButton = new(DiscordButtonStyle.Secondary, nameof(Skill), "",
        emoji: new DiscordComponentEmoji("🪄"));

    private readonly DiscordButtonComponent _ultimateButton = new(DiscordButtonStyle.Secondary, nameof(Ultimate), "",
        emoji: new DiscordComponentEmoji("⚡"));


    /// <summary>
    ///     The team that has forfeited
    /// </summary>
    private Team? _forfeited;


    /// <summary>
    ///     Cancelling this token means interrupting a game in some way. should be used if eg: one of the teams forfeits
    /// </summary>
    private CancellationTokenSource _gameCancellationTokenSource = null!;

    /// <summary>
    ///     Canceled if you want to interrupt waiting for an action from a player/bot in battle.
    ///     mainly used for forfeit, so it's cancelled when someone forfeit to immediately process forfeiting
    /// </summary>
    private CancellationTokenSource? _interruptionCancellationTokenSource;


    private string? _mainText = "battle begins";


    private bool _stopped;

    private Team? _winners;

    static BattleSimulator()
    {
        SetupBattleEventDelegatorStuff();
    }

    public BattleSimulator(Team team1, Team team2)
    {
        if (team1.Count == 0 || team2.Count == 0) throw new Exception("one of the teams has no fighters");

        Team1 = team1;
        Team2 = team2;
    }

    public IEnumerable<Team> Teams
    {
        get
        {
            yield return Team1;
            yield return Team2;
        }
    }

    /// <summary>
    ///     The character who is currently taking their turn
    /// </summary>
    public CharacterPartials_Character ActiveCharacter { get; protected set; } = null!;


    /// <summary>
    ///     All the characters in the battle
    /// </summary>
    public IEnumerable<CharacterPartials_Character> Characters => Team1.Union(Team2);

    /// <summary>
    ///     Creates a new battle between two teams
    /// </summary>
    public Team Team1 { get; }

    public Team Team2 { get; }

    /// <summary>
    ///     whether or not to set health to max health for all characters at start of battle
    /// </summary>
    public bool SetToMaxHealthAtStart { get; set; } = true;

    public int Turn { get; set; }

    public TimeSpan TimeOutTimeSpan { get; set; } = TimeSpan.FromMinutes(1.5);

    /// <summary>
    ///     How long it will take for AI to perform their attack when its their turn, and for some other stuff,
    ///     <br /> like when a character cant move, it will be the delay until the next turn is processed, probably
    /// </summary>
    public int WaitDelay { get; set; } = 5000;


    private async Task<Image<Rgba32>> GetAvatarAsync(string url)
    {
        if (CachedResizedForAvatars.TryGetValue(url, out var characterImageToDraw)) return characterImageToDraw.Clone();
        characterImageToDraw = await ImageFunctions.GetImageFromUrlAsync(url);
        characterImageToDraw.Mutate(mutator => { mutator.Resize(30, 30); });
        CachedResizedForAvatars[url] = characterImageToDraw;
        return characterImageToDraw.Clone();
    }


    public async Task<Image<Rgba32>> GetCombatImageAsync()
    {
        var heightToUse = Teams.Select(i => i.Count).Max() * 160;
        var image = new Image<Rgba32>(500, heightToUse);
        var xOffSet = 70;
        var widest = 0;
        var length = 0;
        var yOffset = 0;
        IImageProcessingContext imageCtx = null!;
        image.Mutate(ctx => imageCtx = ctx);

        foreach (var i in Teams)
        {
            foreach (var j in i)
            {
                using var characterCombatImage = await GenerateCharacterDetailsImageForCombatAsync(j);
                if (characterCombatImage.Width > widest) widest = characterCombatImage.Width;
                imageCtx.DrawImage(characterCombatImage, new Point(xOffSet, yOffset), new GraphicsOptions());


                yOffset += characterCombatImage.Height + 15;
                if (yOffset > length) length = yOffset;
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
            if (i.BattleTeam == Team2)
                circleBgColor = Color.DarkRed;
            else
                circleBgColor = Color.DarkBlue;

            characterImageToDraw.Mutate(j =>
                j.BackgroundColor(circleBgColor)
                    .ConvertToAvatar()
            );
            Color circleColor;
            if (i.BattleTeam == Team2)
                circleColor = Color.Red;
            else
                circleColor = Color.Blue;

            var characterImagePoint =
                new Point((combatReadinessLineTRectangle.X + combatReadinessLineTRectangle.Width / 2.0
                           - characterImageToDraw.Width / 2.0).Round(),
                    (i.CombatReadiness * length / 100).Round());

            var circlePolygon = new EllipsePolygon(characterImageToDraw.Width / 2.0f + characterImagePoint.X,
                characterImageToDraw.Height / 2.0f + characterImagePoint.Y,
                characterImageToDraw.Height / 2.0f);
            imageCtx
                .DrawImage(characterImageToDraw, characterImagePoint, new GraphicsOptions())
                .Draw(circleColor, 2, circlePolygon);
        }

        imageCtx.EntropyCrop();

        return image;
    }


    public static void Idk(BattleEventArgs idk)
    {
    }


    public IEnumerable<object> GetConnectedEntities()
    {
        yield return this;
        foreach (var i in Characters)
        {
            yield return i;
            foreach (var j in i.StatusEffects) yield return j;
            if (i.Blessing is not null) yield return i.Blessing;
        }
    }

    /// <summary>
    ///     Used to get entities connected to this battle simulator
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IEnumerable<T> GetConnectedEntities<T>()
    {
        foreach (var i in GetConnectedEntities())
            if (i is T tValue)
                yield return tValue;
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    private IEnumerable<BattleEventListenerMethodContainer> GetAllEventMethods()
    {
        foreach (var i in GetConnectedEntities())
            if (MethodsCache.TryGetValue(i.GetType(), out var methodDetailsList))
                foreach (var j in methodDetailsList)
                    yield return new BattleEventListenerMethodContainer(i, j);
    }

    /// <summary>
    ///     This will be used to invoke an event if it happens
    /// </summary>
    /// <param name="eventArgs">The argument instance of the battle event</param>
    /// <typeparam name="T">the type of argument of the battle event</typeparam>
    public void InvokeBattleEvent<T>(T eventArgs) where T : BattleEventArgs
    {
        var eventArgsType = eventArgs.GetType();
        foreach (var i in GetAllEventMethods()
                     .Where(k => eventArgsType.IsAssignableTo(k.EventMethodDetails.ParameterType))
                     .OrderByDescending(j => j.EventMethodDetails.Attribute.Priority))
            i.EventMethodDetails.BattleEventMethod.Invoke(i.Entity, eventArgs);
    }


    public IEnumerable<StatsModifierArgs> GetAllStatsModifierArgsInBattle()
    {
        foreach (var i in GetConnectedEntities<IStatsModifier>())
        foreach (var j in i.GetAllStatsModifierArgs())
            yield return j;
    }


    /// <summary>
    ///     Sets the winning team by checking if all the characters in a team are dead
    /// </summary>
    public void CheckForWinnerIfTeamIsDead()
    {
        if (_winners is not null) return;

        foreach (var i in Teams)
        {
            if (!i.All(j => j.IsDead)) continue;
            _winners = Teams.First(k => k != i);
            break;
        }
    }

    private IEnumerable<DiscordSelectComponentOption> GetSelectComponentOptions()
    {
        List<DiscordSelectComponentOption> toSelect = [];
        foreach (var i in Characters)
            toSelect.Add(new DiscordSelectComponentOption(i.NameWithAlphabet,
                i.NameWithAlphabet));

        return toSelect.ToArray();
    }

    private async Task HandleDisplayBattleInfoAsync(ComponentInteractionCreatedEventArgs interactionCreateEventArgs)
    {
        try
        {
            var interaction = interactionCreateEventArgs.Interaction;
            var gottenValue = interactionCreateEventArgs.Values.First();
            var characterToDisplayBattleInfo = Characters.FirstOrDefault(i => i.NameWithAlphabet == gottenValue);
            if (characterToDisplayBattleInfo is null)
            {
                await interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent("Could not find character selected for some reason. Try again!")
                        .AsEphemeral());
                return;
            }

            var descriptionStringBuilder = new StringBuilder();


            descriptionStringBuilder.Append(
                $" {string.Concat(Enumerable.Repeat("\u2b50",(int) characterToDisplayBattleInfo.Rarity))} • Element: {characterToDisplayBattleInfo.Element} • Combat Readiness: {characterToDisplayBattleInfo.CombatReadiness.Round()}%\n\n");
            foreach (var i in characterToDisplayBattleInfo.MoveList)
            {
                var moveTypeName = "Basic Attack :crossed_swords:";

                if (i is Skill skill)
                {
                    moveTypeName = "Skill :magic_wand:";
                    if (skill.IsPassive)
                        moveTypeName += " (passive)";
                }
                    
                else if (i is Ultimate) moveTypeName = "Ultimate :zap:";

                descriptionStringBuilder.Append(
                    $"{moveTypeName}: {i}.\nDescription: {i.GetDescription(characterToDisplayBattleInfo)}");
                if (i is Special special)
                    descriptionStringBuilder.Append(
                        $"\nMax Cooldown: {special.MaxCooldown}. Current Cooldown: {special.Cooldown}");

                descriptionStringBuilder.Append("\n\n");
            }


            var statusEffectsCopy = characterToDisplayBattleInfo.StatusEffects.ToArray();
            if (statusEffectsCopy.Any())
            {
                descriptionStringBuilder.Append("Status Effects\n\n");

                foreach (var effect in statusEffectsCopy)
                    descriptionStringBuilder.Append(
                        $"Name: {effect} | Type: {effect.EffectType} | Duration: {effect.Duration}\n");

                descriptionStringBuilder.Append('\n');
            }

            descriptionStringBuilder
                .Append("Stats\n");
            var shouldNewLine = false;
            foreach (var i in Enum.GetValues<StatType>())
            {
                descriptionStringBuilder.Append(
                    $"{i.GetShortName()}: {characterToDisplayBattleInfo.GetStatFromType(i)}");


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
                .WithAuthor(characterToDisplayBattleInfo.NameWithAlphabet,
                    iconUrl: characterToDisplayBattleInfo.ImageUrl)
                .WithTitle($"{characterToDisplayBattleInfo.NameWithAlphabet}'s description")
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

    private async Task HandleForfeitAsync(DiscordInteraction interaction)
    {
        try
        {
            var team = Teams
                .OfType<PlayerTeam>().First(i => i.UserData.DiscordId == interaction.User.Id);
            var embed = new DiscordEmbedBuilder()
                .WithTitle("For real?")
                .WithColor(DiscordColor.Blue)
                .WithDescription("Are you sure you want to forfeit?");

            await interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                    .AddEmbed(embed)
                    .AsEphemeral()
                    .AddComponents(Yes, No));

            var message = await interaction.GetOriginalResponseAsync();

            var result = await message.WaitForButtonAsync(i => i.User.Id == interaction.User.Id,
                new TimeSpan(0, 0, 30));
            if (result.TimedOut) return;
            string text = "Forfeitation Cancelled";
            if (result.Result.Id == "yes")
            {
                if (_forfeited is null)
                    _forfeited = team;
                if (_interruptionCancellationTokenSource is not null &&
                    !_interruptionCancellationTokenSource.IsCancellationRequested)
                    await _interruptionCancellationTokenSource.CancelIfNotDisposedAsync();
                text = "Forfeitation succeeded";

            }

            await result.Result.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage,
                new DiscordInteractionResponseBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                        .WithColor(DiscordColor.Blue)
                      
                        .WithTitle("Forfeit")
                        .WithDescription(text)));
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private void CheckForForfeitOrInfoTillEndOfBattle(DiscordMessage message, CancellationToken token)
    {
        _ = message.WaitForSelectAsync(args =>
        {
            if (args.Id == "info") _ = HandleDisplayBattleInfoAsync(args);
            return false;
        }, token);
        _ = message.WaitForButtonAsync(args =>
        {
            if (args.Id == "forfeit" &&
                Teams.OfType<PlayerTeam>().Any(i => i.UserData.DiscordId == args.User.Id))
            {
                _ = HandleForfeitAsync(args.Interaction);
                return false;
            }

            return false;
        }, token);
    }

    /// <summary>
    ///     Initiates a new battle between two teams, by editing the provided message
    /// </summary>
    public Task<BattleResult> StartAsync(DiscordMessage message)
    {
        return StartAsync(message, null);
    }

    /// <summary>
    ///     Initiates a new battle between two teams, by editing the provided message with responding to the interaction
    /// </summary>
    /// <param name="interaction"></param>
    /// <returns></returns>
    public Task<BattleResult> StartAsync(DiscordInteraction interaction)
    {
        return StartAsync(null,
            interaction);
    }

    public void AddBattleText(BattleText battleTextInstance)
    {
        if (battleTextInstance is null) throw new ArgumentNullException(nameof(battleTextInstance));
        _battleTextInstances.Add(battleTextInstance);
    }


    /// <summary>
    ///     Initiates a new battle between two teams, by sending a message to a channel
    /// </summary>
    public Task<BattleResult> StartAsync(DiscordChannel channel)
    {
        return StartAsync(null, null, channel);
    }

    public Alphabet GetAlphabetIdentifier(CharacterPartials_Character character)
    {
        return (Alphabet)Characters.IndexOf(character);
    }


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

    public void PrepareTeamForBattle(Team team)
    {
        team.CurrentBattle = this;
        foreach (var i in team)
        {
            i.BattleTeam = team;
            if (i.Blessing is not null)
            {
                i.Blessing.Character = i;
            }
        }
    }

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
        _stopped = false;

        foreach (var i in Teams) PrepareTeamForBattle(i);
        foreach (var i in Characters)
            if (SetToMaxHealthAtStart)
                i.Health = i.MaxHealth;

        Team? timedOut = null;


        CharacterPartials_Character? target = null;

        // If you want the bot to update a message using an interactivity result instead of without, use this
        InvokeBattleEvent(new BattleBeginEventArgs());
        ImmutableArray<DiscordButtonComponent> components =
            [_basicAttackButton, _skillButton, _ultimateButton, ForfeitButton];

        while (true)
        {
            Turn += 1;
            var extraTurnGranted = false;
            var extraTurners =
                Characters.Where(i => i.ShouldTakeExtraTurn)
                    .ToArray();

            if (extraTurners.Any())
            {
                ActiveCharacter = BasicFunctions.RandomChoice(extraTurners.AsEnumerable());
                ActiveCharacter.ShouldTakeExtraTurn = false;
                extraTurnGranted = true;
            }

            while (!extraTurnGranted && !Characters.Any(i => i.CombatReadiness >= 100 && !i.IsDead))
                foreach (var j in Characters)
                    if (!j.IsDead)
                        j.CombatReadiness += 0.0025f * j.Speed;

            if (!extraTurnGranted)
                ActiveCharacter =
                    BasicFunctions.RandomChoice(Characters.Where(i => i.CombatReadiness >= 100 && !i.IsDead));

            if (_mainText is null)
                _mainText = $"{ActiveCharacter}'s turn";


            ActiveCharacter.CombatReadiness = 0;

            foreach (var i in ActiveCharacter.StatusEffects.ToArray())
                //this code executes for status effects that occur just before the beginning of the turn
                if (i.ExecuteStatusEffectBeforeTurn && ActiveCharacter.StatusEffects.Contains(i))
                {
                    i.PassTurn();
                    if (i.Duration <= 0) ActiveCharacter.RemoveStatusEffect(i);
                }


            InvokeBattleEvent(new TurnStartEventArgs(ActiveCharacter));
            var shouldDoTurn = !ActiveCharacter.IsDead;
            if (!shouldDoTurn)
                AddBattleText(
                    $"{ActiveCharacter.NameWithAlphabet} cannot take their turn because they died in the process of taking their turn!");


            var additionalText = BattleText
                .Combine(_battleTextInstances)
                .Select(i => i.Text).Join("\n");
            _battleTextInstances.Clear();

            if (additionalText.Length == 0) additionalText = "No definition";
            else if (additionalText.Length > 1024)
                additionalText = additionalText.Substring(0, 1021) + "...";

            var embedToEdit = new DiscordEmbedBuilder()
                .WithTitle("**BATTLE!!!**")
                .WithAuthor($"{ActiveCharacter.Name} [{ActiveCharacter.AlphabetIdentifier}]",
                    iconUrl: ActiveCharacter.ImageUrl)
                .WithColor(ActiveCharacter.Color)
                .AddField(_mainText, additionalText)
                .WithImageUrl("attachment://battle.png");


            using var combatImage = await GetCombatImageAsync();

            await using var stream = new MemoryStream();
            await combatImage.SaveAsPngAsync(stream);

            stream.Position = 0;

            var messageBuilder = new DiscordMessageBuilder()
                .AddEmbed(embedToEdit.Build())
                .AddFile("battle.png", stream);

            CheckForWinnerIfTeamIsDead();

            foreach (var i in components
                         .Where(i => i != ForfeitButton))
                i.Disable();
            if (!(ActiveCharacter.BattleTeam is not PlayerTeam || ActiveCharacter.IsOverriden)
                && shouldDoTurn
                && _winners is null && !_stopped)
            {
                _basicAttackButton.Enable();

                if (ActiveCharacter.Skill is not null && ActiveCharacter.Skill.CanBeUsedNormally()) _skillButton.Enable();
                if (ActiveCharacter.Ultimate is not null && ActiveCharacter.Ultimate.CanBeUsedNormally())
                    _ultimateButton.Enable();
            }

            var infoSelect = new DiscordSelectComponent("info", "View info of character",
                GetSelectComponentOptions());
            messageBuilder
                .AddComponents(components)
                .AddComponents(infoSelect);


            if (interaction is not null)
            {
                if (interaction.ResponseState != DiscordInteractionResponseState.Replied)
                {
                    await interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage,
                        new DiscordInteractionResponseBuilder(messageBuilder));
                    message = await interaction.GetOriginalResponseAsync();
                }
                else
                {
                    message = await interaction.CreateFollowupMessageAsync(
                        new DiscordFollowupMessageBuilder(messageBuilder));
                }

                interaction = null;
            }
            else if (message is not null)
            {
                message = await message.ModifyAsync(messageBuilder);
            }
            else if (channel is not null)
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
                CheckForForfeitOrInfoTillEndOfBattle(message, _gameCancellationTokenSource.Token);
            }


            _mainText = null;
            if (_forfeited is not null) _winners = Teams.First(i => i != _forfeited);
            if (_winners is not null)
            {
                await Task.Delay(WaitDelay);
                break;
            }

            var battleDecision = BattleDecision.Other;

            StatusEffect? mostPowerfulStatusEffect = null;


            mostPowerfulStatusEffect = ActiveCharacter.StatusEffects.OrderByDescending(i => i.OverrideTurnType)
                .FirstOrDefault();


            if (!shouldDoTurn)
            {
                using (_interruptionCancellationTokenSource = new CancellationTokenSource())
                {
                    await BasicFunctions.DelayWithTokenNoError(WaitDelay,
                        _interruptionCancellationTokenSource.Token);
                }
            }
            else if (mostPowerfulStatusEffect is not null && mostPowerfulStatusEffect.OverrideTurnType > 0)
            {
                var overridenUsageText = mostPowerfulStatusEffect.OverridenUsage(ref target!,
                    ref battleDecision, MoveUsageType.NormalUsage);
                if (overridenUsageText is not null) _mainText = overridenUsageText;
                using (_interruptionCancellationTokenSource = new CancellationTokenSource())
                {
                    await BasicFunctions.DelayWithTokenNoError(WaitDelay,
                        _interruptionCancellationTokenSource.Token);
                }
            }

            else if (ActiveCharacter.BattleTeam is not PlayerTeam)
            {
                ActiveCharacter.NonPlayerCharacterAi(ref target!, ref battleDecision);


                using (_interruptionCancellationTokenSource = new CancellationTokenSource())
                {
                    await BasicFunctions.DelayWithTokenNoError(WaitDelay,
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
                        if (!Teams.OfType<PlayerTeam>().Any(i => i.UserData.DiscordId == e.User.Id))
                            return false;
                        if (!Enum.TryParse(e.Id, out BattleDecision localDecision)) return false;

                        if (ActiveCharacter.BattleTeam is PlayerTeam playerTeam
                            && e.User.Id == playerTeam.UserData.DiscordId
                            && ((IEnumerable<BattleDecision>)
                            [
                                BattleDecision.BasicAttack, BattleDecision.Skill, BattleDecision.Ultimate
                            ]).Contains(localDecision) &&
                            (ActiveCharacter[localDecision]?.CanBeUsedNormally()).GetValueOrDefault(false))
                        {
                            battleDecision = localDecision;
                            return true;
                        }

                        return false;
                    }, _interruptionCancellationTokenSource.Token);
                }

                if (_forfeited is not null)
                {
                    _winners = Teams.First(i => i != _forfeited);
                    break;
                }

                if (results.TimedOut)
                {
                    timedOut = ActiveCharacter.BattleTeam;
                    _winners = Teams.First(i => i != ActiveCharacter.BattleTeam);
                    break;
                }

                interaction = results.Result.Interaction;


                List<DiscordSelectComponentOption> enemiesToSelect = [];
                List<CharacterPartials_Character> possibleTargets = [];
                if (ActiveCharacter[battleDecision] is { } theMove)
                {
                    possibleTargets.AddRange(theMove.GetPossibleTargets());
                    foreach (var i in possibleTargets)
                    {
                        var isEnemy = i.BattleTeam != ActiveCharacter.BattleTeam;
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
                        .AddComponents(ForfeitButton)
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
                            if (ActiveCharacter.BattleTeam is PlayerTeam playerTeam &&
                                e.User.Id == playerTeam.UserData.DiscordId
                                && e.Id == selectMoveTarget.CustomId)
                            {
                                var localTarget = Characters
                                    .First(i => i.GetNameWithAlphabetIdentifier(i.BattleTeam !=
                                                    ActiveCharacter.BattleTeam) ==
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
                        _winners = Teams.First(i => i != _forfeited);
                        break;
                    }

                    if (interactivityResult.TimedOut)
                    {
                        timedOut = ActiveCharacter.BattleTeam;
                        _winners = Teams.First(i => i != ActiveCharacter.BattleTeam);
                        break;
                    }

                    interaction = interactivityResult.Result.Interaction;
                }
            }

            if (_forfeited is not null)
            {
                _winners = Teams.First(i => i != _forfeited);
                break;
            }

            if (_winners is not null) break;
            var move = ActiveCharacter[battleDecision];
            var moveResult = move?.Utilize(target!, MoveUsageType.NormalUsage);
            if (moveResult?.Text is not null) _mainText = moveResult.Text;


            foreach (var i in ActiveCharacter.MoveList)
                if (i is Special special && special.Cooldown > 0)
                    special.Cooldown -= 1;


            foreach (var i in ActiveCharacter.StatusEffects.ToArray())
                if (i.ExecuteStatusEffectAfterTurn && ActiveCharacter.StatusEffects.Contains(i))
                {
                    i.PassTurn();
                    if (i.Duration <= 0) ActiveCharacter.RemoveStatusEffect(i);
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

    private class EventMethodDetails
    {
        public EventMethodDetails(BattleEventMethod battleEventMethod, BattleEventListenerMethodAttribute attribute,
            Type parameterType)
        {
            BattleEventMethod = battleEventMethod;
            Attribute = attribute;
            ParameterType = parameterType;
        }

        public BattleEventMethod BattleEventMethod { get; }
        public BattleEventListenerMethodAttribute Attribute { get; }
        public Type ParameterType { get; }
    }

    private struct BattleEventListenerMethodContainer
    {
        /// <summary>
        ///     The entity to invoke the method with
        /// </summary>
        public object Entity { get; }

        /// <summary>
        ///     The method to invoke
        /// </summary>
        public EventMethodDetails EventMethodDetails { get; }

        public BattleEventListenerMethodContainer(object entity, EventMethodDetails eventMethodDetails)
        {
            Entity = entity;
            EventMethodDetails = eventMethodDetails;
        }
    }
}