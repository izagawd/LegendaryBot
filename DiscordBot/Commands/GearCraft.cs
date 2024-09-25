using System.Collections;
using System.ComponentModel;
using BasicFunctionality;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Entities.LegendaryBot;
using Entities.LegendaryBot.Entities.BattleEntities.Gears;
using Entities.LegendaryBot.Entities.Items;
using Entities.LegendaryBot.Rewards;
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
        public int CoinsCost => MetalStacks * 1300 * ((int) Rarity);
    }

    public static Type GetRequiredMetalType(Rarity gearRarity)
    {
        switch (gearRarity)
        {
            case Rarity.OneStar:
                return typeof(CommonMetal);
            case Rarity.TwoStar:
                return typeof(UncommonMetal);
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
    [BotCommandCategory(BotCommandCategory.Battle)]
    [Description("use this command to craft gear for your characters!")]
    public async ValueTask CraftGearAsync(CommandContext commandContext)
    {
        var userData = await DatabaseContext.Set<UserData>()
            .Include(i => i.Items.Where(j => j is Metal || j is Coin))
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
        var cancelButton = new DiscordButtonComponent(DiscordButtonStyle.Danger, "cancel", "CANCEL");
        Rarity? rarityToUse = null;
        DiscordMessage message = null;
        DiscordInteraction? lastInteraction = null;
        string additionalString = "";
        await MakeOccupiedAsync(userData);
        while (true)
        {

            var embed = new DiscordEmbedBuilder()
                .WithUser(commandContext.User)
                .WithColor(userData.Color)
                .WithTitle($"Gear crafting")
                .WithDescription($"Coins: {userData.Items.GetOrCreateItem<Coin>().Stacks:N0}\n"
                                 + GenerateMetalStrings() + $"\n{additionalString}");


                    if (rarityToUse is not null)
                    {
                        embed.WithFooter($"Cost of crafting is {new MetalCost()
                        {
                            Rarity = rarityToUse.Value,
                            MetalStacks = 10

                        }.CoinsCost:N0} - {new MetalCost()
                    {
                        Rarity = rarityToUse.Value,
                        MetalStacks = 15

                    }.CoinsCost:N0} coins");
                    }
                      
                            
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
                            toUse += $"{i.Name}: {userData.Items.GetItemStacks(i.GetType()):N0}\n";
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
                        .AddComponents([..secondRow,cancelButton])
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

                    using var zaToken = new CancellationTokenSource(TimeSpan.FromMinutes(2));
                    var buttonTask = message.WaitForButtonAsync(commandContext.User,zaToken.Token);
                    var selectTask = message.WaitForSelectAsync(commandContext.User, select.CustomId,zaToken.Token);
                    IEnumerable<Task<InteractivityResult<ComponentInteractionCreatedEventArgs>>> toWaitFor=
                        [buttonTask, 
                            selectTask];
                    var gottenTask = await Task.WhenAny(toWaitFor);
                    await zaToken.CancelAsync();
                    var gotten = await gottenTask;
                    lastInteraction = gotten.Result.Interaction;
                    if (gotten.TimedOut)
                    {
                        foreach (var i in new[]{firstRow,secondRow}
                                     .SelectMany(i => i)
                                     .Append(cancelButton)
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

                        var buttonId = gotten.Result.Id;
                        if (buttonId == "cancel")
                        {
                            foreach (var i in new[]{firstRow,secondRow}
                                         .SelectMany(i => i)
                                         .Append(cancelButton)
                                         .OfType<DiscordButtonComponent>())
                            {
                                i.Disable();
                            }
                            await lastInteraction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage,
                                new DiscordInteractionResponseBuilder(messageBuilder));
                            
                            return;
                        }
                        additionalString = "";
                        var desiredGearType = TypesFunction.GetDefaultObjectsAndSubclasses<Gear>()
                            .First(i =>
                                i.Name
                                    .Equals(buttonId, StringComparison.CurrentCultureIgnoreCase)).GetType();

                        var cost = GetMetalCost(desiredGearType, rarityToUse!.Value);
                        var coins = userData.Items.GetOrCreateItem<Coin>();

                        var metalToUse = (Metal)userData.Items.GetOrCreateItem(cost.MetalType);
                        if (metalToUse.Stacks < cost.MetalStacks || coins.Stacks < cost.CoinsCost)
                        {

                            await lastInteraction.CreateResponseAsync(
                                DiscordInteractionResponseType.ChannelMessageWithSource,
                                new DiscordInteractionResponseBuilder()
                                    .AsEphemeral()
                                    .AddEmbed(new DiscordEmbedBuilder()
                                        .WithColor(userData.Color)
                                        .WithUser(commandContext.User)
                                        .WithTitle("Hmm")
                                        .WithDescription(
                                            $"You need {cost.MetalStacks:N0} {metalToUse.Name}s and {cost.CoinsCost:N0} coins to craft a " +
                                            $"{(int)rarityToUse} star" +
                                            $" {((Gear)TypesFunction.GetDefaultObject(desiredGearType)).Name}")));
                            lastInteraction = null;
                        }
                        else
                        {
                       
                            coins.Stacks -= cost.CoinsCost;
                            metalToUse.Stacks -= cost.MetalStacks;
                            var created =(Gear) Activator.CreateInstance(desiredGearType)!;
                            created.Initialize(rarityToUse.Value);
                            additionalString = await userData.ReceiveRewardsAsync(DatabaseContext.Set<UserData>(),
                                [new EntityReward([created])]);
                            await DatabaseContext.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        return;
                    }
        }

    }
}