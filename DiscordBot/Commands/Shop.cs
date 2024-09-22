using System.ComponentModel;
using DSharpPlus.Commands;
using Entities.LegendaryBot;
using Entities.LegendaryBot.Entities;
using Entities.LegendaryBot.Entities.BattleEntities.Blessings;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

namespace DiscordBot.Commands;

public enum CurrencyToBuyWith
{
    Coin, DivineShard
}
public class ShopItem
{
    public const int FiveStarCharacterCost = 30000;
    public const int FourStarCharacterCost = 1000;  
    public const int ThreeStarCharacterCost = 100000;
    public const int FourStarBlessingCost = 800;
    public const int FiveStarBlessingCost = 20000;
    public const int ThreeStarBlessingCost = 80000;
    public ShopItem(IInventoryEntity itemSample)
    {
        ItemSample = itemSample;
        if (itemSample is Character character)
        {
            switch (character.Rarity)
            {
                case Rarity.ThreeStar:
                    CurrencyToBuyWith = CurrencyToBuyWith.Coin;
                    Cost = ThreeStarCharacterCost;
                    break;
                case Rarity.FourStar:
                    CurrencyToBuyWith = CurrencyToBuyWith.DivineShard;
                    Cost = FourStarCharacterCost;
                    break;
                case Rarity.FiveStar:
                    CurrencyToBuyWith = CurrencyToBuyWith.DivineShard;
                    Cost = FiveStarCharacterCost;
                    break;
            }
        } else if (itemSample is Blessing blessing)
        {
            switch (blessing.Rarity)
            {
                case Rarity.ThreeStar:
                    CurrencyToBuyWith = CurrencyToBuyWith.Coin;
                    Cost = ThreeStarBlessingCost;
                    break;
                case Rarity.FourStar:
                    CurrencyToBuyWith = CurrencyToBuyWith.DivineShard;
                    Cost = FourStarBlessingCost;
                    break;
                case Rarity.FiveStar:
                    CurrencyToBuyWith = CurrencyToBuyWith.DivineShard;
                    Cost = FiveStarBlessingCost;
                    break;
            }
        }
        else
        {
            throw new Exception("Only blessing and character can be sold");
        }

    }
    public IInventoryEntity ItemSample { get; }
    public int Cost { get; }
    public CurrencyToBuyWith CurrencyToBuyWith { get; }
}

public class ShopCommand : GeneralCommandClass
{

    public static ShopItem[] ShopItems =
    [
        new ShopItem(new CommanderJean()),
        new ShopItem(new PowerOfThePhoenix()),
        new ShopItem(new Blast()),
        new ShopItem(new Roxy()),
        new ShopItem(new Slasher()),
        new ShopItem(new GoingAllOut()),
        new ShopItem(new RoyalKnight()),
        new ShopItem(new Takeshi())
    ];
  
    [Command("shop")]
    [Description("Shop characters or items")]
    [BotCommandCategory(BotCommandCategory.Battle)]
    public async ValueTask OpenShopAsync([Parameter("item-to-buy")] int? itemToBuy)
    {
        
    }
}