using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DiscordBotNet.LegendaryBot.Converters;

public class GuidArgumentConverter : ITextArgumentConverter<Guid>, ISlashArgumentConverter<Guid>
{

    public Task<Optional<Guid>> ConvertAsync(TextConverterContext context, MessageCreatedEventArgs eventArgs) => ConvertAsync(context.Argument);

    public Task<Optional<Guid>> ConvertAsync(InteractionConverterContext context, InteractionCreatedEventArgs eventArgs) => ConvertAsync(context.Argument?.RawValue!);

    public Task<Optional<Guid>> ConvertAsync(string value)
    {
         
        if (Guid.TryParse(value, out var guid))
        {
            
            return Task.FromResult(Optional.FromValue(guid));
        }


        return Task.FromResult(Optional.FromValue(Guid.Empty));

    }

    public string ReadableName => "Guid";
    public bool RequiresText => true;
    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.String;
}