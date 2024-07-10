using System.Collections.Immutable;
using System.Reflection;
using DiscordBotNet.Extensions;

namespace DiscordBotNet.LegendaryBot;

public static class DefaultObjects
{

    private static Dictionary<Type, object> _defaultObjectsContainer =  new();


    public static readonly ImmutableArray<Type> AllAssemblyTypes;


    
    static DefaultObjects()
    {
     
        AllAssemblyTypes = Assembly.GetExecutingAssembly()
            .GetTypes().ToImmutableArray();
    }
    public static  TObjectType GetDefaultObject<TObjectType>()
    {
        return (TObjectType)GetDefaultObject(typeof(TObjectType));
    }


    public static IEnumerable<object> GetDefaultObjectsThatSubclass(Type type)
    {
        foreach (var i in AllAssemblyTypes.Where(j => !j.IsAbstract && 
                                              (j.IsSubclassOf(type) || j == type)))
        {
            yield return GetDefaultObject(i);
        }
    }
    public static IEnumerable<TObjectType> GetDefaultObjectsThatSubclass<TObjectType>()
    {
        return GetDefaultObjectsThatSubclass(typeof(TObjectType)).Cast<TObjectType>();
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