using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace BasicFunctionality;

public static class BasicFunctions
{
    public static string MultiplyString(this string toMultiply, int num)
    {
        return string.Concat(Enumerable.Repeat(toMultiply, num));
    }
    public static string CommaConcatenator(IEnumerable<string> values)
    {
        var valuesArray = values.ToArray();
        var length = valuesArray.Length;
        if (length == 0) return "";
        if (length == 1) return valuesArray[0];
        if (length == 2) return $"{valuesArray[0]} and {valuesArray[1]}";
        var resultBuilder = new StringBuilder($"{valuesArray[0]}, {valuesArray[1]}");
        for (var i = 2; i < length - 1; i++) resultBuilder.Append($", {valuesArray[i]}");
        resultBuilder.Append($" and {valuesArray[length - 1]}");
        return resultBuilder.ToString();
    }


    public static async Task DelayWithTokenNoError(int milliseconds, CancellationToken token)
    {
        try
        {
            await Task.Delay(milliseconds, token);
        }
        catch (TaskCanceledException)
        {
        }
    }


    public static FieldInfo[] GetAllFields(Type type)
    {
        var loopingType = type;
        List<FieldInfo> info = [];
        while (loopingType is not null)
        {
            info.AddRange(loopingType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));
            loopingType = loopingType.BaseType;
        }

        return info.Distinct().ToArray();
    }

    /// <summary>
    ///     Gets the size of object.
    /// </summary>
    /// <param name="obj">The object.</param>
    /// <param name="avgStringSize">Average size of the string.</param>
    /// <returns>An approximation of the size of the object in bytes</returns>
    public static int SizeOf(Type type)
    {
        var pointerSize = IntPtr.Size;
        var size = 0;
        var info = GetAllFields(type);
        foreach (var field in info)
            if (field.FieldType.IsValueType)
                try
                {
                    var marshal = Marshal.SizeOf(field.FieldType);
                    size += marshal;
                }
                catch (Exception e)
                {
                    size += SizeOf(field.FieldType);
                }
            else
                size += pointerSize;

        return size;
    }

    public static int GetRandomNumberInBetween(int a, int b)
    {
        return new Random().Next(a, b + 1);
    }

    /// <returns>
    ///     A random element from the parameters
    /// </returns>
    public static T RandomChoice<T>(params T[] elements)
    {
        if (elements.Length <= 0)
            throw new Exception("There is no element in the input");
        Random random = new();
        var index = random.Next(elements.Length);
        return elements[index];
    }

    /// <returns>
    ///     A random element from the list
    /// </returns>
    public static T RandomChoice<T>(IEnumerable<T> elements)
    {
        return RandomChoice(elements.ToArray());
    }

    /// <summary>
    ///     Note: chances MUST add up to 100%. there must be at least one key
    /// </summary>
    public static TValue GetRandom<TValue>(Dictionary<TValue, double> chances) where TValue : notnull
    {
        if (chances.Count <= 0)
            throw new Exception("Dictionary must have at least one key");

        if (chances.Select(i => i.Value).Sum() != 100.0)
            throw new Exception("Sum of dictionary values must be 100");
        var totalWeight = chances.Sum(kv => kv.Value);

        var randomValue = new Random().NextDouble() * totalWeight;
        foreach (var kvp in chances)
        {
            if (randomValue < kvp.Value) return kvp.Key;

            randomValue -= kvp.Value;
        }

        throw new Exception("Unexpected error");
    }


    ///<returns>The amount of time till the next day as a string</returns>
    public static string TimeTillNextDay()
    {
        var now = DateTime.UtcNow;
        int hours = 0, minutes = 0, seconds = 0;
        hours = 24 - now.Hour - 1;
        minutes = 60 - now.Minute - 1;
        seconds = 60 - now.Second - 1;
        return $"{hours}:{minutes}:{seconds}";
    }

    /// <returns>
    ///     True if the percentage chance procs.
    ///     False if it does not proc
    /// </returns>
    /// <param name="number">Percentage chance</param>
    public static bool RandomChance(double number)
    {
        if (number < 0) throw new Exception("Number must be between 0 or 100");
        Random random = new();
        return random.Next(0, 100) <= number;
    }

    /// <returns>
    ///     True if the percentage chance procs.
    ///     False if it does not proc
    /// </returns>
    /// <param name="number">Percentage chance</param>
    public static bool RandomChance(float number)
    {
        return RandomChance((double)number);
    }

    /// <returns>
    ///     True if the percentage chance procs.
    ///     False if it does not proc
    /// </returns>
    /// <param name="number">Percentage chance</param>
    public static bool RandomChance(int number)
    {
        return RandomChance((double)number);
    }
}