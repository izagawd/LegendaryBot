using DiscordBotNet.LegendaryBot.Results;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

namespace DiscordBotNet.LegendaryBot.DialogueNamespace;

/// <summary>
/// Creates dialogue of a character
/// </summary>
public class Dialogue
{

    public  required string Title { get; init; } = "untitled";
    public IEnumerable<DialogueNormalArgument> NormalArguments { get; init; } = [];
    /// <summary>
    /// Whether or not the dialogue can be skipped
    /// </summary>
    public bool Skippable { get; init; } = true;
    
    /// <summary>
    /// Set it to something when you want the user to make a decision at the end of the dialogue
    /// </summary>
    public DialogueDecisionArgument? DecisionArgument { get; init; }

    /// <summary>
    /// Responds to the interaction of the provided context if true. Used if the command is being responded to
    /// with a dialogue
    /// </summary>
    public bool RespondInteraction { get; init; }

    private DiscordMessage? _message = null!;
    private DiscordInteraction? _interaction;
    private DiscordEmbedBuilder _embedBuilder= null!;
    
    private bool _timedOut = false;
    private bool _skipped = false;
    private bool _finished = false;
    /// <summary>
    /// Whether or not to remove all buttons at the end of dialogue. should be used if the last dialogue text
    /// is the last thing that happens in a command
    /// </summary>
    public bool RemoveButtonsAtEnd { get; init; } 
    private static readonly DiscordButtonComponent Next = new(DiscordButtonStyle.Success, "next", "NEXT");
    private static readonly DiscordButtonComponent Skip = new(DiscordButtonStyle.Success, "skip", "SKIP");


    private async Task HandleArgumentDisplay(string text, bool isLast,
        params DiscordActionRowComponent[] discordActionRows)
    {
    
        if (discordActionRows.Any(i =>
                i.Components.Any(j => j.CustomId == "skip")))
        {

            throw new Exception("No discord component in the provided action rows should have an Id of \"skip\"\n" +
                                "since it is already preserved for another purpose");
        }

        if (discordActionRows.Any(i => i.Components.Any(j => j is not DiscordButtonComponent)))
        {
            throw new Exception("Only buttons are allowed in the action rows");
        }
        var lastActionRow = discordActionRows.LastOrDefault();

        if (lastActionRow is not null && lastActionRow.Components.Count < 5 && Skippable)
        {
            lastActionRow = new DiscordActionRowComponent([..lastActionRow.Components, Skip]);
            discordActionRows[discordActionRows.Length -1] = lastActionRow;
        } else if (Skippable)
        {
            discordActionRows = [..discordActionRows, new DiscordActionRowComponent([Skip])];
        }

        _embedBuilder.WithDescription(text);
        var messageBuilder = new DiscordMessageBuilder()
            .AddEmbed(_embedBuilder)
            .AddComponents(discordActionRows.AsEnumerable());
        
        if(isLast && RemoveButtonsAtEnd && DecisionArgument is null)
        {
            messageBuilder.ClearComponents();
          
        }

        
        if (_interaction is not null && _message is null)
        {
            var responseBuilder = new DiscordInteractionResponseBuilder()
                .AddEmbed(_embedBuilder.Build()).AddComponents(Next, Skip);
      
            if (isLast && RemoveButtonsAtEnd && DecisionArgument is null)
            {
                responseBuilder.ClearComponents();
            }

            await _interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                responseBuilder);

            _message = await _interaction.GetOriginalResponseAsync();
        }
        else if (_message is null && _channel is not null)
        {
            _message = await _channel.SendMessageAsync(messageBuilder); 
        } else if(_lastInteraction is not null)
        {
            await _lastInteraction
                .CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(messageBuilder));
            _message = await _lastInteraction.GetOriginalResponseAsync();
        }
        else if(_message is not null)
        {
            _message = await _message.ModifyAsync(messageBuilder);
        }
        else
        {
            throw new Exception("wtf");
        }

        _lastInteraction = null;
    }

    private DiscordInteraction? _lastInteraction = null;

    private void HandleInteractionResult(InteractivityResult<ComponentInteractionCreatedEventArgs> args)
    {
        
        var answer = args.Result.Id;
        if (answer == "skip")
        {
            _skipped = true;
        }
        if(args.TimedOut)
        {
            _timedOut = true;
            _finished = true;
            return;
        }
        if (_skipped)
        {
            _finished = true;

        }
    }

    public Task<DialogueResult> LoadAsync(DiscordUser user, DiscordInteraction interaction)
    {
        if (interaction is null)
            throw new ArgumentNullException(nameof(interaction));
        return LoadAsync(user,interaction,null,null);
    }
    public Task<DialogueResult> LoadAsync(DiscordUser user, DiscordMessage message)
    {
        if (message is null)
            throw new ArgumentNullException(nameof(message));
        return LoadAsync(user,null,message);
    }
    public Task<DialogueResult> LoadAsync(DiscordUser user, DiscordChannel channel)
    {
        if (channel is null)
            throw new ArgumentNullException(nameof(channel));
        return LoadAsync(user,null,null,channel);
    }
    
    private DiscordChannel? _channel;
    /// <summary>
    /// Initiates the dialogue of a character
    /// </summary>
    /// <param name="context">The context of the interaction</param>
    /// <param name="message">if not null, will edit the message provided with the dialogue</param>
    /// <returns></returns>
    private async Task<DialogueResult> LoadAsync(DiscordUser user,DiscordInteraction? interaction,  DiscordMessage? message = null,
        DiscordChannel? channel = null)
    {
        if (!NormalArguments.Any() && DecisionArgument is null)
        {
            throw new Exception("There is no decision argument provided");
        }



        _message = message!;
        _channel = channel;
        _interaction = interaction;
        var loadedDialogueArguments = NormalArguments.ToArray();
        _embedBuilder = new DiscordEmbedBuilder()
            .WithTitle(Title);

        for(var i = 0; i < loadedDialogueArguments.Length; i++)
        {
            _embedBuilder
                .WithAuthor(loadedDialogueArguments[i].CharacterName, iconUrl: loadedDialogueArguments[i].CharacterUrl)
                .WithColor(loadedDialogueArguments[i].CharacterColor);
            var normalArgument = loadedDialogueArguments[i];
      

            var dialogueTexts = normalArgument.DialogueTexts.ToArray();
            for(var j = 0; j < dialogueTexts.Length;   j++)
            {
                var isLast = i == loadedDialogueArguments.Length
                    - 1 && j == dialogueTexts.Length - 1;
                await HandleArgumentDisplay(dialogueTexts[j], isLast,
                    [new DiscordActionRowComponent([Next])]);

                
                if(isLast && RemoveButtonsAtEnd && DecisionArgument is null) break;
                var result = await _message
                    .WaitForButtonAsync(e => e.User == user);
                _lastInteraction = result.Result.Interaction;
                HandleInteractionResult(result);
                if (isLast && DecisionArgument is null) _finished = true;
                if(_finished) break;

                
            }
            if (_finished && _lastInteraction is not null)
            {
                await _lastInteraction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage);
                break;
            }
    
        }

        string? decision = null;
        if (!_finished && DecisionArgument is not null)
        {
            _embedBuilder
                .WithAuthor(DecisionArgument.CharacterName, iconUrl: DecisionArgument.CharacterUrl)
                .WithColor(DecisionArgument.CharacterColor);
            await HandleArgumentDisplay(DecisionArgument.DialogueText, true,
                DecisionArgument.ActionRows.ToArray());
            var result = await _message.WaitForButtonAsync(e
                => e.User == user);
            _lastInteraction = result.Result.Interaction;
            decision = result.Result.Id;
            var defer = true;

            if (RemoveButtonsAtEnd)
            {
                var messageBuilder = new DiscordMessageBuilder(_message);
                messageBuilder.ClearComponents();
                await _lastInteraction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage,
                    new DiscordInteractionResponseBuilder(messageBuilder));

                defer = false;
            }

            if (defer)
                await _lastInteraction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
            HandleInteractionResult(result);
        }
        return new DialogueResult
        {
            Skipped = _skipped,
            TimedOut = _timedOut,
            Message = _message,
            Decision = decision
        };
    }
}