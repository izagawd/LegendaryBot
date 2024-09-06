namespace Entities.LegendaryBot.BattleSimulatorStuff;

public class BattleText
{
    public BattleText(string text)
    {
        Text = text;
    }

    protected BattleText()
    {
    }

    public virtual string Text { get; } = null!;

    public static implicit operator BattleText(string text)
    {
        return new BattleText(text);
    }

    public static IEnumerable<BattleText> Combine(IEnumerable<BattleText> battleTexts)
    {
        var asArray = battleTexts.ToArray();
        List<BattleText> toReturnList = [];
        if (asArray.Length == 0) return [];
        var lastBattleTextInstance = asArray[0];
        for (var i = 1; i < asArray.Length; i++)
        {
            var current = asArray[i];
            var possibleMerged = lastBattleTextInstance.Merge(current);
            if (possibleMerged is null)
            {
                toReturnList.Add(lastBattleTextInstance);
                lastBattleTextInstance = current;
            }
            else
            {
                lastBattleTextInstance = possibleMerged;
            }
        }

        toReturnList.Add(lastBattleTextInstance);
        return toReturnList;
    }

    /// <summary>
    ///     This comes before the argument in the order
    /// </summary>
    public virtual BattleText? Merge(BattleText battleTextInstance)
    {
        return null;
    }
}