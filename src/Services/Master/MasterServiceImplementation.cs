using Grpc.Core;
using System.Threading.Tasks;
using SurveyService.Master; // This is the C# namespace from master.proto

namespace SurveyService.MasterImplementation // Suggested namespace for implementation
{
    public class MasterServiceImpl : MasterService.MasterServiceBase
    {
        // Placeholder for a logger, if needed in the future
        // private readonly ILogger<MasterServiceImpl> _logger;
        private readonly RoundtableLogic _roundtableLogic; // Added field

        // Constructor
        public MasterServiceImpl(/*ILogger<MasterServiceImpl> logger*/)
        {
            // _logger = logger;
            _roundtableLogic = new RoundtableLogic(); // Instantiate RoundtableLogic
        }

        // --- GetStatus RPC Implementation ---
        // This is the existing simple RPC method in master.proto
        public override Task<GetStatusResponse> GetStatus(GetStatusRequest request, ServerCallContext context)
        {
            return Task.FromResult(new GetStatusResponse { Status = "MasterService is Alive!" });
        }

        // Roundtable-related RPC implementations (GetRoundtable, ListRoundtables, CreateRoundtable)
        // will be added here later, delegating to RoundtableLogic.cs.
        // For now, they will throw RpcException(Status.Unimplemented) if called.

        public override Task<GetRoundtableResponse> GetRoundtable(GetRoundtableRequest request, ServerCallContext context)
        {
            return _roundtableLogic.GetRoundtable(request);
        }

        public override Task<ListRoundtablesResponse> ListRoundtables(ListRoundtablesRequest request, ServerCallContext context)
        {
            return _roundtableLogic.ListRoundtables(request);
        }

        public override Task<CreateRoundtableResponse> CreateRoundtable(CreateRoundtableRequest request, ServerCallContext context)
        {
            return _roundtableLogic.CreateRoundtable(request);
        }
    }
}
