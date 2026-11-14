namespace ChangeFlow.Core;
public static class ChangeRequestRules
{
    public static bool CanSee(ActorContext actor,ChangeRequest r)=>
        (actor.Has(Roles.Requester)&&r.RequesterId==actor.UserId)||
        (actor.Has(Roles.Reviewer)&&r.RequesterId!=actor.UserId&&r.Status==RequestStatus.PendingReview);
    public static ChangeRequest Create(ActorContext actor,string? title,string? description,string? reason)
    {
        if(!actor.Has(Roles.Requester))throw new RuleViolation("ROLE_REQUIRED");
        if(!ContentValidation.IsValid(title,description,reason))throw new RuleViolation("VALIDATION_FAILED");
        return new(Guid.NewGuid().ToString(),title!,Empty(description),Empty(reason),actor.UserId,RequestStatus.Draft,1);
    }
    public static ChangeRequest Replace(ActorContext actor,ChangeRequest current,string? title,string? description,string? reason)
    {
        if(!actor.Has(Roles.Requester))throw new RuleViolation("ROLE_REQUIRED");
        if(current.RequesterId!=actor.UserId)throw new RuleViolation("OPERATION_FORBIDDEN");
        if(current.Status==RequestStatus.Approved)throw new RuleViolation("INVALID_STATE");
        if(!ContentValidation.IsValid(title,description,reason))throw new RuleViolation("VALIDATION_FAILED");
        return current with {Title=title!,Description=Empty(description),Reason=Empty(reason),Version=checked(current.Version+1)};
    }
    public static ChangeRequest Submit(ActorContext actor,ChangeRequest current)
    {
        if(!actor.Has(Roles.Requester))throw new RuleViolation("ROLE_REQUIRED");
        if(current.RequesterId!=actor.UserId)throw new RuleViolation("OPERATION_FORBIDDEN");
        if(current.Status is not (RequestStatus.Draft or RequestStatus.ChangesRequested))throw new RuleViolation("INVALID_STATE");
        if(current.Status==RequestStatus.ChangesRequested&&
            current.Title==current.ReviewBaselineTitle&&
            Empty(current.Description)==Empty(current.ReviewBaselineDescription)&&
            Empty(current.Reason)==Empty(current.ReviewBaselineReason))throw new RuleViolation("CHANGES_REQUIRED");
        return current with{Status=RequestStatus.PendingReview,Version=checked(current.Version+1)};
    }
    public static ChangeRequest Review(ActorContext actor,ChangeRequest current,bool approve)
    {
        if(!actor.Has(Roles.Reviewer))throw new RuleViolation("ROLE_REQUIRED");
        if(current.RequesterId==actor.UserId)throw new RuleViolation("SELF_REVIEW_FORBIDDEN");
        if(current.Status!=RequestStatus.PendingReview)throw new RuleViolation("INVALID_STATE");
        return current with{Status=approve?RequestStatus.Approved:RequestStatus.ChangesRequested,Version=checked(current.Version+1),
            ReviewBaselineTitle=approve?current.ReviewBaselineTitle:current.Title,
            ReviewBaselineDescription=approve?current.ReviewBaselineDescription:current.Description,
            ReviewBaselineReason=approve?current.ReviewBaselineReason:current.Reason};
    }
    public static void DeleteDraft(ActorContext actor,ChangeRequest current)
    {
        if(!actor.Has(Roles.Requester))throw new RuleViolation("ROLE_REQUIRED");
        if(current.RequesterId!=actor.UserId)throw new RuleViolation("OPERATION_FORBIDDEN");
        if(current.Status!=RequestStatus.Draft)throw new RuleViolation("INVALID_STATE");
    }
    private static string? Empty(string? value)=>string.IsNullOrEmpty(value)?null:value;
}
