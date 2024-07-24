namespace DiscordBotNet.LegendaryBot;

public struct SubclassOf<TClass> where TClass : class
{
    private Type _type;

    public SubclassOf(Type type)
    {
        // Ensure the type is a subclass of TClass
        if (!typeof(TClass).IsAssignableFrom(type))
        {
            throw new ArgumentException($"{type.FullName} is not a subclass of {typeof(TClass).FullName}");
        }
        _type = type;
    }

    public static explicit operator Type(SubclassOf<TClass> subClass)
    {
        return subClass._type;
    }

    public Type Get()
    {
        return _type;
    }

    public static bool operator ==(SubclassOf<TClass> left, SubclassOf<TClass> right)
    {
        return left._type == right._type;
    }

    public static bool operator !=(SubclassOf<TClass> left, SubclassOf<TClass> right)
    {
        return !(left == right);
    }

    public override bool Equals(object obj)
    {
        if (obj is SubclassOf<TClass> other)
        {
            return _type == other._type;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return _type.GetHashCode();
    }
}