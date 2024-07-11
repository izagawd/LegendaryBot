using DiscordBotNet.Database;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiscordBotNet.Pages;
[Authorize]
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;

    }

    public UserData UserData { get; set; }



    public async Task OnGetAsync()
    {

        await using var databaseContext = new PostgreSqlContext();
        UserData = await databaseContext.UserData.FindOrCreateUserDataAsync(User.GetDiscordUserId());
    }

}