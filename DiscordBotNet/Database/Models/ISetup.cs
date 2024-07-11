namespace DiscordBotNet.Database.Models;

public interface ISetup
{

    /// <summary>
    /// Used to setup an entity properly in the  database and for client use. The setup should be implemented without a transaction,
    /// so you can use a transaction if you want to bulk setup multiple things. This method calls savechanges async, so beware. <br/>
    /// If you want it to be done with a transaction, use <see cref="SetupWithTransactionAsync"/>. it does that for you.
    /// Use <see cref="PostgreSqlContext.HardSaveChangesAsync"/> when you want to save changes in this implementation. If not, a recursion you
    /// will not like will occur
    /// </summary>
    /// <param name="context">The context where the entity came from</param>
    /// <returns>The number of state entries written to the database</returns>
    public void Setup();


}