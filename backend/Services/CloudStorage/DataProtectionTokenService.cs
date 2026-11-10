
using Microsoft.AspNetCore.DataProtection;

namespace backend.Services.CloudStorage;

public class DataProtectionTokenService : ITokenProtectionService
{
    private readonly IDataProtector _protector;

    public DataProtectionTokenService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("Megh.GoogleTokens.v1");
    }

    public string Protect(string value)
    {
        return _protector.Protect(value);
    }

    public string Unprotect(string value)
    {
        return _protector.Unprotect(value);
    }
}
