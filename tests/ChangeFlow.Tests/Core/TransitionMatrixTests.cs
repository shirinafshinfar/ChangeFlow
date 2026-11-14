using ChangeFlow.Core;
using Xunit;
namespace ChangeFlow.Tests.Core;
public sealed class TransitionMatrixTests
{
    [Theory]
    [InlineData(RequestStatus.Draft)]
    [InlineData(RequestStatus.PendingReview)]
    [InlineData(RequestStatus.ChangesRequested)]
    [InlineData(RequestStatus.Approved)]
    public void EveryStateAndOperationCombinationHonorsTheWorkflow(RequestStatus status)
    {
        var owner=new ActorContext("owner",Roles.Requester,1);var reviewer=new ActorContext("other",Roles.Reviewer,1);
        var current=new ChangeRequest("id","changed",null,null,"owner",status,1,"baseline",null,null);
        void Verify(bool allowed,Func<ChangeRequest> action,RequestStatus next){
            if(allowed){var r=action();Assert.Equal(next,r.Status);Assert.Equal(2,r.Version);Assert.Equal(current.RequesterId,r.RequesterId);}
            else Assert.Throws<RuleViolation>(()=>action());
        }
        Verify(status!=RequestStatus.Approved,()=>ChangeRequestRules.Replace(owner,current,"edit",null,null),status);
        Verify(status is RequestStatus.Draft or RequestStatus.ChangesRequested,()=>ChangeRequestRules.Submit(owner,current),RequestStatus.PendingReview);
        Verify(status==RequestStatus.PendingReview,()=>ChangeRequestRules.Review(reviewer,current,true),RequestStatus.Approved);
        Verify(status==RequestStatus.PendingReview,()=>ChangeRequestRules.Review(reviewer,current,false),RequestStatus.ChangesRequested);
        if(status==RequestStatus.Draft)ChangeRequestRules.DeleteDraft(owner,current);
        else Assert.Throws<RuleViolation>(()=>ChangeRequestRules.DeleteDraft(owner,current));
        Assert.Equal(1,current.Version);Assert.Equal(status,current.Status);
    }
}
