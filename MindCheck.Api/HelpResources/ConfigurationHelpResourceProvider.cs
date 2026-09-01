using MindCheck.Application.Abstractions;
using MindCheck.Application.Dtos;

namespace MindCheck.Api.HelpResources;

// Emergency hotline info is deployment config, not clinical data, so it comes
// from appsettings rather than a database table.
public sealed class ConfigurationHelpResourceProvider : IHelpResourceProvider
{
    private readonly IReadOnlyList<HelpResourceDto> _resources;

    public ConfigurationHelpResourceProvider(IConfiguration configuration)
    {
        _resources = configuration.GetSection("EmergencyHelpResources").Get<List<HelpResourceDto>>()
            ?? new List<HelpResourceDto>();
    }

    public IReadOnlyList<HelpResourceDto> GetEmergencyResources() => _resources;
}
