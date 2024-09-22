using System.ComponentModel;
using System.Text;
using BasicFunctionality;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Entities.LegendaryBot;
using Entities.LegendaryBot.Entities;
using Entities.LegendaryBot.Entities.BattleEntities.Blessings;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Entities.Items;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

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

    public static readonly ShopItem[] ShopItems =
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
    public async ValueTask OpenShopAsync(CommandContext ctx, [Parameter("item-num")] int? itemNumber = null)
    {
        if (itemNumber is null)
        {
            var userDataCol = await DatabaseContext.Set<UserData>()
                                  .Where(i => i.DiscordId == ctx.User.Id)
                                  .Select(i => new DiscordColor?(i.Color))
                                  .FirstOrDefaultAsync() ??
                              TypesFunction.GetDefaultObject<UserData>().Color;
            var embedToBuild = new DiscordEmbedBuilder()
                .WithUser(ctx.User)
                .WithColor(userDataCol)
                .WithTitle("The shop");
            
            var index = 1;
            foreach (var i in ShopItems)
            {
                embedToBuild.AddField($"{index++} • {i.ItemSample.Name} {string.Concat(Enumerable.Repeat("\u2b50", (int)i.ItemSample.Rarity))}",
                   $"Type: {i.ItemSample.TypeGroup.Name}\nPrice: {i.Cost:N0} {i.CurrencyToBuyWith}" );
      
            }

          
            await ctx.RespondAsync(embedToBuild);
            return;
        }
        else
        {
            int[] typeIdsToLookFor = ShopItems.Select(i => i.ItemSample)
                .OfType<Character>().Select(i => i.TypeId).ToArray();
            var userData = await DatabaseContext.Set<UserData>()
                .Include(i => i.Items.Where(j => j is DivineShard || j is Coin))
                .Include(i => 
                    i.Characters.Where(j => typeIdsToLookFor.Contains(j.TypeId)))
                .FirstOrDefaultAsync(i => i.DiscordId == ctx.User.Id);

            if (userData is null)
            {
               await AskToDoBeginAsync(ctx);
               return;
            }
            var embedToBuild = new DiscordEmbedBuilder()
                .WithUser(ctx.User)
                .WithColor(userData.Color)
                .WithTitle("The shop");

            if (itemNumber > ShopItems.Length)
            {
                embedToBuild.WithDescription("Item num out of range");
                await ctx.RespondAsync(embedToBuild);
                return;
            }

            var itemToBuy = ShopItems[itemNumber.Value - 1];
            await ctx.RespondAsync(itemToBuy.ItemSample.Name);

        }
    }
}