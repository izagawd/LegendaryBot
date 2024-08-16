namespace DiscordBotNet.LegendaryBot.Entities.Items;

public abstract class Energy : Item
{
    public abstract TimeSpan EnergyIncreaseInterval { get; }
    public DateTime LastTimeUpdated { get; private set; } = DateTime.UtcNow.AddDays(-1);
    public abstract int MaxEnergyValue { get; }
    public void RefreshEnergyValue()
    {
        if (Stacks >= MaxEnergyValue)
        {
            LastTimeUpdated = DateTime.UtcNow;
            return;
        }
        // Calculate the total elapsed time in minutes
        var elapsedTime = DateTime.UtcNow - LastTimeUpdated;
        

        // Determine how much energy has accumulated based on the rate of 1 energy every 6 minutes
        var newEnergy = (int)(elapsedTime / EnergyIncreaseInterval);
      
        // Update the Value with the new energy amount
        Stacks += newEnergy;
        if (Stacks >= MaxEnergyValue)
            Stacks = MaxEnergyValue;
        // Update LastTimeAccessed to the current time, minus the remaining minutes that didn't add up to a full energy point
        LastTimeUpdated = DateTime.UtcNow.AddSeconds(-(elapsedTime.TotalSeconds % EnergyIncreaseInterval.TotalSeconds));
    }
}