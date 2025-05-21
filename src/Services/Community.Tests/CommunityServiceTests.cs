using Xunit;
using SurveyService.Community; // Namespace for request/response/enum messages
using SurveyService.CommunityImplementation; // Namespace for the service implementation
using Grpc.Core;
using System.Linq;
using System.Threading.Tasks; // For Task

// Assuming the namespace for the tests
namespace SurveyService.Community.Tests
{
    public class CommunityServiceTests
    {
        private readonly CommunityServiceImpl _service;

        public CommunityServiceTests()
        {
            // In a real scenario, you might inject dependencies like loggers or mock data sources.
            // For this service with self-contained sample data, direct instantiation is fine.
            _service = new CommunityServiceImpl();
        }

        private ServerCallContext GetTestServerCallContext()
        {
            // This is a mock ServerCallContext. For more complex scenarios,
            // you might need a more sophisticated mock.
            return new MockServerCallContext();
        }

        // MockServerCallContext for testing purposes
        public class MockServerCallContext : ServerCallContext
        {
            protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders) => Task.CompletedTask;
            protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions options) => null;
            protected override string MethodCore => "TestMethod";
            protected override string HostCore => "TestHost";
            protected override string PeerCore => "TestPeer";
            protected override System.DateTime DeadlineCore => System.DateTime.UtcNow.AddHours(1);
            protected override Metadata RequestHeadersCore => new Metadata();
            protected override System.Threading.CancellationToken CancellationTokenCore => System.Threading.CancellationToken.None;
            protected override Metadata ResponseTrailersCore => new Metadata();
            protected override Status StatusCore { get; set; }
            protected override WriteOptions WriteOptionsCore { get; set; }
            protected override AuthContext AuthContextCore => null; // Or a mock AuthContext if needed
        }


        [Fact]
        public async Task ListRoundtables_NoFilters_ReturnsDefaultPageOfAllItems()
        {
            var request = new ListRoundtablesRequest { PageSize = 2 }; // Expecting 2 items
            var context = GetTestServerCallContext();

            var response = await _service.ListRoundtables(request, context);

            Assert.NotNull(response);
            Assert.Equal(6, response.TotalSize); // Total sample items from CommunityServiceImplementation
            Assert.Equal(2, response.Roundtables.Count);
            Assert.NotEmpty(response.NextPageToken); // Expecting more pages
        }

        [Fact]
        public async Task ListRoundtables_FilterByStatus_Active()
        {
            var request = new ListRoundtablesRequest { PageSize = 10, StatusFilter = RoundtableStatus.Active };
            var context = GetTestServerCallContext();

            var response = await _service.ListRoundtables(request, context);

            Assert.NotNull(response);
            Assert.True(response.Roundtables.All(r => r.Status == RoundtableStatus.Active));
            Assert.Equal(4, response.TotalSize); // 4 active items in sample
            Assert.Equal(4, response.Roundtables.Count);
        }

        [Fact]
        public async Task ListRoundtables_FilterByStatus_Inactive()
        {
            var request = new ListRoundtablesRequest { PageSize = 10, StatusFilter = RoundtableStatus.Inactive };
            var context = GetTestServerCallContext();

            var response = await _service.ListRoundtables(request, context);

            Assert.NotNull(response);
            Assert.True(response.Roundtables.All(r => r.Status == RoundtableStatus.Inactive));
            Assert.Equal(2, response.TotalSize); // 2 inactive items in sample
            Assert.Equal(2, response.Roundtables.Count);
            // Assert.Equal("Gamma Roundtable", response.Roundtables[0].Name); // Order isn't guaranteed, check presence
            Assert.Contains(response.Roundtables, r => r.Name == "Gamma Roundtable");
            Assert.Contains(response.Roundtables, r => r.Name == "Zeta Forum");
        }
        
        [Fact]
        public async Task ListRoundtables_FilterByStatus_All()
        {
            var request = new ListRoundtablesRequest { PageSize = 10, StatusFilter = RoundtableStatus.All };
            var context = GetTestServerCallContext();

            var response = await _service.ListRoundtables(request, context);

            Assert.NotNull(response);
            Assert.Equal(6, response.TotalSize); 
            Assert.Equal(6, response.Roundtables.Count);
        }

        [Fact]
        public async Task ListRoundtables_SearchByName_ReturnsMatching()
        {
            var request = new ListRoundtablesRequest { PageSize = 10, SearchQuery = "Alpha" };
            var context = GetTestServerCallContext();

            var response = await _service.ListRoundtables(request, context);

            Assert.NotNull(response);
            Assert.Single(response.Roundtables);
            Assert.Equal("Alpha Roundtable", response.Roundtables[0].Name);
            Assert.Equal(1, response.TotalSize);
        }

        [Fact]
        public async Task ListRoundtables_SearchByDirectorName_ReturnsMatching()
        {
            // Dr. Alice Director is on Alpha, Gamma, Epsilon
            var request = new ListRoundtablesRequest { PageSize = 10, SearchQuery = "Dr. Alice Director" };
            var context = GetTestServerCallContext();

            var response = await _service.ListRoundtables(request, context);

            Assert.NotNull(response);
            Assert.Equal(3, response.TotalSize); 
            Assert.Equal(3, response.Roundtables.Count);
            Assert.Contains(response.Roundtables, r => r.Name == "Alpha Roundtable");
            Assert.Contains(response.Roundtables, r => r.Name == "Gamma Roundtable");
            Assert.Contains(response.Roundtables, r => r.Name == "Epsilon Discussion Group");
        }
        
        [Fact]
        public async Task ListRoundtables_SearchByAssociateName_ReturnsMatching()
        {
            // Diana Helper is in Alpha and Delta
            var request = new ListRoundtablesRequest { PageSize = 10, SearchQuery = "Diana Helper" };
            var context = GetTestServerCallContext();

            var response = await _service.ListRoundtables(request, context);

            Assert.NotNull(response);
            Assert.Equal(2, response.TotalSize); 
            Assert.Equal(2, response.Roundtables.Count);
            Assert.Contains(response.Roundtables, r => r.Name == "Alpha Roundtable");
            Assert.Contains(response.Roundtables, r => r.Name == "Delta Project Circle");
        }

        [Fact]
        public async Task ListRoundtables_Search_NoResults()
        {
            var request = new ListRoundtablesRequest { PageSize = 10, SearchQuery = "NonExistent" };
            var context = GetTestServerCallContext();

            var response = await _service.ListRoundtables(request, context);

            Assert.NotNull(response);
            Assert.Empty(response.Roundtables);
            Assert.Equal(0, response.TotalSize);
            Assert.Empty(response.NextPageToken);
        }

        [Fact]
        public async Task ListRoundtables_Pagination_PageSizeAndNextToken()
        {
            var request1 = new ListRoundtablesRequest { PageSize = 1 }; // First page, 1 item
            var context = GetTestServerCallContext();
            var response1 = await _service.ListRoundtables(request1, context);

            Assert.NotNull(response1);
            Assert.Single(response1.Roundtables);
            Assert.Equal(6, response1.TotalSize); // Total sample items
            Assert.NotEmpty(response1.NextPageToken); // Expect next page

            // Test fetching the "next" page using the token.
            // The service's page_token is just the skipAmount as string.
            var request2 = new ListRoundtablesRequest { PageSize = 1, PageToken = response1.NextPageToken };
            var response2 = await _service.ListRoundtables(request2, context);

            Assert.NotNull(response2);
            Assert.Single(response2.Roundtables);
            Assert.Equal(6, response2.TotalSize);
            Assert.NotEqual(response1.Roundtables[0].Id, response2.Roundtables[0].Id); // Ensure it's a different item
            Assert.NotEmpty(response2.NextPageToken);
        }
        
        [Fact]
        public async Task ListRoundtables_PageSizeMaxEnforced()
        {
            var request = new ListRoundtablesRequest { PageSize = 200 }; // Exceeds max of 100
            var context = GetTestServerCallContext();

            var response = await _service.ListRoundtables(request, context);
            
            Assert.NotNull(response);
            Assert.Equal(6, response.TotalSize); // Total sample items
            // PageSize should be capped at 100, but since we only have 6 items, we get 6.
            Assert.Equal(6, response.Roundtables.Count); 
        }

        [Fact]
        public async Task ListRoundtables_PageSizeZeroDefaultsToTen() 
        {
            var request = new ListRoundtablesRequest { PageSize = 0 }; // Should default to 10
            var context = GetTestServerCallContext();

            var response = await _service.ListRoundtables(request, context);
            
            Assert.NotNull(response);
            Assert.Equal(6, response.TotalSize); // Total sample items
            // Since PageSize defaults to 10, and we have 6 items, we expect 6 items.
            Assert.Equal(6, response.Roundtables.Count); 
        }

        [Fact]
        public async Task ListRoundtables_Pagination_GetAllItemsInPages()
        {
            var context = GetTestServerCallContext();
            var collectedIds = new System.Collections.Generic.HashSet<string>();
            string nextPageToken = null;
            int itemsPerPage = 2;
            int totalItemsFetched = 0;

            do
            {
                var request = new ListRoundtablesRequest { PageSize = itemsPerPage, PageToken = nextPageToken };
                var response = await _service.ListRoundtables(request, context);

                Assert.NotNull(response);
                Assert.True(response.Roundtables.Count <= itemsPerPage);

                foreach (var rt in response.Roundtables)
                {
                    Assert.DoesNotContain(rt.Id, collectedIds); // Ensure no duplicates
                    collectedIds.Add(rt.Id);
                }
                totalItemsFetched += response.Roundtables.Count;
                nextPageToken = response.NextPageToken;

                if (string.IsNullOrEmpty(nextPageToken))
                {
                    Assert.Equal(response.TotalSize, totalItemsFetched);
                }

            } while (!string.IsNullOrEmpty(nextPageToken));

            Assert.Equal(6, totalItemsFetched); // Check if all items were fetched
            Assert.Equal(6, collectedIds.Count);
        }
    }
}
