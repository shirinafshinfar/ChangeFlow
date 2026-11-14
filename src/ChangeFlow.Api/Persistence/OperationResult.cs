using System.Text.Json;
namespace ChangeFlow.Api.Persistence;
public sealed record OperationResult(int Status, string Json, string Code, string? TargetId=null, string? Location=null)
{
    internal static readonly JsonSerializerOptions JsonOptions=new(JsonSerializerDefaults.Web);
    public static OperationResult Receipt(int status,string id,string result,string? location=null) =>
        new(status,JsonSerializer.Serialize(new{requestId=id,result},JsonOptions),result,id,location);
    public static OperationResult Error(int status,string code) =>
        new(status,JsonSerializer.Serialize(new {type="about:blank",title=status==404?"Not found":status==400?"Invalid request":status==401?"Unauthorized":status==403?"Forbidden":status==503?"Service unavailable":"Request failed",status,code},JsonOptions),code);
}
public sealed record ContentInput(string? Title,string? Description,string? Reason);
public sealed record MutationRequest(string Operation,string? Target,long? ExpectedVersion,string Key,ContentInput? Content);
public enum MutationPoint { AfterSnapshot, BeforeWriterReservation, AfterWriterReservation, AfterMutation, AfterLedger, BeforeCommit, AfterCommit, BeforeReconcile }
public interface IMutationObserver { void Observe(MutationPoint point,MutationRequest request); }
