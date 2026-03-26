using SportsLeague.Domain.Entities;

namespace SportsLeague.Domain.Interfaces.Services;

public interface ISponsorService
{
    // CRUD Sponsor
    Task<IEnumerable<Sponsor>> GetAllAsync();
    Task<Sponsor?> GetByIdAsync(int id);
    Task<Sponsor> CreateAsync(Sponsor sponsor);
    Task UpdateAsync(int id, Sponsor sponsor);
    Task DeleteAsync(int id);

    // Lógica N:M
    Task<IEnumerable<TournamentSponsor>> GetTournamentsBySponsorAsync(int sponsorId);
    Task<TournamentSponsor> LinkToTournamentAsync(int sponsorId, int tournamentId, decimal contractAmount);
    Task UnlinkFromTournamentAsync(int sponsorId, int tournamentId);
}