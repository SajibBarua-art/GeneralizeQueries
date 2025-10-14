using System.Transactions;
using GeneralizeQueries.Api.DTOs.FeatureManagement;
using GeneralizeQueries.Core.Entities.FeatureManagement;
using GeneralizeQueries.Core.Exceptions;
using GeneralizeQueries.Core.Interfaces;
using GeneralizeQueries.Core.Models;
using GeneralizeQueries.Core.Models.FeatureManagement;
using GeneralizeQueries.Core.Models.Validation;
using Microsoft.Extensions.Logging;

namespace GeneralizeQueries.Application;

public class FeatureManagementService : IFeatureManagementService
{
    private readonly IFeatureAggregateRootsRepositoryFactory _featureAggregateRootsRepositoryFactory;
    private readonly IFeatureAggregateRootsService _featureAggregateRootsService;
    private readonly IFeatureViewModelsRepositoryFactory _featureViewModelsRepositoryFactory;
    private readonly IFeatureViewModelsService _featureViewModelsService;
    private readonly ILogger<FeatureManagementService> _logger;
    private readonly IRoleFeatureViewModelsRepositoryFactory _roleFeatureViewModelsRepositoryFactory;
    private readonly IRoleFeatureViewModelsService _roleFeatureViewModelsService;

    public FeatureManagementService(
        IFeatureAggregateRootsService featureAggregateRootsService,
        IFeatureAggregateRootsRepositoryFactory featureAggregateRootsRepositoryFactory,
        IFeatureViewModelsService featureViewModelsService,
        IFeatureViewModelsRepositoryFactory featureViewModelsRepositoryFactory,
        IRoleFeatureViewModelsService roleFeatureViewModelsService,
        IRoleFeatureViewModelsRepositoryFactory roleFeatureViewModelsRepositoryFactory,
        ILogger<FeatureManagementService> logger)
    {
        _featureAggregateRootsService = featureAggregateRootsService;
        _featureAggregateRootsRepositoryFactory = featureAggregateRootsRepositoryFactory;
        _featureViewModelsService = featureViewModelsService;
        _featureViewModelsRepositoryFactory = featureViewModelsRepositoryFactory;
        _roleFeatureViewModelsService = roleFeatureViewModelsService;
        _roleFeatureViewModelsRepositoryFactory = roleFeatureViewModelsRepositoryFactory;
        _logger = logger;
    }

    public async Task<PagedResult<FeatureManagementDto>> GetPagedFeaturesAsync(
        string serviceId,
        PaginationParameters parameters,
        string? searchTerm = null)
    {
        _logger.LogInformation(
            "Getting paged features for service: {ServiceId}, Page: {Page}, PageSize: {PageSize}, SearchTerm: {SearchTerm}",
            serviceId, parameters.Page, parameters.PageSize, searchTerm);

        try
        {
            // Get repositories using the same factory pattern as existing controllers
            var featureAggregateRootsRepo =
                await _featureAggregateRootsRepositoryFactory.CreateRepositoryAsync(serviceId);
            var featureViewModelsRepo = await _featureViewModelsRepositoryFactory.CreateRepositoryAsync(serviceId);
            var roleFeatureViewModelsRepo =
                await _roleFeatureViewModelsRepositoryFactory.CreateRepositoryAsync(serviceId);

            if (featureAggregateRootsRepo == null || featureViewModelsRepo == null || roleFeatureViewModelsRepo == null)
            {
                _logger.LogError(
                    "Service with ID '{ServiceId}' not found in configuration or missing required database configuration",
                    serviceId);
                throw new InvalidOperationException(
                    $"Service with ID '{serviceId}' not found in configuration or missing required database configuration.");
            }

            // Create a map to store features by ID
            var featureMap = new Dictionary<string, FeatureGroup>();

            // Process FeatureAggregateRoot collection
            await ProcessFeatureAggregateRoot(featureAggregateRootsRepo, featureMap);

            // Process FeatureViewModel collection
            await ProcessFeatureViewModel(featureViewModelsRepo, featureMap);

            // Process RoleFeatureViewModel collection
            await ProcessRoleFeatureViewModel(roleFeatureViewModelsRepo, featureMap);

            // Validate features
            ValidateFeatures(featureMap.Values.ToList());

            // Convert to FeatureManagementDto and apply filtering
            var allFeatures = featureMap.Values.Select(group => new FeatureManagementDto
            {
                Id = group.Id,
                Name = group.Name,
                Errors = group.Errors
            }).ToList();

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
                allFeatures = allFeatures.Where(f =>
                    f.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    f.Id.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();

            // Apply sorting
            var sortedFeatures = ApplySorting(allFeatures, parameters.SortBy, parameters.SortDirection);

            // Apply pagination
            var totalCount = sortedFeatures.Count;
            var pagedFeatures = sortedFeatures
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToList();

            _logger.LogInformation("Returning {Count} features out of {TotalCount} for service: {ServiceId}",
                pagedFeatures.Count, totalCount, serviceId);
            return PagedResult<FeatureManagementDto>.Create(pagedFeatures, parameters.Page, parameters.PageSize,
                totalCount);
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogError(ex,
                "MongoDB connection was disposed while processing features for service: {ServiceId}. This may indicate a cache eviction issue.",
                serviceId);
            throw new InvalidOperationException(
                $"Database connection error for service '{serviceId}'. Please retry the request.", ex);
        }
        catch (InvalidOperationException)
        {
            // Re-throw InvalidOperationException as-is (service not found, etc.)
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting paged features for service: {ServiceId}", serviceId);
            throw;
        }
    }

    public async Task<FeatureManagementDetailDto> GetByIdAsync(
        string serviceId,
        Guid id)
    {
        _logger.LogInformation("Getting feature by ID: {FeatureId} for service: {ServiceId}", id, serviceId);

        try
        {
            // Get all repositories
            var featureAggregateRepo = await GetFeatureAggregateRepository(serviceId);
            var featureViewModelRepo = await GetFeatureViewModelRepository(serviceId);
            var roleFeatureViewModelRepo = await GetRoleFeatureViewModelRepository(serviceId);

            // Fetch entities from all three collections
            var featureAggregate = await _featureAggregateRootsService.GetByIdAsync(featureAggregateRepo, id);
            var featureViewModel = await _featureViewModelsService.GetByIdAsync(featureViewModelRepo, id);
            var roleFeatureViewModel = await _roleFeatureViewModelsService.GetByIdAsync(roleFeatureViewModelRepo, id);

            // Check if at least one entity exists
            if (featureAggregate == null && featureViewModel == null && roleFeatureViewModel == null)
            {
                _logger.LogWarning("No feature found with ID '{FeatureId}' in service '{ServiceId}'", id, serviceId);
                throw new NotFoundException($"No feature found with ID '{id}' in service '{serviceId}'.");
            }

            // Determine the feature name (priority: FeatureAggregate -> FeatureViewModel -> RoleFeatureViewModel)
            var featureName = featureAggregate?.Name ?? featureViewModel?.Name ?? roleFeatureViewModel?.Name ?? "";

            // Create detailed DTO
            var result = new FeatureManagementDetailDto
            {
                Id = id.ToString(),
                Name = featureName,
                ServiceId = serviceId,
                FeatureAggregateRoot = featureAggregate != null
                    ? new FeatureAggregateRootsDto
                    {
                        Name = featureAggregate.Name,
                        UniqName = featureAggregate.UniqName,
                        TagName = featureAggregate.TagName,
                        Commands = featureAggregate.Commands ?? new List<string>()
                    }
                    : null,
                FeatureViewModel = featureViewModel != null
                    ? new FeatureViewModelsDto
                    {
                        Name = featureViewModel.Name,
                        UniqName = featureViewModel.UniqName,
                        TagName = featureViewModel.TagName,
                        Commands = featureViewModel.Commands ?? new List<string>()
                    }
                    : null,
                RoleFeatureViewModel = roleFeatureViewModel != null
                    ? new RoleFeatureViewModelsDto
                    {
                        Name = roleFeatureViewModel.Name,
                        TagName = roleFeatureViewModel.TagName,
                        Commands = roleFeatureViewModel.Commands ?? new List<string>()
                    }
                    : null
            };

            _logger.LogInformation("Successfully retrieved feature details for ID: {FeatureId}, service: {ServiceId}",
                id, serviceId);
            return result;
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogError(ex,
                "MongoDB connection was disposed while getting feature by ID: {FeatureId} for service: {ServiceId}", id,
                serviceId);
            throw new InvalidOperationException(
                $"Database connection error for service '{serviceId}'. Please retry the request.", ex);
        }
        catch (NotFoundException)
        {
            // Re-throw NotFoundException as-is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting feature by ID: {FeatureId} for service: {ServiceId}",
                id, serviceId);
            throw;
        }
    }

    public async Task DeleteFeatureAtomicAsync(
        string serviceId,
        Guid id)
    {
        _logger.LogInformation("Starting atomic delete for feature ID: {FeatureId} in service: {ServiceId}", id,
            serviceId);
        // Use TransactionScope to ensure atomicity across all operations
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        try
        {
            // Get all repositories
            var featureAggregateRepo = await GetFeatureAggregateRepository(serviceId);
            var featureViewModelRepo = await GetFeatureViewModelRepository(serviceId);
            var roleFeatureViewModelRepo = await GetRoleFeatureViewModelRepository(serviceId);

            // Check if all entities exist before attempting to delete
            var featureAggregate = await _featureAggregateRootsService.GetByIdAsync(featureAggregateRepo, id);
            var featureViewModel = await _featureViewModelsService.GetByIdAsync(featureViewModelRepo, id);
            var roleFeatureViewModel = await _roleFeatureViewModelsService.GetByIdAsync(roleFeatureViewModelRepo, id);

            // Validate that at least one entity exists
            if (featureAggregate == null && featureViewModel == null && roleFeatureViewModel == null)
            {
                _logger.LogWarning("No feature entities found with ID '{FeatureId}' in service '{ServiceId}'", id,
                    serviceId);
                throw new NotFoundException($"No feature entities found with ID '{id}' in service '{serviceId}'.");
            }

            // Delete entities that exist (in reverse dependency order if applicable)
            if (roleFeatureViewModel != null)
                await _roleFeatureViewModelsService.DeleteAsync(roleFeatureViewModelRepo, id);

            if (featureViewModel != null) await _featureViewModelsService.DeleteAsync(featureViewModelRepo, id);

            if (featureAggregate != null) await _featureAggregateRootsService.DeleteAsync(featureAggregateRepo, id);

            // If we get here without exceptions, commit the transaction
            transaction.Complete();
            _logger.LogInformation("Successfully deleted feature ID: {FeatureId} from service: {ServiceId}", id,
                serviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting feature ID: {FeatureId} from service: {ServiceId}", id, serviceId);
            // Transaction will automatically rollback if Complete() is not called
            throw;
        }
    }

    public async Task ValidateUniqNameAsync(
        string serviceId,
        string uniqName,
        Guid? excludeId = null)
    {
        _logger.LogInformation("Validating UniqName: {UniqName} for service: {ServiceId}, ExcludeId: {ExcludeId}",
            uniqName, serviceId, excludeId);

        try
        {
            // Get repositories
            var featureAggregateRepo = await GetFeatureAggregateRepository(serviceId);
            var featureViewModelRepo = await GetFeatureViewModelRepository(serviceId);

            var errors = new List<string>();

            // Check FeatureAggregateRoots collection
            await CheckUniqNameInFeatureAggregateRoots(featureAggregateRepo, uniqName, excludeId, errors);

            // Check FeatureViewModels collection
            await CheckUniqNameInFeatureViewModels(featureViewModelRepo, uniqName, excludeId, errors);

            // If any duplicates found, throw exception
            if (errors.Any())
            {
                _logger.LogWarning("UniqName '{UniqName}' already exists in: {Collections}", uniqName,
                    string.Join(", ", errors));
                throw new DuplicateUniqNameException(
                    $"UniqName '{uniqName}' already exists in: {string.Join(", ", errors)}");
            }
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogError(ex,
                "MongoDB connection was disposed while validating UniqName: {UniqName} for service: {ServiceId}",
                uniqName, serviceId);
            throw new InvalidOperationException(
                $"Database connection error for service '{serviceId}'. Please retry the request.", ex);
        }
        catch (DuplicateUniqNameException)
        {
            // Re-throw DuplicateUniqNameException as-is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while validating UniqName: {UniqName} for service: {ServiceId}",
                uniqName, serviceId);
            throw;
        }
    }

    private async Task ProcessFeatureAggregateRoot(
        IFeatureAggregateRootsRepository repository,
        Dictionary<string, FeatureGroup> featureMap)
    {
        // Get all feature aggregate roots using the repository
        var paginationParams = new PaginationParameters
        {
            Page = 1,
            PageSize = int.MaxValue, // Get all records
            SortBy = null,
            SortDirection = "asc"
        };

        var pagedResult = await _featureAggregateRootsService.GetPagedAsync(repository, paginationParams);

        foreach (var featureAggregate in pagedResult.Items)
        {
            var id = featureAggregate.Id.ToString();
            var name = featureAggregate.Name;
            var uniqName = featureAggregate.UniqName;
            var commands = featureAggregate.Commands;

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
                continue;

            var group = featureMap.GetValueOrDefault(id);
            if (group == null)
            {
                group = new FeatureGroup { Id = id, Name = name, FeatureItems = [] };
                featureMap[id] = group;
            }

            group.FeatureItems.Add(
                new FeatureItem
                {
                    Id = id,
                    Name = name,
                    UniqName = uniqName ?? "",
                    Commands = commands,
                    CollectionName = nameof(FeatureAggregateRoot)
                });
        }
    }

    private async Task ProcessFeatureViewModel(
        IFeatureViewModelsRepository repository,
        Dictionary<string, FeatureGroup> featureMap)
    {
        // Get all feature view models using the repository
        var paginationParams = new PaginationParameters
        {
            Page = 1,
            PageSize = int.MaxValue, // Get all records
            SortBy = null,
            SortDirection = "asc"
        };

        var pagedResult = await _featureViewModelsService.GetPagedAsync(repository, paginationParams);

        foreach (var featureViewModel in pagedResult.Items)
        {
            var id = featureViewModel.Id.ToString();
            var name = featureViewModel.Name;
            var uniqName = featureViewModel.UniqName;
            var commands = featureViewModel.Commands;

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
                continue;

            var group = featureMap.GetValueOrDefault(id);
            if (group == null)
            {
                group = new FeatureGroup { Id = id, Name = name, FeatureItems = [] };
                featureMap[id] = group;
            }

            group.FeatureItems.Add(
                new FeatureItem
                {
                    Id = id,
                    Name = name,
                    UniqName = uniqName ?? "",
                    Commands = commands,
                    CollectionName = nameof(FeatureViewModel)
                });
        }
    }

    private async Task ProcessRoleFeatureViewModel(
        IRoleFeatureViewModelsRepository repository,
        Dictionary<string, FeatureGroup> featureMap)
    {
        // Get all role feature view models using the repository
        var paginationParams = new PaginationParameters
        {
            Page = 1,
            PageSize = int.MaxValue, // Get all records
            SortBy = null,
            SortDirection = "asc"
        };

        var pagedResult = await _roleFeatureViewModelsService.GetPagedAsync(repository, paginationParams);

        foreach (var roleFeatureViewModel in pagedResult.Items)
        {
            var id = roleFeatureViewModel.Id.ToString();
            var name = roleFeatureViewModel.Name;
            var commands = roleFeatureViewModel.Commands;

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
                continue;

            var group = featureMap.GetValueOrDefault(id);
            if (group == null)
            {
                group = new FeatureGroup { Id = id, Name = name, FeatureItems = [] };
                featureMap[id] = group;
            }

            group.FeatureItems.Add(
                new FeatureItem
                {
                    Id = id,
                    Name = name,
                    UniqName = "",
                    Commands = commands,
                    CollectionName = nameof(RoleFeatureViewModel)
                });
        }
    }


    private static void ValidateFeatures(List<FeatureGroup> featureGroups)
    {
        const string aggregateRootCollectionName = nameof(FeatureAggregateRoot);
        const string viewModelCollectionName = nameof(FeatureViewModel);

        foreach (var group in featureGroups)
        {
            // --- 1. Check for missing MANDATORY collections ---
            if (!group.FeatureItems.Any(item => item.CollectionName == aggregateRootCollectionName))
                group.Errors.Add(new ValidationErrorMessage
                {
                    Message = $"This role is missing on '{aggregateRootCollectionName}' collection."
                });
            if (!group.FeatureItems.Any(item => item.CollectionName == viewModelCollectionName))
                group.Errors.Add(new ValidationErrorMessage
                {
                    Message = $"This role is missing on '{viewModelCollectionName}' collection."
                });

            // --- 2. Check for command mismatch using pairwise comparison ---
            if (group.FeatureItems.Count > 1)
            {
                var mismatchFound = false;
                for (var i = 0; i < group.FeatureItems.Count - 1; i++)
                {
                    for (var j = i + 1; j < group.FeatureItems.Count; j++)
                    {
                        var item1 = group.FeatureItems[i];
                        var item2 = group.FeatureItems[j];

                        var commands1 = item1.Commands.OrderBy(c => c);
                        var commands2 = item2.Commands.OrderBy(c => c);

                        if (!commands1.SequenceEqual(commands2))
                        {
                            // Create a new structured error object
                            var validationError = new ValidationErrorMessage
                            {
                                Message =
                                    $"Command mismatch found between '{item1.CollectionName}' and '{item2.CollectionName}'."
                            };

                            // Find the specific differences and add them to the Details list
                            var onlyInItem1 = commands1.Except(commands2).ToList();
                            var onlyInItem2 = commands2.Except(commands1).ToList();

                            if (onlyInItem1.Any())
                                validationError.Details.Add(
                                    $"'{item1.CollectionName}' has commands not in '{item2.CollectionName}': [{string.Join(", ", onlyInItem1)}]");
                            if (onlyInItem2.Any())
                                validationError.Details.Add(
                                    $"'{item2.CollectionName}' has commands not in '{item1.CollectionName}': [{string.Join(", ", onlyInItem2)}]");

                            // Add the complete, structured error object to the list
                            group.Errors.Add(validationError);

                            mismatchFound = true;
                            break;
                        }
                    }

                    if (mismatchFound) break;
                }
            }
        }
    }

    private static List<FeatureManagementDto> ApplySorting(
        List<FeatureManagementDto> features,
        string? sortBy,
        string sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortBy)) return features.OrderBy(f => f.Name).ToList();

        var isDescending = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return sortBy.ToLowerInvariant() switch
        {
            "id" => isDescending
                ? features.OrderByDescending(f => f.Id).ToList()
                : features.OrderBy(f => f.Id).ToList(),
            "name" => isDescending
                ? features.OrderByDescending(f => f.Name).ToList()
                : features.OrderBy(f => f.Name).ToList(),
            "errors" => isDescending
                ? features.OrderByDescending(f => f.Errors.Count).ToList()
                : features.OrderBy(f => f.Errors.Count).ToList(),
            _ => features.OrderBy(f => f.Name).ToList()
        };
    }

    private async Task<IFeatureAggregateRootsRepository> GetFeatureAggregateRepository(string serviceId)
    {
        var repo = await _featureAggregateRootsRepositoryFactory.CreateRepositoryAsync(serviceId);
        if (repo == null)
            throw new NotFoundException($"Service with ID '{serviceId}' not found in feature aggregate configuration.");
        return repo;
    }

    private async Task<IFeatureViewModelsRepository> GetFeatureViewModelRepository(string serviceId)
    {
        var repo = await _featureViewModelsRepositoryFactory.CreateRepositoryAsync(serviceId);
        if (repo == null)
            throw new NotFoundException(
                $"Service with ID '{serviceId}' not found in feature view model configuration.");
        return repo;
    }

    private async Task<IRoleFeatureViewModelsRepository> GetRoleFeatureViewModelRepository(string serviceId)
    {
        var repo = await _roleFeatureViewModelsRepositoryFactory.CreateRepositoryAsync(serviceId);
        if (repo == null)
            throw new NotFoundException(
                $"Service with ID '{serviceId}' not found in role feature view model configuration.");
        return repo;
    }

    private async Task CheckUniqNameInFeatureAggregateRoots(
        IFeatureAggregateRootsRepository repository,
        string uniqName,
        Guid? excludeId,
        List<string> errors)
    {
        // Get all feature aggregate roots
        var paginationParams = new PaginationParameters
        {
            Page = 1,
            PageSize = int.MaxValue,
            SortBy = null,
            SortDirection = "asc"
        };

        var pagedResult = await _featureAggregateRootsService.GetPagedAsync(repository, paginationParams);

        // Check for duplicate UniqName
        var duplicateExists = pagedResult.Items.Any(item =>
            !string.IsNullOrEmpty(item.UniqName) &&
            item.UniqName.Equals(uniqName, StringComparison.OrdinalIgnoreCase) &&
            (excludeId == null || item.Id != excludeId.Value));

        if (duplicateExists) errors.Add("FeatureAggregateRoots");
    }

    private async Task CheckUniqNameInFeatureViewModels(
        IFeatureViewModelsRepository repository,
        string uniqName,
        Guid? excludeId,
        List<string> errors)
    {
        // Get all feature view models
        var paginationParams = new PaginationParameters
        {
            Page = 1,
            PageSize = int.MaxValue,
            SortBy = null,
            SortDirection = "asc"
        };

        var pagedResult = await _featureViewModelsService.GetPagedAsync(repository, paginationParams);

        // Check for duplicate UniqName
        var duplicateExists = pagedResult.Items.Any(item =>
            !string.IsNullOrEmpty(item.UniqName) &&
            item.UniqName.Equals(uniqName, StringComparison.OrdinalIgnoreCase) &&
            (excludeId == null || item.Id != excludeId.Value));

        if (duplicateExists) errors.Add("FeatureViewModels");
    }
}