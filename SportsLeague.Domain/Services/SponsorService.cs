using System.Net.Mail;
using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.Domain.Services;

public class SponsorService : ISponsorService
{
    private readonly ISponsorRepository _sponsorRepository;
    private readonly ITournamentSponsorRepository _tournamentSponsorRepository;
    private readonly ITournamentRepository _tournamentRepository;
    private readonly ILogger<SponsorService> _logger;

    public SponsorService(
        ISponsorRepository sponsorRepository,
        ITournamentSponsorRepository tournamentSponsorRepository,
        ITournamentRepository tournamentRepository,
        ILogger<SponsorService> logger)
    {
        _sponsorRepository = sponsorRepository;
        _tournamentSponsorRepository = tournamentSponsorRepository;
        _tournamentRepository = tournamentRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<Sponsor>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all sponsors");
        return await _sponsorRepository.GetAllAsync();
    }

    public async Task<Sponsor?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving sponsor with ID: {SponsorId}", id);
        var sponsor = await _sponsorRepository.GetByIdAsync(id);

        if (sponsor == null)
            _logger.LogWarning("Sponsor with ID {SponsorId} not found", id);

        return sponsor;
    }

    public async Task<Sponsor> CreateAsync(Sponsor sponsor)
    {
        var existsByName = await _sponsorRepository.ExistsByNameAsync(sponsor.Name);
        if (existsByName)
        {
            throw new InvalidOperationException($"Ya existe un sponsor con el nombre '{sponsor.Name}'");
        }

        ValidateEmail(sponsor.ContactEmail);

        _logger.LogInformation("Creating sponsor: {SponsorName}", sponsor.Name);
        return await _sponsorRepository.CreateAsync(sponsor);
    }

    public async Task UpdateAsync(int id, Sponsor sponsor)
    {
        var existingSponsor = await _sponsorRepository.GetByIdAsync(id);
        if (existingSponsor == null)
        {
            throw new KeyNotFoundException($"No se encontró el sponsor con ID {id}");
        }

        if (!existingSponsor.Name.Equals(sponsor.Name, StringComparison.OrdinalIgnoreCase))
        {
            var existsByName = await _sponsorRepository.ExistsByNameAsync(sponsor.Name);
            if (existsByName)
            {
                throw new InvalidOperationException($"Ya existe un sponsor con el nombre '{sponsor.Name}'");
            }
        }

        ValidateEmail(sponsor.ContactEmail);

        existingSponsor.Name = sponsor.Name;
        existingSponsor.ContactEmail = sponsor.ContactEmail;
        existingSponsor.Phone = sponsor.Phone;
        existingSponsor.WebsiteUrl = sponsor.WebsiteUrl;
        existingSponsor.Category = sponsor.Category;

        _logger.LogInformation("Updating sponsor with ID: {SponsorId}", id);
        await _sponsorRepository.UpdateAsync(existingSponsor);
    }

    public async Task DeleteAsync(int id)
    {
        var exists = await _sponsorRepository.ExistsAsync(id);
        if (!exists)
        {
            throw new KeyNotFoundException($"No se encontró el sponsor con ID {id}");
        }

        _logger.LogInformation("Deleting sponsor with ID: {SponsorId}", id);
        await _sponsorRepository.DeleteAsync(id);
    }

    public async Task<TournamentSponsor> LinkTournamentAsync(int sponsorId, TournamentSponsor tournamentSponsor)
    {
        var sponsorExists = await _sponsorRepository.ExistsAsync(sponsorId);
        if (!sponsorExists)
        {
            throw new KeyNotFoundException($"No se encontró el sponsor con ID {sponsorId}");
        }

        var tournamentExists = await _tournamentRepository.ExistsAsync(tournamentSponsor.TournamentId);
        if (!tournamentExists)
        {
            throw new KeyNotFoundException($"No se encontró el torneo con ID {tournamentSponsor.TournamentId}");
        }

        if (tournamentSponsor.ContractAmount <= 0)
        {
            throw new InvalidOperationException("El ContractAmount debe ser mayor a 0");
        }

        var existingLink = await _tournamentSponsorRepository
            .GetByTournamentAndSponsorAsync(tournamentSponsor.TournamentId, sponsorId);

        if (existingLink != null)
        {
            throw new InvalidOperationException("Este sponsor ya está vinculado a este torneo");
        }

        tournamentSponsor.SponsorId = sponsorId;
        tournamentSponsor.JoinedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "Linking sponsor {SponsorId} to tournament {TournamentId}",
            sponsorId,
            tournamentSponsor.TournamentId);

        var createdLink = await _tournamentSponsorRepository.CreateAsync(tournamentSponsor);
        return await _tournamentSponsorRepository.GetByIdWithDetailsAsync(createdLink.Id) ?? createdLink;
    }

    public async Task<IEnumerable<TournamentSponsor>> GetTournamentsBySponsorAsync(int sponsorId)
    {
        var sponsorExists = await _sponsorRepository.ExistsAsync(sponsorId);
        if (!sponsorExists)
        {
            throw new KeyNotFoundException($"No se encontró el sponsor con ID {sponsorId}");
        }

        _logger.LogInformation("Retrieving tournaments for sponsor ID: {SponsorId}", sponsorId);
        return await _tournamentSponsorRepository.GetBySponsorIdAsync(sponsorId);
    }

    public async Task UnlinkTournamentAsync(int sponsorId, int tournamentId)
    {
        var sponsorExists = await _sponsorRepository.ExistsAsync(sponsorId);
        if (!sponsorExists)
        {
            throw new KeyNotFoundException($"No se encontró el sponsor con ID {sponsorId}");
        }

        var existingLink = await _tournamentSponsorRepository
            .GetByTournamentAndSponsorAsync(tournamentId, sponsorId);

        if (existingLink == null)
        {
            throw new KeyNotFoundException("No se encontró la vinculación entre el sponsor y el torneo");
        }

        _logger.LogInformation(
            "Unlinking sponsor {SponsorId} from tournament {TournamentId}",
            sponsorId,
            tournamentId);

        await _tournamentSponsorRepository.DeleteAsync(existingLink.Id);
    }

    private static void ValidateEmail(string email)
    {
        try
        {
            var mailAddress = new MailAddress(email);
            if (mailAddress.Address != email)
            {
                throw new InvalidOperationException("El ContactEmail no tiene un formato válido");
            }
        }
        catch
        {
            throw new InvalidOperationException("El ContactEmail no tiene un formato válido");
        }
    }
}