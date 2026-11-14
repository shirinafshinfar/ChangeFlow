using ChangeFlow.Core;
using ChangeFlow.Api.Persistence;
namespace ChangeFlow.Api.Application;
public sealed class DeleteDraft(MutationExecutor executor)
{
    public OperationResult Execute(ActorContext actor,MutationRequest request)=>executor.Execute(actor,request,(c,tx)=>{
        try {
            var current=ChangeRequestStore.Find(c,tx,request.Target!);
            if(current is null||!ChangeRequestRules.CanSee(actor,current))return OperationResult.Error(404,"NOT_FOUND");
            if(current.RequesterId!=actor.UserId)return OperationResult.Error(403,"OPERATION_FORBIDDEN");
            if(current.Version!=request.ExpectedVersion)return OperationResult.Error(412,"VERSION_CONFLICT");
            ChangeRequestRules.DeleteDraft(actor,current);
            ChangeRequestStore.Delete(c,tx,current.Id,current.Version);
            return OperationResult.Receipt(200,current.Id,"Deleted");
        } catch(RuleViolation error){return ContentCommands.Failure(error);}
    });
}
