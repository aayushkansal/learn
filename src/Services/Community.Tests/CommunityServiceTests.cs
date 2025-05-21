using Xunit;
using SurveyService.Community; // Namespace for request/response/enum messages
using SurveyService.CommunityImplementation; // Namespace for the service implementation
using Grpc.Core;
using System.Linq;
using System.Threading.Tasks; // For Task
using System.Collections.Generic; // For List

// Assuming the namespace for the tests
namespace SurveyService.Community.Tests
{
    public class CommunityServiceTests
    {
        private CommunityServiceImpl _service; // Service instance for each test
        private ServerCallContext _context; // Mock context

        public CommunityServiceTests()
        {
            // Re-initialize service for each test to ensure clean state for _roundtablesStorage
            _service = new CommunityServiceImpl(); 
            _context = new MockServerCallContext();
        }

        // MockServerCallContext for testing purposes (can be shared or nested)
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
            protected override AuthContext AuthContextCore => null;
        }

        // --- Existing ListRoundtables Tests (Keep As Is) ---
        [Fact]
        public async Task ListRoundtables_NoFilters_ReturnsDefaultPageOfAllItems()
        {
            var request = new ListRoundtablesRequest { PageSize = 2 };
            var response = await _service.ListRoundtables(request, _context);
            Assert.NotNull(response);
            Assert.Equal(6, response.TotalSize); // Updated sample size
            Assert.Equal(2, response.Roundtables.Count);
            Assert.NotEmpty(response.NextPageToken);
        }

        [Fact]
        public async Task ListRoundtables_FilterByStatus_Active()
        {
            var request = new ListRoundtablesRequest { PageSize = 10, StatusFilter = RoundtableStatus.Active };
            var response = await _service.ListRoundtables(request, _context);
            Assert.NotNull(response);
            Assert.True(response.Roundtables.All(r => r.Status == RoundtableStatus.Active));
            Assert.Equal(5, response.TotalSize); // 5 active items in updated sample
            Assert.Equal(5, response.Roundtables.Count);
        }

        [Fact]
        public async Task ListRoundtables_FilterByStatus_Inactive()
        {
            var request = new ListRoundtablesRequest { PageSize = 10, StatusFilter = RoundtableStatus.Inactive };
            var response = await _service.ListRoundtables(request, _context);
            Assert.NotNull(response);
            Assert.True(response.Roundtables.All(r => r.Status == RoundtableStatus.Inactive));
            Assert.Equal(1, response.TotalSize); 
            Assert.Single(response.Roundtables);
            Assert.Equal("Gamma Roundtable", response.Roundtables[0].Name);
        }
        
        [Fact]
        public async Task ListRoundtables_FilterByStatus_All()
        {
            var request = new ListRoundtablesRequest { PageSize = 10, StatusFilter = RoundtableStatus.All };
            var response = await _service.ListRoundtables(request, _context);
            Assert.NotNull(response);
            Assert.Equal(6, response.TotalSize); 
            Assert.Equal(6, response.Roundtables.Count);
        }

        [Fact]
        public async Task ListRoundtables_SearchByName_ReturnsMatching()
        {
            var request = new ListRoundtablesRequest { PageSize = 10, SearchQuery = "Alpha" };
            var response = await _service.ListRoundtables(request, _context);
            Assert.NotNull(response);
            Assert.Single(response.Roundtables);
            Assert.Equal("Alpha Roundtable", response.Roundtables[0].Name);
            Assert.Equal(1, response.TotalSize);
        }
        
        [Fact]
        public async Task ListRoundtables_SearchByAbbreviation_ReturnsMatching()
        {
            var request = new ListRoundtablesRequest { PageSize = 10, SearchQuery = "BETA" };
            var response = await _service.ListRoundtables(request, _context);
            Assert.NotNull(response);
            Assert.Single(response.Roundtables);
            Assert.Equal("Beta Roundtable", response.Roundtables[0].Name);
            Assert.Equal("BETA", response.Roundtables[0].Abbreviation);
            Assert.Equal(1, response.TotalSize);
        }

        [Fact]
        public async Task ListRoundtables_SearchByDirectorName_ReturnsMatching()
        {
            var request = new ListRoundtablesRequest { PageSize = 10, SearchQuery = "Dr. Alice" }; // Updated to match sample data
            var response = await _service.ListRoundtables(request, _context);
            Assert.NotNull(response);
            Assert.Equal(3, response.TotalSize); // Dr. Alice directs Alpha, Gamma, Epsilon
            Assert.Equal(3, response.Roundtables.Count);
        }
        
        [Fact]
        public async Task ListRoundtables_SearchByAssociateName_ReturnsMatching()
        {
            var request = new ListRoundtablesRequest { PageSize = 10, SearchQuery = "Diana Helper" }; // Updated to match sample data
            var response = await _service.ListRoundtables(request, _context);
            Assert.NotNull(response);
            Assert.Equal(2, response.TotalSize); // Diana is in Alpha and Delta
            Assert.Equal(2, response.Roundtables.Count);
        }

        [Fact]
        public async Task ListRoundtables_Search_NoResults()
        {
            var request = new ListRoundtablesRequest { PageSize = 10, SearchQuery = "NonExistent" };
            var response = await _service.ListRoundtables(request, _context);
            Assert.NotNull(response);
            Assert.Empty(response.Roundtables);
            Assert.Equal(0, response.TotalSize);
        }

        [Fact]
        public async Task ListRoundtables_PageSizeMaxEnforced()
        {
            var request = new ListRoundtablesRequest { PageSize = 200 };
            var response = await _service.ListRoundtables(request, _context);
            Assert.NotNull(response);
            Assert.Equal(6, response.TotalSize);
            Assert.Equal(6, response.Roundtables.Count); 
        }

        [Fact]
        public async Task ListRoundtables_PageSizeZeroDefaultsToTen() 
        {
            var request = new ListRoundtablesRequest { PageSize = 0 };
            var response = await _service.ListRoundtables(request, _context);
            Assert.NotNull(response);
            Assert.Equal(6, response.TotalSize);
            Assert.Equal(6, response.Roundtables.Count); 
        }

        // --- New Tests for CreateRoundtable ---

        [Fact]
        public async Task CreateRoundtable_Successful_WithRequiredFields()
        {
            var request = new CreateRoundtableRequest
            {
                Name = "New Test Roundtable",
                Abbreviation = "NTR",
                IsActive = true,
                DirectorUserIds = { "dir1" },
                PrimaryDirectorUserId = "dir1"
            };

            var response = await _service.CreateRoundtable(request, _context);

            Assert.NotNull(response);
            Assert.NotNull(response.Roundtable);
            Assert.Equal("New Test Roundtable", response.Roundtable.Name);
            Assert.Equal("NTR", response.Roundtable.Abbreviation);
            Assert.Equal(RoundtableStatus.Active, response.Roundtable.Status);
            Assert.Equal("dir1", response.Roundtable.Director.UserId);
            Assert.True(response.Roundtable.Director.IsPrimary);

            // Verify it's added to the list for subsequent List calls
            var listResponse = await _service.ListRoundtables(new ListRoundtablesRequest { SearchQuery = "NTR" }, _context);
            Assert.Single(listResponse.Roundtables);
        }

        [Fact]
        public async Task CreateRoundtable_Successful_WithAllFields()
        {
            var request = new CreateRoundtableRequest
            {
                Name = "Full Test Roundtable",
                Abbreviation = "FULL",
                IsActive = false,
                Description = "A full description.",
                ClientIds = { "clientA", "clientB" },
                DirectorUserIds = { "dir10", "dir11" },
                PrimaryDirectorUserId = "dir10",
                AssociateUserIds = { "assoc20", "assoc21" },
                PrimaryAssociateUserId = "assoc20"
            };

            var response = await _service.CreateRoundtable(request, _context);
            var rt = response.Roundtable;

            Assert.NotNull(rt);
            Assert.Equal("Full Test Roundtable", rt.Name);
            Assert.Equal("FULL", rt.Abbreviation);
            Assert.Equal(RoundtableStatus.Inactive, rt.Status);
            Assert.Equal("A full description.", rt.Description);
            Assert.Equal(2, rt.ClientIds.Count);
            Assert.Contains("clientA", rt.ClientIds);
            Assert.Equal("dir10", rt.Director.UserId);
            Assert.True(rt.Director.IsPrimary);
            Assert.Equal(2, rt.Associates.Count);
            var primaryAssociate = rt.Associates.First(a => a.UserId == "assoc20");
            Assert.True(primaryAssociate.IsPrimary);
            var secondaryAssociate = rt.Associates.First(a => a.UserId == "assoc21");
            Assert.False(secondaryAssociate.IsPrimary);
            Assert.Equal(2, rt.ClientsWithAccessCount);
        }

        [Fact]
        public async Task CreateRoundtable_Fail_MissingName()
        {
            var request = new CreateRoundtableRequest { Abbreviation = "FAIL", DirectorUserIds = { "d1" }, PrimaryDirectorUserId = "d1" };
            var exception = await Assert.ThrowsAsync<RpcException>(() => _service.CreateRoundtable(request, _context));
            Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
            Assert.Contains("Name is required", exception.Status.Detail);
        }

        [Fact]
        public async Task CreateRoundtable_Fail_MissingAbbreviation()
        {
            var request = new CreateRoundtableRequest { Name = "Fail RT", DirectorUserIds = { "d1" }, PrimaryDirectorUserId = "d1" };
            var exception = await Assert.ThrowsAsync<RpcException>(() => _service.CreateRoundtable(request, _context));
            Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
            Assert.Contains("Abbreviation is required", exception.Status.Detail);
        }
        
        [Fact]
        public async Task CreateRoundtable_Fail_MissingDirectors()
        {
            var request = new CreateRoundtableRequest { Name = "Fail RT", Abbreviation = "FAIL", PrimaryDirectorUserId = "d1" };
            var exception = await Assert.ThrowsAsync<RpcException>(() => _service.CreateRoundtable(request, _context));
            Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
            Assert.Contains("At least one Director must be selected", exception.Status.Detail);
        }

        [Fact]
        public async Task CreateRoundtable_Fail_MissingPrimaryDirector()
        {
            var request = new CreateRoundtableRequest { Name = "Fail RT", Abbreviation = "FAIL", DirectorUserIds = { "d1" } };
            var exception = await Assert.ThrowsAsync<RpcException>(() => _service.CreateRoundtable(request, _context));
            Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
            Assert.Contains("Primary Director is required", exception.Status.Detail);
        }
        
        [Fact]
        public async Task CreateRoundtable_Fail_PrimaryDirectorNotInSelectedDirectors()
        {
            var request = new CreateRoundtableRequest { Name = "Fail RT", Abbreviation = "FAIL", DirectorUserIds = { "d1" }, PrimaryDirectorUserId = "d2-not-selected" };
            var exception = await Assert.ThrowsAsync<RpcException>(() => _service.CreateRoundtable(request, _context));
            Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
            Assert.Contains("Primary Director must be one of the selected Directors", exception.Status.Detail);
        }

        [Fact]
        public async Task CreateRoundtable_Fail_PrimaryAssociateNotInSelectedAssociates()
        {
            var request = new CreateRoundtableRequest { 
                Name = "Fail RT", Abbreviation = "FAIL", 
                DirectorUserIds = { "d1" }, PrimaryDirectorUserId = "d1",
                AssociateUserIds = { "a1" }, PrimaryAssociateUserId = "a2-not-selected"
            };
            var exception = await Assert.ThrowsAsync<RpcException>(() => _service.CreateRoundtable(request, _context));
            Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
            Assert.Contains("Primary Associate must be one of the selected Associates", exception.Status.Detail);
        }


        [Fact]
        public async Task CreateRoundtable_Fail_DuplicateAbbreviation()
        {
            var initialRequest = new CreateRoundtableRequest 
            { 
                Name = "First RT", Abbreviation = "DUP", 
                DirectorUserIds = { "d1" }, PrimaryDirectorUserId = "d1" 
            };
            await _service.CreateRoundtable(initialRequest, _context); // Create first one

            var duplicateRequest = new CreateRoundtableRequest 
            { 
                Name = "Second RT", Abbreviation = "DUP", // Same abbreviation
                DirectorUserIds = { "d2" }, PrimaryDirectorUserId = "d2" 
            };
            var exception = await Assert.ThrowsAsync<RpcException>(() => _service.CreateRoundtable(duplicateRequest, _context));
            Assert.Equal(StatusCode.AlreadyExists, exception.StatusCode);
            Assert.Contains("Abbreviation 'DUP' already exists", exception.Status.Detail);
        }

        [Fact]
        public async Task CreateRoundtable_DefaultIsActiveTrue()
        {
            // Note: is_active defaults to false (proto3 default for bool) if not sent by client.
            // The user story says "checkbox should be checked by default", implying client sends true.
            // This test assumes client sends true.
            var request = new CreateRoundtableRequest
            {
                Name = "Default Active RT", Abbreviation = "DART",
                DirectorUserIds = { "d1" }, PrimaryDirectorUserId = "d1",
                IsActive = true // Explicitly set as per typical client behavior for "checked by default"
            };
            var response = await _service.CreateRoundtable(request, _context);
            Assert.Equal(RoundtableStatus.Active, response.Roundtable.Status);
        }
    }
}
