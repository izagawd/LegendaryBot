namespace Entities.LegendaryBot.BattleSimulatorStuff;

/// <summary>
///     Use it on any method that is involved in a battle, and it will listen for events
///     Note: doesnt work on methods in moves. use in character instead
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class BattleEventListenerMethodAttribute : Attribute
{
    public BattleEventListenerMethodAttribute(int priority = 1)
    {
        Priority = priority;
    }

    /// <summary>
    ///     The higher the priority, the more likely this method is called first before any other event listener
    ///     is called
    /// </summary>
    public int Priority { get; }
}