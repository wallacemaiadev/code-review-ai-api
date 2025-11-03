using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;

using NDE.Application.Interfaces;
using NDE.Domain.Notifications;

namespace NDE.Api.Controllers;

[ApiVersion(1)]
[Route("api/v{v:apiVersion}/[controller]")]
public class TeamStandardController : MainController
{
  private readonly ITeamStandardService _teamStandardService;
  private readonly ILogger<TeamStandardController> _logger;

  public TeamStandardController(ITeamStandardService teamStandardService, ILogger<TeamStandardController> logger, INotificator notificator) : base(notificator)
  {
    _teamStandardService = teamStandardService;
    _logger = logger;
  }

  [MapToApiVersion(1)]
  [HttpGet("{repositoryId:guid}")]
  public async Task<IActionResult> GetTeamStandard([FromRoute] Guid repositoryId)
  {
    if (repositoryId == Guid.Empty)
    {
      NotifyError("O identificador informado é inválido ou não foi fornecido.");

      _logger.LogWarning(
        "ID inválido recebido na requisição. Id: {Id}",
        repositoryId);

      return NDEResponse();
    }

    return NDEResponse(await _teamStandardService.GetTeamStandardAsync(repositoryId: repositoryId));
  }

  [MapToApiVersion(1)]
  [HttpPost("save/{repositoryId:guid}")]
  public async Task<IActionResult> CreateTeamStandard([FromRoute] Guid repositoryId, [FromBody] object teamStandard)
  {
    return NDEResponse(await _teamStandardService.SaveTeamStandardAsync(teamStandard: teamStandard, repositoryId: repositoryId));
  }
}
