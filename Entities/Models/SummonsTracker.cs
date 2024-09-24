using System.ComponentModel.DataAnnotations;

namespace Entities.Models;

public abstract class SummonsTracker
{
    [Timestamp] public uint Version { get; private set; }
    
    
    public abstract int TypeId { get; protected init; }
    public long UserDataId { get; set; }
    public long Id { get; set; }
    public int FiveStarPity { get; set; }

    public int FourStarPity { get; set; }

}

public class LimitedBlessingSummonsTracker : SummonsTracker
{
    public override int TypeId
    {
        get => 1; protected init{} }
}
public class LimitedCharacterSummonsTracker : SummonsTracker
{
    public override int TypeId
    {
        get => 2; protected init{} }
}