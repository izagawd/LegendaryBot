using DSharpPlus.Entities;

namespace DiscordBotNet.Extensions;

public static class BasicExtension
{
    public static int IndexOf<T>(this IEnumerable<T> enumerable, T target)
    {
        int index = 0;
        if (target is null)
        {
            foreach (var item in enumerable)
            {
                if (item is null)
                    return index;
                if(item.Equals(target))
                    return index;
                index++;
            }
        }
        else
        {
            foreach (var item in enumerable)
            {
                if (target.Equals(item))
                    return index;
                index++;
            }
        }
        return -1; // Item not found
    }
    public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
    {
        foreach (T item in enumeration)
        {
            action(item);
        }
    }


    public static Color ToImageSharpColor(this DiscordColor color)
    {
        return Color.ParseHex(color.ToString());
    }

    public static void CancelIfNotDisposed(this CancellationTokenSource cancellationTokenSource)
    {
        try
        {
            cancellationTokenSource.Cancel();
        }
        catch (ObjectDisposedException)
        {
            
        }
    }
    public static Task CancelIfNotDisposedAsync(this CancellationTokenSource cancellationTokenSource)
    {
        try
        {
            return cancellationTokenSource.CancelAsync();
        }
        catch (ObjectDisposedException)
        {
            return Task.CompletedTask;
            ;
        }
    }



    public static DiscordEmbedBuilder WithUser(this DiscordEmbedBuilder embedBuilder, DiscordUser user)
    {
        return embedBuilder.WithAuthor(user.Username, iconUrl: user.AvatarUrl);
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

    public static TExpressionResult Map<TObject, TExpressionResult>(this TObject theObject,Func<TObject, TExpressionResult> expression)
    {
        return expression(theObject);
    }



}