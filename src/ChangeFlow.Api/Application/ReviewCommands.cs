using ChangeFlow.Api.Persistence;
using ChangeFlow.Core;
namespace ChangeFlow.Api.Application;
public sealed class ReviewCommands(MutationExecutor executor)
{
    public OperationResult Execute(ActorContext actor,MutationRequest request)=>executor.Execute(actor,request,(c,tx)=>{
        var current=ChangeRequestStore.Find(c,tx,request.Target!);
        if(current is null||!ChangeRequestRules.CanSee(actor,current))return OperationResult.Error(404,"NOT_FOUND");
        if(current.RequesterId==actor.UserId)return OperationResult.Error(403,"SELF_REVIEW_FORBIDDEN");
        if(current.Version!=request.ExpectedVersion)return OperationResult.Error(412,"VERSION_CONFLICT");
        try {
            var next=ChangeRequestRules.Review(actor,current,request.Operation=="Approve");
            ChangeRequestStore.Update(c,tx,next,current.Version);
            return OperationResult.Receipt(200,current.Id,request.Operation=="Approve"?"Approved":"ChangesRequested");
        } catch(RuleViolation error){return ContentCommands.Failure(error);}
    });
}
