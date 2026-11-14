namespace ChangeFlow.Core;
public enum RequestStatus { Draft,PendingReview,ChangesRequested,Approved }
public sealed record ChangeRequest(
    string Id,string Title,string? Description,string? Reason,string RequesterId,RequestStatus Status,long Version,
    string? ReviewBaselineTitle=null,string? ReviewBaselineDescription=null,string? ReviewBaselineReason=null);
public sealed class RuleViolation(string code):Exception(code) { public string Code {get;}=code; }
