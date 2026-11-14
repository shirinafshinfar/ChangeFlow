using ChangeFlow.Api.Persistence;
namespace ChangeFlow.Tests.Fixtures;
public sealed class OperationGates:IMutationObserver
{
    public Action<MutationPoint,MutationRequest>? Callback {get;set;}
    public void Observe(MutationPoint point,MutationRequest request)=>Callback?.Invoke(point,request);
}
