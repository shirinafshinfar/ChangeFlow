using System.Net;
using ChangeFlow.Api.Persistence;
using ChangeFlow.Tests.Fixtures;
using Xunit;
namespace ChangeFlow.Tests.Persistence;
public sealed class CommitRecoveryTests
{
    [Fact]
    public async Task LostResponseAfterRealCommitReconcilesWithoutSecondDml()
    {
        using var f=new SqliteFixture();var id=f.Seed();var writes=0;
        f.Gates.Callback=(p,_)=>{if(p==MutationPoint.AfterMutation)writes++;if(p==MutationPoint.AfterCommit)throw new IOException("response lost");};
        var result=await f.Send("POST",$"/api/change-requests/{id}/submit",key:"lost",version:1);
        Assert.Equal(HttpStatusCode.OK,result.StatusCode);Assert.Equal(1,writes);
        Assert.Equal(2L,f.Scalar("SELECT Version FROM ChangeRequest"));Assert.Equal(1L,f.Scalar("SELECT COUNT(*) FROM OperationResult"));
    }
    [Fact]
    public async Task UnreadableReconciliationReturns503AndLaterSameKeyRecoversCommit()
    {
        using var f=new SqliteFixture();var id=f.Seed();
        f.Gates.Callback=(p,_)=>{if(p is MutationPoint.AfterCommit or MutationPoint.BeforeReconcile)throw new IOException("unreadable");};
        var uncertain=await f.Send("POST",$"/api/change-requests/{id}/submit",key:"uncertain",version:1);
        Assert.Equal(HttpStatusCode.ServiceUnavailable,uncertain.StatusCode);Assert.NotNull(uncertain.Headers.RetryAfter);
        f.Gates.Callback=null;
        Assert.Equal(HttpStatusCode.OK,(await f.Send("POST",$"/api/change-requests/{id}/submit",key:"uncertain",version:1)).StatusCode);
        Assert.Equal(2L,f.Scalar("SELECT Version FROM ChangeRequest"));Assert.Equal(1L,f.Scalar("SELECT COUNT(*) FROM OperationResult"));
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RealBusyBeginOrCommitRollsBackAndAllowsSameKeyRetry(bool atCommit)
    {
        using var f=new SqliteFixture();var id=f.Seed();
        using var blocker=f.Database.Open();
        using var hold=atCommit?blocker.BeginTransaction(deferred:true):blocker.BeginTransaction(deferred:false);
        using var read=blocker.CreateCommand();read.Transaction=hold;read.CommandText="SELECT COUNT(*) FROM ChangeRequest";read.ExecuteScalar();
        var result=await f.Send("POST",$"/api/change-requests/{id}/submit",key:"busy",version:1);
        Assert.Equal(HttpStatusCode.ServiceUnavailable,result.StatusCode);Assert.NotNull(result.Headers.RetryAfter);
        hold.Rollback();
        Assert.Equal(1L,f.Scalar("SELECT Version FROM ChangeRequest"));Assert.Equal(0L,f.Scalar("SELECT COUNT(*) FROM OperationResult"));
        Assert.Equal(HttpStatusCode.OK,(await f.Send("POST",$"/api/change-requests/{id}/submit",key:"busy",version:1)).StatusCode);
        Assert.Equal(2L,f.Scalar("SELECT Version FROM ChangeRequest"));Assert.Equal(1L,f.Scalar("SELECT COUNT(*) FROM OperationResult"));
    }
}
