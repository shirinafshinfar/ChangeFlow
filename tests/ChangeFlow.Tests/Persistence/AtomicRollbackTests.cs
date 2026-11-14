using System.Net;
using ChangeFlow.Api.Persistence;
using ChangeFlow.Tests.Fixtures;
using Xunit;
namespace ChangeFlow.Tests.Persistence;
public sealed class AtomicRollbackTests
{
    [Theory]
    [InlineData(MutationPoint.AfterMutation,"create")]
    [InlineData(MutationPoint.AfterLedger,"create")]
    [InlineData(MutationPoint.AfterMutation,"edit")]
    [InlineData(MutationPoint.AfterLedger,"edit")]
    [InlineData(MutationPoint.AfterMutation,"submit")]
    [InlineData(MutationPoint.AfterLedger,"submit")]
    [InlineData(MutationPoint.AfterMutation,"approve")]
    [InlineData(MutationPoint.AfterLedger,"request-changes")]
    public async Task RealHttpFailureRollsBackEntityAndResult(MutationPoint point,string operation)
    {
        using var f=new SqliteFixture();var id=f.Seed(operation is "approve" or "request-changes"?"PendingReview":"Draft");
        f.Gates.Callback=MutationFaults.Once(point);
        var r=operation switch{
            "create"=>await f.Send("POST","/api/change-requests","{\"title\":\"new\"}",key:"fault"),
            "edit"=>await f.Send("PUT",$"/api/change-requests/{id}","{\"title\":\"new\"}",key:"fault",version:1),
            _=>await f.Send("POST",$"/api/change-requests/{id}/{operation}",user:operation is "approve" or "request-changes"?"bob":"alice",key:"fault",version:1)
        };
        Assert.Equal(HttpStatusCode.ServiceUnavailable,r.StatusCode);Assert.NotNull(r.Headers.RetryAfter);
        Assert.Equal(1L,f.Scalar("SELECT COUNT(*) FROM ChangeRequest"));Assert.Equal(1L,f.Scalar("SELECT Version FROM ChangeRequest"));
        Assert.Equal("original",f.Scalar("SELECT Title FROM ChangeRequest"));Assert.Equal(DBNull.Value,f.Scalar("SELECT ReviewBaselineTitle FROM ChangeRequest"));
        Assert.Equal(0L,f.Scalar("SELECT COUNT(*) FROM OperationResult"));
    }
}
