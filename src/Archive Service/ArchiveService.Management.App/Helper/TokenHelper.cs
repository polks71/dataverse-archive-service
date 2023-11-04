using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace ArchiveService.Management.App.Helper
{
    internal static class TokenHelper
    {
        public static async Task<string> GetToken(string environment, DefaultAzureCredential credential, IMemoryCache cache)
        {
            var accessToken = await cache.GetOrCreateAsync(environment, async (cacheEntry) => {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20);
                var token = credential.GetToken(new TokenRequestContext(new[] { $"{environment}" }));
                return token;
            });
            return accessToken.Token;
        }
    }
}
