using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using BasicFunctionality;
using DatabaseManagement;
using Entities.LegendaryBot;
using Entities.LegendaryBot.Entities.BattleEntities.Blessings;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Entities.BattleEntities.Gears;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebFunctions;
using Website.Pages.Characters;

namespace WebsiteApi.Apis.Hubs;


[Authorize]
public class EquipmentHub : Hub
{

}