
namespace backend.Services.CloudStorage;

public interface ITokenProtectionService
{
    string Protect(string value);
    string Unprotect(string value);
}
