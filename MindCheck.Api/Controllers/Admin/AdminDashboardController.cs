using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindCheck.Application.Dtos.Admin;
using MindCheck.Application.UseCases.Admin;

namespace MindCheck.Api.Controllers.Admin;

[ApiController]
[Authorize]
[Route("api/admin/dashboard")]
public sealed class AdminDashboardController : ControllerBase
{
    private readonly GetDashboardStatsUseCase _getDashboardStatsUseCase;

    public AdminDashboardController(GetDashboardStatsUseCase getDashboardStatsUseCase)
    {
        _getDashboardStatsUseCase = getDashboardStatsUseCase;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetStats(CancellationToken cancellationToken)
    {
        return Ok(await _getDashboardStatsUseCase.ExecuteAsync(cancellationToken));
    }
}
