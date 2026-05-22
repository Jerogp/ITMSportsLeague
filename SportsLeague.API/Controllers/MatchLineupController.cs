using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SportsLeague.API.DTOs.Request;
using SportsLeague.API.DTOs.Response;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.API.Controllers;

[ApiController]
[Route("api/match/{matchId}")]
public class MatchLineupController : ControllerBase
{
    private readonly IMatchLineupService _matchLineupService;
    private readonly IMapper _mapper;

    public MatchLineupController(
        IMatchLineupService matchLineupService,
        IMapper mapper)
    {
        _matchLineupService = matchLineupService;
        _mapper = mapper;
    }

    [HttpPost("lineup")]
    public async Task<ActionResult<MatchLineupResponseDTO>> AddPlayerToLineup(
        int matchId,
        [FromBody] MatchLineupRequestDTO request)
    {
        try
        {
            var lineup = _mapper.Map<MatchLineup>(request);

            var created = await _matchLineupService.AddPlayerToLineupAsync(
                matchId,
                lineup);

            var response = _mapper.Map<MatchLineupResponseDTO>(created);

            return CreatedAtAction(
                nameof(GetLineupByMatch),
                new { matchId },
                response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet("lineup")]
    public async Task<ActionResult<IEnumerable<MatchLineupResponseDTO>>> GetLineupByMatch(
        int matchId)
    {
        try
        {
            var lineup = await _matchLineupService.GetLineupByMatchAsync(matchId);

            var response = _mapper.Map<IEnumerable<MatchLineupResponseDTO>>(lineup);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("lineup/team/{teamId}")]
    public async Task<ActionResult<IEnumerable<MatchLineupResponseDTO>>> GetLineupByMatchAndTeam(
        int matchId,
        int teamId)
    {
        try
        {
            var lineup = await _matchLineupService.GetLineupByMatchAndTeamAsync(
                matchId,
                teamId);

            var response = _mapper.Map<IEnumerable<MatchLineupResponseDTO>>(lineup);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("lineup/{lineupId}")]
    public async Task<IActionResult> DeleteLineup(
        int matchId,
        int lineupId)
    {
        try
        {
            await _matchLineupService.DeleteLineupAsync(matchId, lineupId);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}