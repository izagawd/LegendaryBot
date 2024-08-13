using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Numerics;
using System.Reflection;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot;


[AttributeUsage(AttributeTargets.Method)]
/**
 * Will only work on static methods
 */
public class DefaultObjectRegistererAttribute : Attribute
{
    
}
public static class TypesFunction
{


    private static ConcurrentDictionary<Type, object> _defaultObjectsContainer =  new();





    public static ConcurrentDictionary<Assembly, Type[]> _allAssemblyTypes = new();

    private static void RegisterDefaultObjects(Assembly assembly)
    {
        List<MethodInfo> failedMethods = [];
        foreach (var i in assembly.GetTypes())
        {
            foreach (var j in i.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                         .Where(j => j.GetCustomAttribute<DefaultObjectRegistererAttribute>() is not null))
            {
                if (j.ReturnType != typeof(IEnumerable<object>) || j.GetParameters().Length > 0 || !j.IsStatic)
                {
                    failedMethods.Add(j);
                    continue;
                }
                if(failedMethods.Any()) continue;
                IEnumerable<object> theObjects = (IEnumerable<object>) j.Invoke(null, null)!;
                foreach (var k in theObjects)
                {
                    _defaultObjectsContainer[k.GetType()] = k;
                }
            }
        }

        if (failedMethods.Any())
        {
            var zaString =
                $"Methods should be static, have no parameter, and should return {typeof(IEnumerable<object>)}. objects in the enumerable will be registered as the default of their type.\n" +
                $"these are the following methods that did not follow the rules: ";
            foreach (var i in failedMethods)
            {
                zaString += $"\n{i.Name} from class {i.DeclaringType}";
            }
            throw new Exception(zaString);
        }
    }
    private static void AddAllAssemblyTypes(Assembly assembly)
    {
        _allAssemblyTypes[assembly] = assembly.GetTypes();
    }
    private static void RegisterAssemblies(IEnumerable<Assembly> assemblies)
    {
        var arrayed = assemblies.ToArray();
        foreach (var i in arrayed)
        {
            AddAllAssemblyTypes(i);
        }
        foreach (var i in arrayed)
        {
            RegisterDefaultObjects(i);
        }

    }
    
    private static ConcurrentDictionary<Type, List<Type>> _subClassesCache = [];

    public static IEnumerable<Type> AllTypes
    {
        get
        {
            foreach (var i in _allAssemblyTypes)
            {
                foreach (var j in i.Value)
                {
                    yield return j;
                }
            }
        }
    }

    static TypesFunction()
    {
     
        AppDomain.CurrentDomain.AssemblyLoad += (_, args) =>
        {
            _subClassesCache.Clear();
           RegisterAssemblies([args.LoadedAssembly]);
        };
       RegisterAssemblies(AppDomain.CurrentDomain.GetAssemblies());
    }
    public static  TObjectType GetDefaultObject<TObjectType>()
    {
        return (TObjectType)GetDefaultObject(typeof(TObjectType));
    }


    private static List<Type> GetTypesThatSubclassesList(Type type)
    {
        if (!_subClassesCache.TryGetValue(type, out var zaList))
        {
            zaList = [];
            zaList.AddRange(AllTypes.Where(i=>i.IsAssignableTo(type)));
            _subClassesCache[type] = zaList;
        }
        return zaList;
    }
    public static IEnumerable<Type> GetTypesThatSubclasses(Type type)
    {
        foreach (var i in GetTypesThatSubclassesList(type))
        {
            yield return i;
        }
    }

    public static IEnumerable<Type> GetCachedAssemblyTypes(Assembly assembly)
    {
        if (!_allAssemblyTypes.TryGetValue(assembly, out var types))
        {
            types = assembly.GetTypes();
            _allAssemblyTypes[assembly] = types;
        }

        foreach (var i in types)
        {
            yield return i;
        }
    }
    public static IEnumerable<object> GetDefaultObjectsAndSubclasses(Type type)
    {

        foreach (var i in GetTypesThatSubclassesList(type))
        {
            if(!i.IsAbstract)
                yield return GetDefaultObject(i);
        }
   
    }
    public static IEnumerable<TObjectType> GetDefaultObjectsAndSubclasses<TObjectType>()
    {
        return GetDefaultObjectsAndSubclasses(typeof(TObjectType))
            .Cast<TObjectType>();
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