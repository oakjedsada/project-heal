using MindCheck.Application.Dtos;

namespace MindCheck.Application.Abstractions;

// Emergency contact info is presentation/config, not part of the clinical data
// model, so it is sourced outside Application (see Api layer) via this seam.
public interface IHelpResourceProvider
{
    IReadOnlyList<HelpResourceDto> GetEmergencyResources();
}
