using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;

namespace DiscordBotNet.LegendaryBot;

public static class TypesFunctionality
{

    private static ConcurrentDictionary<Type, object> _defaultObjectsContainer =  new();


    public static readonly ImmutableArray<Type> AllAssemblyTypes;


    static TypesFunctionality()
    {
     
        AllAssemblyTypes = typeof(TypesFunctionality).Assembly
            .GetTypes().ToImmutableArray();
    }
    public static  TObjectType GetDefaultObject<TObjectType>()
    {
        return (TObjectType)GetDefaultObject(typeof(TObjectType));
    }


    public static IEnumerable<object> GetDefaultObjectsAndSubclasses(Type type)
    {
        IEnumerable<Type> typesToLoop = null;

        foreach (var i in AllAssemblyTypes.Where(i=>i.IsAssignableTo(type) && !i.IsAbstract))
        {
            yield return GetDefaultObject(i);
        }
    }
    public static IEnumerable<TObjectType> GetDefaultObjectsAndSubclasses<TObjectType>()
    {
        return GetDefaultObjectsAndSubclasses(typeof(TObjectType)).Cast<TObjectType>();
    }

    public static object GetDefaultObject(Type type)
    {
  
        if(!_defaultObjectsContainer.TryGetValue(type, out var o))
        {
            if (type.IsAbstract) throw new Exception($"Cannot get or create abstract class {type}");
            var createdObject  = Activator.CreateInstance(type, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null,[],null);
            if (createdObject is null)
            {
                throw new Exception("Something went wrong");
            }
            _defaultObjectsContainer[type] = createdObject;
            return createdObject;
        }

        return o;
    }
}