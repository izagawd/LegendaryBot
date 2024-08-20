using System.Reflection;
using System.Text;
using Entities.LegendaryBot.BattleEvents.EventArgs;

namespace Entities.LegendaryBot.BattleSimulatorStuff;

public partial class BattleSimulator
{
    /// <summary>
    ///     Compiles possible battle event methods and caches them, so calling them will not take much performance cost
    /// </summary>
    private static void SetupBattleEventDelegatorStuff()
    {
        List<MethodInfo> invalidMethods = [];
        foreach (var i in typeof(BattleSimulator).Assembly.GetTypes().Where(j => !j.IsAbstract && !j.IsInterface))
        foreach (var j in i.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                     .Select(j => new
                         { method = j, attribute = j.GetCustomAttribute<BattleEventListenerMethodAttribute>() })
                     .Where(j => j.attribute is not null))
        {
            if (j.method.ReturnType != typeof(void))
            {
                invalidMethods.Add(j.method);
                continue;
            }

            var parameters = j.method.GetParameters();


            if (parameters.Length != 1)
            {
                invalidMethods.Add(j.method);
                continue;
            }

            var parameter = parameters[0];
            if (!parameter.ParameterType.IsAssignableTo(typeof(BattleEventArgs)))
            {
                invalidMethods.Add(j.method);
                continue;
            }

            var cache = new EventMethodDetails(CreateBattleEventMethodFrom(j.method), j.attribute!,
                parameter.ParameterType);
            if (_methodsCache.TryGetValue(i, out var list))
                list.Add(cache);
            else
                _methodsCache[i] = [cache];
        }

        var stringBuilder = new StringBuilder();
        if (invalidMethods.Count > 0)
        {
            stringBuilder.Append(
                $"The following methods should  return nothing, need to have one parameter, " +
                $"and that one parameter should be a type or subtype of {typeof(BattleEventArgs)},\n"
                + $"since it uses the {nameof(BattleEventListenerMethodAttribute)} to listen to events:\n");

            foreach (var i in invalidMethods)
                stringBuilder.Append($"Method \"{i.Name}\" from class \"{i.DeclaringType}\"");
            throw new Exception(stringBuilder.ToString());
        }
    }

    public static BattleEventMethod CreateBattleEventMethodFrom(MethodInfo methodInfo)
    {
        if (methodInfo.DeclaringType is null)
            throw new NullReferenceException("declarinng type Shouldnt be null");
        if (methodInfo.ReturnType != typeof(void)) throw new Exception("Method must return nothing");
        var parameters = methodInfo.GetParameters();

        if (parameters.Length != 1) throw new Exception("Params length must be 1");
        var param = parameters[0];
        if (!param.ParameterType.IsAssignableTo(typeof(BattleEventArgs)))
            throw new Exception($"Parameter must be of instance {nameof(BattleEventArgs)}");
        var parameterType = param.ParameterType;

        var genericType = typeof(BattleEventMethod<,>).MakeGenericType(methodInfo.DeclaringType, parameterType);

        var actionType = typeof(Action<,>).MakeGenericType(methodInfo.DeclaringType, parameterType);
        var createdAction = Delegate.CreateDelegate(actionType, methodInfo);
        var createdInstance = (BattleEventMethod)Activator.CreateInstance(genericType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, [createdAction],
            null, null)!;


        return createdInstance;
    }

    public abstract class BattleEventMethod
    {
        public abstract void Invoke(object listener, BattleEventArgs args);
    }

    public sealed class BattleEventMethod<TInvokerType, TBattleEventArgs> : BattleEventMethod
        where TBattleEventArgs : BattleEventArgs
    {
        private Action<TInvokerType, TBattleEventArgs> _action = null!;


        private BattleEventMethod(Action<TInvokerType, TBattleEventArgs> action)
        {
            _action = action;
            if (_action is null) throw new ArgumentNullException();
        }

        public static string ActionName => nameof(_action);

        public override void Invoke(object listener, BattleEventArgs args)
        {
            if (args is TBattleEventArgs asMyArgs && listener is TInvokerType invokerType)
                _action(invokerType, asMyArgs);
        }
    }
}