namespace ChangeFlow.Core;

[Flags]
public enum Roles { None = 0, Requester = 1, Reviewer = 2 }

public sealed record ActorContext(string UserId, Roles Roles, long AuthorizationVersion)
{
    public bool Has(Roles role) => (Roles & role) == role;
}
