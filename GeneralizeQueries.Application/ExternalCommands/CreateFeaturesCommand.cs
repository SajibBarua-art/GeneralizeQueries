using GeneralizeQueries.Api.DTOs.FeatureManagement;
using Platform.Infrastructure.Core.Commands;

namespace Platform.Uam.Commands;

public sealed class CreateFeaturesCommand : Command
{
    public required FeatureDto[] Features { get; set; }
}