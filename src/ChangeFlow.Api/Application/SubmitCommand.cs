using ChangeFlow.Api.Persistence;
using ChangeFlow.Core;
namespace ChangeFlow.Api.Application;
public sealed class SubmitCommand(MutationExecutor executor)
{
    public OperationResult Execute(ActorContext actor,MutationRequest request)=>executor.Execute(actor,request,(c,tx)=>{
        var current=ChangeRequestStore.Find(c,tx,request.Target!);
        if(current is null||!ChangeRequestRules.CanSee(actor,current))return OperationResult.Error(404,"NOT_FOUND");
        if(current.RequesterId!=actor.UserId)return OperationResult.Error(403,"OPERATION_FORBIDDEN");
        if(current.Version!=request.ExpectedVersion)return OperationResult.Error(412,"VERSION_CONFLICT");
        try {
            var next=ChangeRequestRules.Submit(actor,current);ChangeRequestStore.Update(c,tx,next,current.Version);
            return OperationResult.Receipt(200,current.Id,"Submitted");
        } catch(RuleViolation e){return ContentCommands.Failure(e);}
    });
}
