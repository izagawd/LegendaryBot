using System.Collections;
using BasicFunctionality;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Entities.LegendaryBot;
using Entities.LegendaryBot.Entities.BattleEntities.Gears;
using Entities.LegendaryBot.Entities.Items;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Commands;

public class GearCraft : GeneralCommandClass
{
    public struct MetalCost
    {
        public Rarity Rarity;

        public Type MetalType => GetRequiredMetalType(Rarity);
        public int MetalStacks;
        public int CoinsCost => MetalStacks * 5 * ((int) Rarity);
    }

    public static Type GetRequiredMetalType(Rarity gearRarity)
    {
        switch (gearRarity)
        {
            case Rarity.OneStar:
                return typeof(UncommonMetal);
            case Rarity.TwoStar:
                return typeof(CommonMetal);
            case Rarity.ThreeStar:
                return typeof(RareMetal);
            case Rarity.FourStar:
                return typeof(EpicMetal);
            case Rarity.FiveStar:
                return typeof(DivineMetal);
            default:
                throw new ArgumentOutOfRangeException(nameof(gearRarity), gearRarity, null);
        }
    }
    public MetalCost GetMetalCost(Type gearType, Rarity gearRarity)
    {
        Type[] normals = [typeof(Armor), typeof(Helmet), typeof(Weapon)];
        Type[] advanced = [typeof(Ring), typeof(Necklace), typeof(Boots)];
        if (normals.Contains(gearType))
        {
            return new MetalCost() { MetalStacks = 10, Rarity =gearRarity };
        }

        if (advanced.Contains(gearType))
        {
            return new MetalCost() { MetalStacks = 15, Rarity = gearRarity };
        }

        throw new Exception();
    }
    [Command("craft-gear")]
    public async ValueTask CraftGearAsync(CommandContext commandContext)
    {
        var userData = await DatabaseContext.Set<UserData>()
            .Include(i => i.Items.Where(j => j is Metal))
            .FirstOrDefaultAsync(i => i.DiscordId == commandContext.User.Id);
        if (userData is null || userData.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(commandContext);
            return;
        }

        if (userData.IsOccupied)
        {
            await NotifyAboutOccupiedAsync(commandContext);
            return;
        }

        DiscordButtonComponent CreateButton<TGear>() where TGear : Gear
        {
            var name = TypesFunction.GetDefaultObject<TGear>().Name;
            return new DiscordButtonComponent(DiscordButtonStyle.Success,
                name.ToLower(), name.ToUpper());
        }
        DiscordComponent[] firstRow = [
            CreateButton<Weapon>(),
            CreateButton<Helmet>(),
            CreateButton<Armor>(),
            
        ];
        DiscordComponent[] secondRow = [
            CreateButton<Necklace>(),
            CreateButton<Ring>(),
            CreateButton<Boots>()
        ];
        Rarity? rarityToUse = null;
        DiscordMessage message = null;
        DiscordInteraction? lastInteraction = null;
        while (true)
        {
                    var embed = new DiscordEmbedBuilder()
                        .WithUser(commandContext.User)
                        .WithColor(userData.Color)
                        .WithTitle($"Gear crafting")
                        .WithDescription(GenerateMetalStrings());

                    if (rarityToUse is null)
                    {
                        foreach (var i in firstRow.Union(secondRow)
                                     .OfType<DiscordButtonComponent>())
                        {
                            i.Disable();
                        }
                    }
                    else
                    {
                        foreach (var i in firstRow.Union(secondRow)
                                     .OfType<DiscordButtonComponent>())
                        {
                            i.Enable();
                        }
                    }
  
                    string GenerateMetalStrings()
                    {
                        var toUse = "";
                        foreach (var i in TypesFunction.GetDefaultObjectsAndSubclasses<Metal>())
                        {
                            toUse += $"{i.Name}: {userData.Items.GetItemStacks(i.GetType())}\n";
                        }
            
                        return toUse;
                    }
            
                    var select = new DiscordSelectComponent("rarity-select", "SELECT-RARITY",
                        Enum.GetValues<Rarity>()
                            .Select(i => 
                                new DiscordSelectComponentOption("\u2b50".MultiplyString((int)i),
                                    ((int)i).ToString(),null,
                                    isDefault: rarityToUse ==i))
                            .ToArray());
                    var messageBuilder = new DiscordMessageBuilder()
                        .AddEmbed(embed)
                        .AddComponents(firstRow)
                        .AddComponents(secondRow)
                        .AddComponents(select);
    
                    if (lastInteraction is null || message is null)
                    {
                        if (message is not null)
                        {
                            await message.ModifyAsync(messageBuilder);
                        }
                        else
                        {
                            await commandContext.RespondAsync(messageBuilder);
                            message = (await commandContext.GetResponseAsync())!;
                        }
                    
                    }
                    else
                    {
                     
                        await lastInteraction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage,
                            new DiscordInteractionResponseBuilder(messageBuilder));
                    }
                    
                    var buttonTask = message.WaitForButtonAsync(commandContext.User);
                    var selectTask = message.WaitForSelectAsync(commandContext.User, select.CustomId);
                    IEnumerable<Task<InteractivityResult<ComponentInteractionCreatedEventArgs>>> toWaitFor=
                        [buttonTask, 
                            selectTask];
                    var gottenTask = await Task.WhenAny(toWaitFor);
                    var gotten = await gottenTask;
                    lastInteraction = gotten.Result.Interaction;
                    if (gotten.TimedOut)
                    {
                        foreach (var i in new[]{firstRow,secondRow}
                                     .SelectMany(i => i)
                                     .OfType<DiscordButtonComponent>())
                        {
                            i.Disable();
                        }
                        await message.ModifyAsync(messageBuilder);
                        return;
                    }

                    if (gottenTask == selectTask)
                    {
                        rarityToUse =(Rarity) int.Parse(gotten.Result.Interaction.Data.Values[0]);
                    } else if (gottenTask == buttonTask)
                    {
                        var desiredGearType = TypesFunction.GetDefaultObjectsAndSubclasses<Gear>()
                            .First(i =>
                                i.Name
                                    .Equals(gotten.Result.Id, StringComparison.CurrentCultureIgnoreCase)).GetType();

                        var cost = GetMetalCost(desiredGearType, rarityToUse!.Value);
                        var coins = userData.Items.GetOrCreateItem<Coin>();

                        var metalToUse = (Metal)userData.Items.GetOrCreateItem(cost.MetalType);
                        if (metalToUse.Stacks < cost.MetalStacks || coins.Stacks < cost.CoinsCost)
                        {
                     
                        }
                    }
                    else
                    {
                        return;
                    }
        }

    }
}