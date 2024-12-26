using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ttc.DataAccess.Services;
using Ttc.Model.Players;
using Ttc.WebApi.Utilities.Auth;

namespace Ttc.WebApi.Controllers;

[Authorize]
[Route("api/players")]
public class PlayersController
{
    #region Constructor
    private readonly PlayerService _service;
    private readonly UserProvider _user;

    public PlayersController(PlayerService service, UserProvider user)
    {
        _service = service;
        _user = user;
    }
    #endregion

    [HttpGet]
    [AllowAnonymous]
    public async Task<IEnumerable<Player>> Get()
    {
        var result = await _service.GetOwnClub();
        _user.CleanSensitiveData(result);
        return result;
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<Player> Get(int id)
    {
        var result = await _service.GetPlayer(id);
        _user.CleanSensitiveData(result);
        return result;
    }

    [HttpPost]
    [Route("UpdateStyle")]
    public async Task<Player> UpdateStyle([FromBody] PlayerStyle playerStyle)
    {
        var result = await _service.UpdateStyle(playerStyle);
        return result;
    }

    [HttpPost]
    [Route("UpdatePlayer")]
    public async Task<Player> UpdatePlayer([FromBody] Player player)
    {
        var result = await _service.UpdatePlayer(player);
        return result;
    }

    [HttpPost]
    [Route("DeletePlayer/{playerId:int}")]
    public async Task DeletePlayer(int playerId)
    {
        await _service.DeletePlayer(playerId);
    }

    [HttpPost]
    [Route("FrenoySync")]
    public async Task FrenoySync()
    {
        await _service.FrenoySync();
    }

    [HttpGet]
    [Route("ExcelExport")]
    public async Task<string> GetExcelExport()
    {
        byte[] excel = await _service.GetExcelExport();
        return Convert.ToBase64String(excel);
    }
}
