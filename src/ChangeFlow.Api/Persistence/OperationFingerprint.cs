using System.Security.Cryptography;
using System.Text.Json;
namespace ChangeFlow.Api.Persistence;
public static class OperationFingerprint
{
    public static byte[] Compute(MutationRequest request) => SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(
        new {request.Operation,request.Target,request.ExpectedVersion,request.Content},OperationResult.JsonOptions));
}
