namespace ChangeFlow.Api.Persistence;
internal sealed record UserRecord(string Id, bool Enabled, int RolesMask, long AuthorizationVersion);
internal sealed record AccessTokenRecord(byte[] TokenHash, string UserId, bool Enabled);
