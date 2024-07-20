using System.Reflection;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.Stats;
using DiscordBotNet.LegendaryBot.Entities.Items;
using DiscordBotNet.LegendaryBot.Quests;
using Microsoft.EntityFrameworkCore;
using ConfigurationManager = System.Configuration.ConfigurationManager;


namespace DiscordBotNet.Database;



public class PostgreSqlContext : DbContext
{
    
    

    


    private static readonly Type[] EntityClasses;

    public DbSet<UserData> UserData { get; set; }
    public DbSet<GuildData> GuildDatas { get; set; }
    public DbSet<Blessing> Blessings { get; set; }

    public DbSet<Gear> Gears { get; set; }
    public DbSet<Character> Characters { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<Quote> Quote { get; set; }

    /// <summary>
    /// this should be called before any query if u want to ever use this method
    /// </summary>
    /// <param name="userId"> the user id u want  to refresh to a new day</param>
    public  async Task CheckForNewDayAsync(ulong userId)
    {

        var user = await UserData
            .Include(i => i.Quests)
            .FirstOrDefaultAsync(i => i.Id == userId);
        if(user is null)
            return;
        var rightNowUtc = DateTime.UtcNow;
        if (user.LastTimeQuestWasChecked.Date == rightNowUtc.Date) return;
        
        user.Quests.Clear();
        var availableQuests = TypesFunctionality.GetDefaultObjectsThatIsInstanceOf<Quest>()
            .Select(i => i.GetType())
            .Where(i => !i.IsAbstract)
            .OrderBy(_ => BasicFunctionality.GetRandomNumberInBetween(0, 100))
            .Take(4);

        foreach (var i in availableQuests)
        {
            user.Quests.Add((Quest) Activator.CreateInstance(i)!);
        }
   

        
        user.LastTimeQuestWasChecked = DateTime.UtcNow;


    }

    public async Task<UserData> CreateNonExistantUserdataAsync(ulong id)
    {
        var user = new UserData() { Id = id };
        await UserData.AddAsync(user);
        return user;
    }
 

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        
        // Configure the database provider and connection string
        optionsBuilder
            .UseNpgsql(ConfigurationManager.AppSettings["ConnectionString"])

            .EnableSensitiveDataLogging();
    
    }
    public void ResetDatabase()
    {
        Database.EnsureDeleted();
        Database.EnsureCreated();
      
    }
    public async Task ResetDatabaseAsync()
    {
        await Database.EnsureDeletedAsync();
        await Database.EnsureCreatedAsync();
      
    }


    

    private static readonly IEnumerable<Type> QuestTypes =
        Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(i => i.IsSubclassOf(typeof(Quest)) 
                        && !i.IsAbstract).ToArray();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {


        modelBuilder
            .UsePropertyAccessMode(PropertyAccessMode.Property);
        foreach (var i in TypesFunctionality.AllAssemblyTypes.Where(i => i.IsRelatedToType(typeof(Quest))))
        {
            modelBuilder.Entity(i);
        }
        foreach (var entityType in TypesFunctionality
                     .AllAssemblyTypes
                     .Where(i => i.IsClass && i.GetInterfaces().Contains(typeof(IInventoryEntity))))
        {
            modelBuilder.Entity(entityType);
        }

    
        foreach (var i in TypesFunctionality.AllAssemblyTypes.Where(i => i.IsRelatedToType(typeof(GearStat))))
        {
            modelBuilder.Entity(i);
        }


        modelBuilder
            .ApplyConfiguration(new ItemDatabaseConfiguration())
            .ApplyConfiguration(new UserDataDatabaseConfiguration())
            .ApplyConfiguration(new QuoteDatabaseConfiguration())
            .ApplyConfiguration(new PlayerTeamDatabaseConfiguration())
            .ApplyConfiguration(new CharacterDatabaseConfiguration())
            .ApplyConfiguration(new PlayerDatabaseConfiguration())
            .ApplyConfiguration(new GearDatabaseConfiguration())
            .ApplyConfiguration(new GearStatDatabaseConfiguration())
            .ApplyConfiguration(new BlessingDatabaseConfiguration())
            .ApplyConfiguration(new GuildDataDatabaseConfig());






    }
}