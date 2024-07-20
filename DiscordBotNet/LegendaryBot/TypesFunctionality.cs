using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace DiscordBotNet.LegendaryBot;

public static class TypesFunctionality
{

    private static ConcurrentDictionary<Type, object> _defaultObjectsContainer =  new();


    public static readonly ImmutableArray<Type> AllAssemblyTypes;


    static TypesFunctionality()
    {
     
        AllAssemblyTypes = Assembly.GetExecutingAssembly()
            .GetTypes().ToImmutableArray();
    }
    public static  TObjectType GetDefaultObject<TObjectType>()
    {
        return (TObjectType)GetDefaultObject(typeof(TObjectType));
    }


    public static IEnumerable<object> GetDefaultObjectsThatIsInstanceOf(Type type)
    {
        IEnumerable<Type> typesToLoop = null;
        if (type.IsInterface)
        {
            typesToLoop = AllAssemblyTypes.Where(j => !j.IsAbstract && j.GetInterfaces().Contains(type));
        }
        else
        {
            typesToLoop = AllAssemblyTypes.Where(j => !j.IsAbstract && (j.IsSubclassOf(type) || j == type));
        }
        foreach (var i in typesToLoop)
        {
            yield return GetDefaultObject(i);
        }
    }
    public static IEnumerable<TObjectType> GetDefaultObjectsThatIsInstanceOf<TObjectType>()
    {
        return GetDefaultObjectsThatIsInstanceOf(typeof(TObjectType)).Cast<TObjectType>();
    }

    public static object GetDefaultObject(Type type)
    {
  
        if(!_defaultObjectsContainer.ContainsKey(type))
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

        return _defaultObjectsContainer[type];
    }
}