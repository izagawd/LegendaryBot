using System.Reflection;
using DiscordBotNet.Database.ManyToManyInstances;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;
using DiscordBotNet.LegendaryBot.Quests;

using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using ConfigurationManager = System.Configuration.ConfigurationManager;


namespace DiscordBotNet.Database;

public class PostgreSqlContext : DbContext
{
    
    

    


    private static readonly Type[] EntityClasses;

    public DbSet<UserData> UserData { get; set; }
    public DbSet<GuildData> GuildData { get; set; }
    public DbSet<Entity> Entity { get; set; }
    public DbSet<Quest> Quests { get; set; }

    public DbSet<Quote> Quote { get; set; }

    /// <summary>
    /// this should be called before any query if u want to ever use this method
    /// </summary>
    /// <param name="userId"> the user id u want  to refresh to a new day</param>
    public  async Task CheckForNewDayAsync(long userId)
    {

        var user = await UserData
            .Include(i => i.Quests)
            .FindOrCreateUserDataAsync(userId);
        var rightNowUtc = DateTime.UtcNow;
        if (user.LastTimeChecked.Date == rightNowUtc.Date) return;
        
        user.Quests.Clear();
        var availableQuests = DefaultObjects.GetDefaultObjectsThatSubclass<Quest>()
            .Where(i => i.QuestTier == user.Tier)
            .Select(i => i.GetType())
            .ToList();

        while (user.Quests.Count < 4)
        {
            if(availableQuests.Count <= 0) break;
            var randomQuestType = BasicFunctionality.RandomChoice(availableQuests.AsEnumerable());
            availableQuests.Remove(randomQuestType);
            if (user.Quests.Any(j => j.GetType() == randomQuestType)) continue;
            user.Quests.Add((Quest) Activator.CreateInstance(randomQuestType)!);
            
        }

        
        user.LastTimeChecked = DateTime.UtcNow;


    }

    private static readonly Type[] _assemblyTypes;
    static PostgreSqlContext()
    {
        _assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();
        EntityClasses = _assemblyTypes
            .Where(type => type.IsRelatedToType(typeof(Entity))).ToArray();

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
        foreach (var i in QuestTypes)
        {
            modelBuilder.Entity(i);
        }
        foreach (var entityType in EntityClasses)
        {
            modelBuilder.Entity(entityType);
        }



        modelBuilder.Entity<Gear>(entity =>
            {
                
            });
       
        modelBuilder.Entity<UserData>(entity =>
        {
            entity.Property(i => i.Color)
                .HasConversion(i => i.ToString(), j => new DiscordColor(j)
                );
            entity.HasMany(i => i.PlayerTeams)
                .WithOne(i => i.UserData)
                .HasForeignKey(i => i.UserDataId);
                    
            entity
                .HasMany(i => i.Inventory)
                .WithOne(i => i.UserData)
                .HasForeignKey(i => i.UserDataId);
            entity
                .HasOne(i => i.EquippedPlayerTeam)
                .WithOne()
                .HasForeignKey<PlayerTeam>(i => i.EquippedUserDataId)
                .OnDelete(DeleteBehavior.SetNull);

            entity
                .HasMany(i => i.Quests)
                .WithOne()
                .HasForeignKey(i => i.UserDataId);
            entity
                .HasMany(i => i.Quotes)
                .WithOne(i => i.UserData)
                .HasForeignKey(i => i.UserDataId);

        });

        modelBuilder.Entity<Entity>(entity =>
        {
            entity.HasKey(i => i.Id);
            
            entity
                .Property(i => i.Id)
                .ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Quote>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity
                .Property(i => i.Id)
                .ValueGeneratedOnAdd();
            entity.HasMany(i => i.QuoteReactions)
                .WithOne(i => i.Quote)
                .HasForeignKey(i => i.QuoteId);
        });

        modelBuilder.Entity<PlayerTeam>(entity =>
        {
            entity.Property(i => i.Id)
                .ValueGeneratedOnAdd();
            entity.HasIndex(i => new { i.UserDataId, i.TeamName })
                .IsUnique();
            entity    .Navigation(i => i.Characters)
                .AutoInclude();
            entity.HasKey(i => i.Id);
            entity.HasMany<Character>(i => i.Characters)
                .WithMany(i => i.PlayerTeams)
                .UsingEntity<CharacterPlayerTeam>(i
                    => i.HasOne<Character>().WithMany().HasForeignKey(j => j.CharacterId), i =>
                    i.HasOne<PlayerTeam>().WithMany().HasForeignKey(j => j.PlayerTeamId), i =>
                    i.HasKey(j => new { j.CharacterId, j.PlayerTeamId }));
        });
    

            
        modelBuilder.Entity<Character>(entity =>
        {
            entity.HasMany(i => i.Gears)
                .WithOne()
                .HasForeignKey(i => i.CharacterGearEquipperId);

            entity .HasOne(i => i.Blessing)
                .WithOne(i => i.Character)
                .HasForeignKey<Blessing>(i => i.CharacterBlessingEquipperId)
                .OnDelete(DeleteBehavior.SetNull);
        });



        modelBuilder.Entity<Player>()
            .Property(i => i.Element)
            .HasColumnName(nameof(Player.Element));




        modelBuilder.Entity<UserData>(entity =>
        {
            entity.HasMany(i => i.QuoteReactions)
                .WithOne(i => i.UserData)
                .HasForeignKey(i => i.UserDataId);
            entity.HasKey(i => i.Id);

        });
       

  
    
           
        
    }
}