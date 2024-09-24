using System.ComponentModel.DataAnnotations;

namespace Entities.Models;

public class WishDetails
{
    [Timestamp] public uint Version { get; private set; }
    public long UserDataId { get; set; }
    public int FiveStarCharacterLimitedPity { get; set; }
    public int FiveStarCharacterStandardPity { get; set; }
    public int FourStarCharacterLimitedPity { get; set; }
    public int FourStarCharacterStandardPity { get; set; }
    public int FourStarBlessingLimitedPity { get; set; }
    public int FourStarBlessingStandardPity { get; set; }
    public int FiveStarBlessingLimitedPity { get; set; }
    public int FiveStarBlessingStandardPity { get; set; }
}