using ChangeFlow.Core;
using ChangeFlow.Api.Persistence;
namespace ChangeFlow.Api.Application;
public sealed class ContentCommands(MutationExecutor executor)
{
    public static OperationResult Failure(RuleViolation e)=>OperationResult.Error(e.Code switch {
        "VALIDATION_FAILED"=>400,"ROLE_REQUIRED" or "OPERATION_FORBIDDEN" or "SELF_REVIEW_FORBIDDEN"=>403,
        "VERSION_CONFLICT"=>412,_=>409
    },e.Code);
    public OperationResult Execute(ActorContext actor,MutationRequest request)=>executor.Execute(actor,request,(c,tx)=>{
        try {
            if(request.Operation=="Create") {
                var content=request.Content!;
                var r=ChangeRequestRules.Create(actor,content.Title,content.Description,content.Reason);
                ChangeRequestStore.Insert(c,tx,r);return OperationResult.Receipt(201,r.Id,"Created","/api/change-requests/"+r.Id);
            }
            var current=ChangeRequestStore.Find(c,tx,request.Target!);
            if(current is null||!ChangeRequestRules.CanSee(actor,current))return OperationResult.Error(404,"NOT_FOUND");
            if(current.RequesterId!=actor.UserId)return OperationResult.Error(403,"OPERATION_FORBIDDEN");
            if(current.Version!=request.ExpectedVersion)return OperationResult.Error(412,"VERSION_CONFLICT");
            var input=request.Content!;
            var next=ChangeRequestRules.Replace(actor,current,input.Title,input.Description,input.Reason);
            ChangeRequestStore.Update(c,tx,next,current.Version);
            return OperationResult.Receipt(200,current.Id,"Updated");
        } catch(RuleViolation error){return Failure(error);}
    });
}
