using Grpc.Core;
using SurveyService.Community; // This is the generated namespace from community.proto
using System; // For Guid
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic; // For List

// Assuming the namespace for the service implementation
namespace SurveyService.CommunityImplementation
{
    public class CommunityServiceImpl : SurveyService.Community.CommunityService.CommunityServiceBase
    {
        // Placeholder for a logger
        // private readonly ILogger<CommunityServiceImpl> _logger;

        // In-memory store for sample data
        private static List<Roundtable> _roundtablesStorage = new List<Roundtable>();
        private static readonly object _lock = new object();

        public CommunityServiceImpl(/*ILogger<CommunityServiceImpl> logger*/)
        {
            // _logger = logger;
            // Initialize with sample data if storage is empty
            lock (_lock)
            {
                if (!_roundtablesStorage.Any())
                {
                    _roundtablesStorage.AddRange(GenerateSampleRoundtables());
                }
            }
        }

        public override Task<ListRoundtablesResponse> ListRoundtables(ListRoundtablesRequest request, ServerCallContext context)
        {
            // Validate inputs
            if (request.PageSize <= 0) request.PageSize = 10;
            if (request.PageSize > 100) request.PageSize = 100;

            List<Roundtable> currentRoundtablesView;
            lock(_lock) // Ensure thread-safe access to the list
            {
                // Make a copy for filtering to avoid modifying the original list during iteration
                currentRoundtablesView = new List<Roundtable>(_roundtablesStorage);
            }
            
            var filteredRoundtables = currentRoundtablesView;

            if (!string.IsNullOrEmpty(request.SearchQuery))
            {
                string query = request.SearchQuery.ToLower();
                filteredRoundtables = filteredRoundtables.Where(r =>
                    (r.Name?.ToLower().Contains(query) ?? false) ||
                    (r.Director?.Name?.ToLower().Contains(query) ?? false) ||
                    (r.Associates?.Any(a => a.Name?.ToLower().Contains(query) ?? false) ?? false) ||
                    (r.Abbreviation?.ToLower().Contains(query) ?? false) // Search by abbreviation
                ).ToList();
            }

            if (request.StatusFilter != RoundtableStatus.All && request.StatusFilter != RoundtableStatus.RoundtableStatusUnspecified)
            {
                filteredRoundtables = filteredRoundtables.Where(r => r.Status == request.StatusFilter).ToList();
            }
            
            // Basic offset calculation if page_token is a simple integer offset.
            // This is still a placeholder for true cursor-based pagination.
            int skipAmount = 0;
            if (!string.IsNullOrEmpty(request.PageToken) && int.TryParse(request.PageToken, out int parsedOffset))
            {
                skipAmount = parsedOffset;
            }
            
            int totalItems = filteredRoundtables.Count;
            var paginatedRoundtables = filteredRoundtables.Skip(skipAmount).Take(request.PageSize).ToList();

            var response = new ListRoundtablesResponse { TotalSize = totalItems };
            response.Roundtables.AddRange(paginatedRoundtables);

            if ((skipAmount + paginatedRoundtables.Count) < totalItems)
            {
                // Placeholder for next_page_token. In a real scenario, this would be the 'skipAmount + request.PageSize' or a cursor.
                response.NextPageToken = (skipAmount + request.PageSize).ToString(); 
            }

            return Task.FromResult(response);
        }

        public override Task<CreateRoundtableResponse> CreateRoundtable(CreateRoundtableRequest request, ServerCallContext context)
        {
            // --- Input Validation ---
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Roundtable Name is required."));
            }
            if (string.IsNullOrWhiteSpace(request.Abbreviation))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Roundtable Abbreviation is required."));
            }
            if (request.DirectorUserIds == null || !request.DirectorUserIds.Any())
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "At least one Director must be selected."));
            }
            if (string.IsNullOrWhiteSpace(request.PrimaryDirectorUserId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Primary Director is required."));
            }
            if (!request.DirectorUserIds.Contains(request.PrimaryDirectorUserId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Primary Director must be one of the selected Directors."));
            }
            if (!string.IsNullOrWhiteSpace(request.PrimaryAssociateUserId) && (request.AssociateUserIds == null || !request.AssociateUserIds.Contains(request.PrimaryAssociateUserId)))
            {
                 throw new RpcException(new Status(StatusCode.InvalidArgument, "Primary Associate must be one of the selected Associates."));
            }

            // Placeholder for abbreviation uniqueness check (requires data store access)
            lock(_lock)
            {
                if (_roundtablesStorage.Any(rt => rt.Abbreviation.Equals(request.Abbreviation, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new RpcException(new Status(StatusCode.AlreadyExists, $"Roundtable Abbreviation '{request.Abbreviation}' already exists."));
                }
            }

            // --- Construct Roundtable Object ---
            var newRoundtable = new Roundtable
            {
                Id = Guid.NewGuid().ToString(), // Generate new UUID
                Name = request.Name,
                Abbreviation = request.Abbreviation,
                Description = request.Description ?? string.Empty, // Handle optional field
                Status = request.IsActive ? RoundtableStatus.Active : RoundtableStatus.Inactive,
                // Populate Director
                Director = new UserDetails { 
                    UserId = request.PrimaryDirectorUserId, 
                    Name = $"User_{request.PrimaryDirectorUserId.Substring(0,Math.Min(5, request.PrimaryDirectorUserId.Length))}", // Placeholder name, ensure substring length is valid
                    IsPrimary = true 
                },
                ClientsWithAccessCount = request.ClientIds?.Count ?? 0
            };
            if (request.ClientIds != null)
            {
                newRoundtable.ClientIds.AddRange(request.ClientIds);
            }

            // Populate Associates
            if (request.AssociateUserIds != null)
            {
                foreach (var assocId in request.AssociateUserIds)
                {
                    newRoundtable.Associates.Add(new UserDetails {
                        UserId = assocId,
                        Name = $"User_{assocId.Substring(0,Math.Min(5, assocId.Length))}", // Placeholder name, ensure substring length is valid
                        IsPrimary = (assocId == request.PrimaryAssociateUserId)
                    });
                }
            }
            
            // --- Persist (Add to in-memory list) ---
            lock(_lock)
            {
                _roundtablesStorage.Add(newRoundtable);
            }

            // _logger?.LogInformation("New roundtable created with ID: {Id}", newRoundtable.Id);
            return Task.FromResult(new CreateRoundtableResponse { Roundtable = newRoundtable });
        }

        // Updated helper method to generate sample data
        private static List<Roundtable> GenerateSampleRoundtables()
        {
            var sampleDirector1 = new UserDetails { UserId = "user-dir-1", Name = "Dr. Alice Director", IsPrimary = true };
            var sampleDirector2 = new UserDetails { UserId = "user-dir-2", Name = "Mr. Bob Overseer", IsPrimary = true };

            var sampleAssociate1 = new UserDetails { UserId = "user-assoc-1", Name = "Charlie Associate", IsPrimary = true };
            var sampleAssociate2 = new UserDetails { UserId = "user-assoc-2", Name = "Diana Helper", IsPrimary = false };
            var sampleAssociate3 = new UserDetails { UserId = "user-assoc-3", Name = "Edward Contributor", IsPrimary = true };
            
            return new List<Roundtable>
            {
                new Roundtable
                {
                    Id = "rt-uuid-1", Name = "Alpha Roundtable", Abbreviation = "ALPHA", Description = "The first roundtable.",
                    Director = sampleDirector1, Status = RoundtableStatus.Active, ClientsWithAccessCount = 10,
                    Associates = { sampleAssociate1, sampleAssociate2 }, ClientIds = { "client-1", "client-2" }
                },
                new Roundtable
                {
                    Id = "rt-uuid-2", Name = "Beta Roundtable", Abbreviation = "BETA", Description = "The second roundtable, focusing on tech.",
                    Director = sampleDirector2, Status = RoundtableStatus.Active, ClientsWithAccessCount = 5,
                    Associates = { sampleAssociate3 }, ClientIds = { "client-3" }
                },
                new Roundtable
                {
                    Id = "rt-uuid-3", Name = "Gamma Roundtable", Abbreviation = "GAMMA", Description = "An inactive roundtable for archival.",
                    Director = sampleDirector1, Status = RoundtableStatus.Inactive, ClientsWithAccessCount = 7,
                    Associates = { sampleAssociate1 }, ClientIds = { "client-1", "client-4" }
                },
                new Roundtable
                {
                    Id = "rt-uuid-4", Name = "Delta Project Circle", Abbreviation = "DELTA", Description = "For special projects.",
                    Director = sampleDirector2, Status = RoundtableStatus.Active, ClientsWithAccessCount = 12,
                    Associates = { sampleAssociate2, sampleAssociate3 }, ClientIds = { "client-2", "client-3", "client-5" }
                },
                // Add a couple more to make pagination more evident
                new Roundtable
                {
                    Id = "rt-uuid-5", Name = "Epsilon Think Tank", Abbreviation = "EPSILON", Description = "Strategic discussions.",
                    Director = sampleDirector1, Status = RoundtableStatus.Active, ClientsWithAccessCount = 3,
                    Associates = { sampleAssociate1 }, ClientIds = { "client-6" }
                },
                new Roundtable
                {
                    Id = "rt-uuid-6", Name = "Zeta Initiative", Abbreviation = "ZETA", Description = "Future planning group.",
                    Director = sampleDirector2, Status = RoundtableStatus.Active, ClientsWithAccessCount = 8,
                    Associates = { sampleAssociate2 }, ClientIds = { "client-7", "client-8" }
                }
            };
        }
    }
}
