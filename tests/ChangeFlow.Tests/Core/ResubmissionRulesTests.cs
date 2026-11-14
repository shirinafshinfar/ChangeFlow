using ChangeFlow.Core;
using Xunit;
namespace ChangeFlow.Tests.Core;
public sealed class ResubmissionRulesTests
{
    private static readonly ActorContext Actor=new("owner",Roles.Requester,1);
    private static ChangeRequest Returned()=>new("id","base","desc","reason","owner",RequestStatus.ChangesRequested,8,"base","desc","reason");
    [Theory]
    [InlineData("title")]
    [InlineData("description")]
    [InlineData("reason")]
    [InlineData("clear")]
    public void AnyEffectiveContentDifferencePermitsResubmission(string field)
    {
        var r=Returned();
        r=field switch{"title"=>r with{Title="base "},"description"=>r with{Description="new"},"reason"=>r with{Reason="new"},_=>r with{Description=null}};
        var next=ChangeRequestRules.Submit(Actor,r);
        Assert.Equal(RequestStatus.PendingReview,next.Status);Assert.Equal(9,next.Version);Assert.Equal("base",next.ReviewBaselineTitle);
    }
    [Fact]
    public void VersionAndEditingHistoryCannotReplaceEffectiveDifference()
    {
        var r=Returned();
        var unchanged=ChangeRequestRules.Replace(Actor,r,"base","desc","reason");
        Assert.Equal("CHANGES_REQUIRED",Assert.Throws<RuleViolation>(()=>ChangeRequestRules.Submit(Actor,unchanged)).Code);
        var changed=ChangeRequestRules.Replace(Actor,r,"new","desc","reason");
        var reverted=ChangeRequestRules.Replace(Actor,changed,"base","desc","reason");
        Assert.Equal("CHANGES_REQUIRED",Assert.Throws<RuleViolation>(()=>ChangeRequestRules.Submit(Actor,reverted)).Code);
        var nullBaseline=r with{Description=null,Reason=null,ReviewBaselineDescription=null,ReviewBaselineReason=null};
        var empty=ChangeRequestRules.Replace(Actor,nullBaseline,"base","","");
        Assert.Equal("CHANGES_REQUIRED",Assert.Throws<RuleViolation>(()=>ChangeRequestRules.Submit(Actor,empty)).Code);
    }
}
