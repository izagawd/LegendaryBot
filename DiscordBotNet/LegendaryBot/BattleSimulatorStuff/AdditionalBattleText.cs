namespace DiscordBotNet.LegendaryBot.BattleSimulatorStuff;

public class AdditionalBattleText
{
 
    public static implicit operator AdditionalBattleText(string text) => new(text);
    public virtual string Text { get; } 

    public AdditionalBattleText(string text)
    {
        Text = text;
    }

    public static IEnumerable<AdditionalBattleText> Combine(IEnumerable<AdditionalBattleText> battleTexts)
    {
        var asArray = battleTexts.ToArray();
        List<AdditionalBattleText> toReturnList = [];
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
    /// This comes before the argument in the order
    /// </summary>

    public virtual AdditionalBattleText? Merge(AdditionalBattleText additionalBattleTextInstance)
    {
        return null;
    }

    protected AdditionalBattleText(){}
}



