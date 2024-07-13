using System.Linq.Expressions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

namespace DiscordBotNet.Extensions;

public static class BasicExtension
{
    public static string Join(this IEnumerable<string> enumerable, string seperator)
    {
        return string.Join(seperator, enumerable);
    }
   
    public static Color ToImageSharpColor(this DiscordColor color)
    {
        return Color.ParseHex(color.ToString());
    }

    public static Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForAnyComponentInteractionAsync(this DiscordMessage message, Func<ComponentInteractionCreateEventArgs,bool> predicate,
        TimeSpan? timeoutOverride = null)
    {

        return Bot.Interactivity.WaitForEventArgsAsync<ComponentInteractionCreateEventArgs>
            (i => i.Message == message && predicate(i),timeoutOverride);
    }
    public static async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForAnyComponentInteractionAsync(this DiscordMessage message, Func<ComponentInteractionCreateEventArgs,bool> predicate, CancellationTokenSource token)
    {
        IEnumerable<Task<InteractivityResult<ComponentInteractionCreateEventArgs>>> tasks=
        [
            message.WaitForButtonAsync(predicate, token.Token),
            message.WaitForSelectAsync(predicate, token.Token)
        ];
        var result = await await Task.WhenAny(tasks);
        await token.CancelAsync();
        return result;
    }
    public static DiscordEmbedBuilder WithUser(this DiscordEmbedBuilder embedBuilder, DiscordUser user)
    {
        return embedBuilder.WithAuthor(user.Username, iconUrl: user.AvatarUrl);
    }
    public static string Represent<T>(this IEnumerable<T> it,bool print = false)
    {
        var a =  "{ " + String.Join(", "
            , it.Select(i => i?.ToString())) + " }";
        if (print)
        {
            Console.WriteLine(a);
        }

        return a;
        
    }

    public static bool IsRelatedToType(this Type theType, Type type)
    {
        return theType.IsSubclassOf(type) || theType == type;
    }
    public static int Round(this double theDouble)
    {
        return (int) Math.Round(theDouble);
  
    }
    public static long RoundLong(this float theDouble)
    {
        return (long) Math.Round(theDouble);
  
    }
    public static long RoundLong(this double theDouble)
    {
        return (long) Math.Round(theDouble);
  
    }
    public static int Round(this float theFloat)
    {
        
        return (int) Math.Round(theFloat);
    }
    public static T Print<T>(this T idk)
    {
        Console.WriteLine(idk);
   
        return idk;
    }

    public static TExpressionResult Map<TObject, TExpressionResult>(this TObject theObject, Expression<Func<TObject, TExpressionResult>> expression)
    {
        return expression.Compile().Invoke(theObject);
    }



}