using Grpc.Core;
using SurveyService.Community; // This is the generated namespace from community.proto
using System.Linq; // For Enumerable.Range and LINQ examples
using System.Threading.Tasks; // For Task
using System.Collections.Generic; // For List

// Assuming the namespace for the service implementation
namespace SurveyService.CommunityImplementation
{
    public class CommunityServiceImpl : SurveyService.Community.CommunityService.CommunityServiceBase
    {
        // Placeholder for a logger if you have one configured
        // private readonly ILogger<CommunityServiceImpl> _logger;
        // public CommunityServiceImpl(ILogger<CommunityServiceImpl> logger)
        // {
        //     _logger = logger;
        // }

        public CommunityServiceImpl()
        {
            // Default constructor
        }

        public override Task<ListRoundtablesResponse> ListRoundtables(ListRoundtablesRequest request, ServerCallContext context)
        {
            // _logger?.LogInformation("ListRoundtables called with request: {Request}", request);

            // 1. Validate inputs
            if (request.PageSize <= 0)
            {
                request.PageSize = 10; // Default page size
            }
            if (request.PageSize > 100) // Max page size
            {
                request.PageSize = 100;
            }

            // 2. Placeholder Data Source & Filtering Logic
            // In a real application, this would involve querying a database (e.g., using Entity Framework Core)
            // and applying filters based on request.SearchQuery and request.StatusFilter.
            // It would also involve calls to UserService to get director/associate names if only IDs are stored.

            var allRoundtables = GenerateSampleRoundtables(); // Replace with actual data access

            // Apply Search Query (simple example)
            var filteredRoundtables = allRoundtables;
            if (!string.IsNullOrEmpty(request.SearchQuery))
            {
                string query = request.SearchQuery.ToLower();
                filteredRoundtables = filteredRoundtables.Where(r =>
                    r.Name.ToLower().Contains(query) ||
                    (r.Director != null && r.Director.Name.ToLower().Contains(query)) || // Added null check for Director
                    (r.Associates != null && r.Associates.Any(a => a.Name.ToLower().Contains(query))) // Added null check for Associates
                ).ToList();
            }

            // Apply Status Filter
            if (request.StatusFilter != RoundtableStatus.All && request.StatusFilter != RoundtableStatus.RoundtableStatusUnspecified)
            {
                filteredRoundtables = filteredRoundtables.Where(r => r.Status == request.StatusFilter).ToList();
            }

            // 3. Pagination
            int totalItems = filteredRoundtables.Count;
            // For simplicity, page_token is not implemented in this placeholder.
            // Real pagination would use the page_token to fetch the correct slice of data.
            // If page_token is an offset (e.g. "10"), you'd skip that many.
            // If it's a cursor (e.g. last item's ID), you'd query for items after that ID.
            
            // Basic offset calculation if page_token is a simple integer offset.
            // This is still a placeholder for true cursor-based pagination.
            int skipAmount = 0;
            if (!string.IsNullOrEmpty(request.PageToken))
            {
                if (int.TryParse(request.PageToken, out int offset))
                {
                    skipAmount = offset;
                }
                // else, handle invalid page_token (e.g., log warning, default to 0)
            }

            var paginatedRoundtables = filteredRoundtables.Skip(skipAmount).Take(request.PageSize).ToList();

            var response = new ListRoundtablesResponse
            {
                TotalSize = totalItems,
            };
            response.Roundtables.AddRange(paginatedRoundtables);

            if ((skipAmount + paginatedRoundtables.Count) < totalItems)
            {
                // Placeholder for next_page_token. In a real scenario, this would be the 'skipAmount + request.PageSize' or a cursor.
                response.NextPageToken = (skipAmount + request.PageSize).ToString(); 
            }

            return Task.FromResult(response);
        }

        // Helper method to generate sample data (replace with actual data access)
        private List<Roundtable> GenerateSampleRoundtables()
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
                    Id = "rt-uuid-1", Name = "Alpha Roundtable", Director = sampleDirector1, Status = RoundtableStatus.Active, ClientsWithAccessCount = 10,
                    Associates = { sampleAssociate1, sampleAssociate2 }
                },
                new Roundtable
                {
                    Id = "rt-uuid-2", Name = "Beta Roundtable", Director = sampleDirector2, Status = RoundtableStatus.Active, ClientsWithAccessCount = 5,
                    Associates = { sampleAssociate3 }
                },
                new Roundtable
                {
                    Id = "rt-uuid-3", Name = "Gamma Roundtable", Director = sampleDirector1, Status = RoundtableStatus.Inactive, ClientsWithAccessCount = 7,
                    Associates = { sampleAssociate1 }
                },
                new Roundtable
                {
                    Id = "rt-uuid-4", Name = "Delta Project Circle", Director = sampleDirector2, Status = RoundtableStatus.Active, ClientsWithAccessCount = 12,
                    Associates = { sampleAssociate2, sampleAssociate3 }
                },
                new Roundtable // Adding more data for pagination testing
                {
                    Id = "rt-uuid-5", Name = "Epsilon Discussion Group", Director = sampleDirector1, Status = RoundtableStatus.Active, ClientsWithAccessCount = 3
                },
                new Roundtable
                {
                    Id = "rt-uuid-6", Name = "Zeta Forum", Director = sampleDirector2, Status = RoundtableStatus.Inactive, ClientsWithAccessCount = 8,
                    Associates = { sampleAssociate1, sampleAssociate3 }
                }
            };
        }
    }
}
