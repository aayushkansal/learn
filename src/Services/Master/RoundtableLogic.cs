using SurveyService.Master; // Namespace from master.proto for Roundtable, UserDetails, etc.
using Grpc.Core; // For RpcException, Status, StatusCode
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SurveyService.MasterImplementation // Same namespace as MasterServiceImpl for simplicity
{
    public class RoundtableLogic
    {
        // In-memory store for sample data - similar to what was in CommunityService
        private static List<Roundtable> _roundtablesStorage = new List<Roundtable>();
        private static readonly object _lock = new object();

        // Constructor - Initializes with sample data if storage is empty
        public RoundtableLogic()
        {
            lock (_lock)
            {
                if (!_roundtablesStorage.Any())
                {
                    _roundtablesStorage.AddRange(GenerateSampleRoundtables());
                }
            }
        }

        // --- GetRoundtable ---
        public Task<GetRoundtableResponse> GetRoundtable(GetRoundtableRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RoundtableId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Roundtable ID cannot be empty."));
            }

            Roundtable roundtable;
            lock (_lock)
            {
                roundtable = _roundtablesStorage.FirstOrDefault(r => r.Id == request.RoundtableId);
            }

            if (roundtable == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Roundtable with ID '{request.RoundtableId}' not found."));
            }
            return Task.FromResult(new GetRoundtableResponse { Roundtable = roundtable });
        }

        // --- ListRoundtables ---
        public Task<ListRoundtablesResponse> ListRoundtables(ListRoundtablesRequest request)
        {
            if (request.PageSize <= 0) request.PageSize = 10;
            if (request.PageSize > 100) request.PageSize = 100; // Max page size

            List<Roundtable> currentRoundtablesView;
            lock (_lock)
            {
                currentRoundtablesView = new List<Roundtable>(_roundtablesStorage); // Work on a copy
            }

            var filteredRoundtables = currentRoundtablesView;

            if (!string.IsNullOrEmpty(request.SearchQuery))
            {
                string query = request.SearchQuery.ToLower();
                filteredRoundtables = filteredRoundtables.Where(r =>
                    (r.Name?.ToLower().Contains(query) ?? false) ||
                    (r.Abbreviation?.ToLower().Contains(query) ?? false) ||
                    (r.Description?.ToLower().Contains(query) ?? false) ||
                    (r.DirectorDetails?.Name?.ToLower().Contains(query) ?? false) ||
                    (r.AssociateDetails?.Any(a => a.Name?.ToLower().Contains(query) ?? false) ?? false)
                ).ToList();
            }

            if (request.StatusFilter != RoundtableStatus.RoundtableStatusUnspecified && request.StatusFilter != RoundtableStatus.All)
            {
                filteredRoundtables = filteredRoundtables.Where(r => r.Status == request.StatusFilter).ToList();
            }

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
                response.NextPageToken = (skipAmount + request.PageSize).ToString();
            }

            return Task.FromResult(response);
        }

        // --- CreateRoundtable ---
        public Task<CreateRoundtableResponse> CreateRoundtable(CreateRoundtableRequest request)
        {
            // Basic Validations
            if (string.IsNullOrWhiteSpace(request.Name)) throw new RpcException(new Status(StatusCode.InvalidArgument, "Roundtable Name is required."));
            if (string.IsNullOrWhiteSpace(request.Abbreviation)) throw new RpcException(new Status(StatusCode.InvalidArgument, "Roundtable Abbreviation is required."));
            if (request.DirectorUserIds == null || !request.DirectorUserIds.Any()) throw new RpcException(new Status(StatusCode.InvalidArgument, "At least one Director User ID must be provided."));
            if (string.IsNullOrWhiteSpace(request.PrimaryDirectorUserId)) throw new RpcException(new Status(StatusCode.InvalidArgument, "Primary Director User ID is required."));
            if (!request.DirectorUserIds.Contains(request.PrimaryDirectorUserId)) throw new RpcException(new Status(StatusCode.InvalidArgument, "Primary Director must be one of the selected Director User IDs."));
            if (!string.IsNullOrWhiteSpace(request.PrimaryAssociateUserId) && (request.AssociateUserIds == null || !request.AssociateUserIds.Contains(request.PrimaryAssociateUserId)))
                 throw new RpcException(new Status(StatusCode.InvalidArgument, "Primary Associate must be one of the selected Associate User IDs."));


            lock (_lock)
            {
                if (_roundtablesStorage.Any(rt => rt.Abbreviation.Equals(request.Abbreviation, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new RpcException(new Status(StatusCode.AlreadyExists, $"Roundtable Abbreviation '{request.Abbreviation}' already exists."));
                }
            }

            var newRoundtable = new Roundtable
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Name,
                Abbreviation = request.Abbreviation,
                Description = request.Description ?? string.Empty,
                Status = request.IsActive ? RoundtableStatus.Active : RoundtableStatus.Inactive,
                ClientsWithAccessCount = request.ClientIds?.Count ?? 0,
                DirectorDetails = new UserDetails { UserId = request.PrimaryDirectorUserId, Name = $"User_{request.PrimaryDirectorUserId.Substring(0, Math.Min(5, request.PrimaryDirectorUserId.Length))}", IsPrimary = true } // Placeholder name
            };
            if (request.ClientIds != null) newRoundtable.ClientIds.AddRange(request.ClientIds);

            if (request.AssociateUserIds != null)
            {
                foreach (var assocId in request.AssociateUserIds)
                {
                    newRoundtable.AssociateDetails.Add(new UserDetails {
                        UserId = assocId,
                        Name = $"User_{assocId.Substring(0, Math.Min(5, assocId.Length))}", // Placeholder name
                        IsPrimary = (assocId == request.PrimaryAssociateUserId)
                    });
                }
            }
            // Note: The CreateRoundtableRequest has director_user_ids and associate_user_ids (repeated string).
            // The Roundtable message has director_details (single UserDetails) and associate_details (repeated UserDetails).
            // This implementation currently only sets the PrimaryDirectorUserId into DirectorDetails.
            // A fuller implementation might involve looking up all user details from a UserService or mapping all director_user_ids to UserDetails objects if multiple directors were supported by the Roundtable message.
            // For now, we are using the primary director for DirectorDetails and selected associates for AssociateDetails.

            lock (_lock)
            {
                _roundtablesStorage.Add(newRoundtable);
            }

            return Task.FromResult(new CreateRoundtableResponse { Roundtable = newRoundtable });
        }

        // --- Helper to Generate Sample Data ---
        private static List<Roundtable> GenerateSampleRoundtables()
        {
            var sampleDirector1 = new UserDetails { UserId = "user-dir-1", Name = "Dr. Alice Director", IsPrimary = true };
            var sampleDirector2 = new UserDetails { UserId = "user-dir-2", Name = "Mr. Bob Overseer", IsPrimary = true };

            var sampleAssociate1 = new UserDetails { UserId = "user-assoc-1", Name = "Charlie Associate", IsPrimary = true };
            var sampleAssociate2 = new UserDetails { UserId = "user-assoc-2", Name = "Diana Helper", IsPrimary = false };
            var sampleAssociate3 = new UserDetails { UserId = "user-assoc-3", Name = "Edward Contributor", IsPrimary = true };

            return new List<Roundtable>
            {
                new Roundtable {
                    Id = "rt-master-1", Name = "Master Alpha Roundtable", Abbreviation = "M_ALPHA", Description = "The first roundtable in Master.",
                    Status = RoundtableStatus.Active, ClientsWithAccessCount = 10, DirectorDetails = sampleDirector1,
                    AssociateDetails = { sampleAssociate1, sampleAssociate2 }, ClientIds = { "client-1", "client-2" }
                },
                new Roundtable {
                    Id = "rt-master-2", Name = "Master Beta Roundtable", Abbreviation = "M_BETA", Description = "The second roundtable in Master, tech focus.",
                    Status = RoundtableStatus.Active, ClientsWithAccessCount = 5, DirectorDetails = sampleDirector2,
                    AssociateDetails = { sampleAssociate3 }, ClientIds = { "client-3" }
                },
                new Roundtable {
                    Id = "rt-master-3", Name = "Master Gamma Roundtable", Abbreviation = "M_GAMMA", Description = "An inactive roundtable in Master.",
                    Status = RoundtableStatus.Inactive, ClientsWithAccessCount = 7, DirectorDetails = sampleDirector1,
                    AssociateDetails = { sampleAssociate1 }, ClientIds = { "client-1", "client-4" }
                }
                // Add more if needed
            };
        }
    }
}
