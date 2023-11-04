using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Graph;
using System.Data.Common;

namespace ArchiveService.Management.App.Data
{
    public class AadAuthenticationInterceptor : DbConnectionInterceptor
    {
        private DateTimeOffset? _tokenexpiry = null;
        private Azure.Core.AccessToken _token;

        public AadAuthenticationInterceptor (Azure.Core.AccessToken token)
        {
            _token = token;
            _tokenexpiry = token.ExpiresOn;
        }

        public AadAuthenticationInterceptor()
        { }


        public override InterceptionResult ConnectionOpening(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result)
        {
            var sqlConnection = (SqlConnection)connection;

            //var provider = new AzureServiceTokenProvider();
            //// Note: in some situations the access token may not be cached automatically the Azure Token Provider.
            //// Depending on the kind of token requested, you may need to implement your own caching here.
            //sqlConnection.AccessToken = await provider.GetAccessTokenAsync("https://database.windows.net/", null, cancellationToken);

            if (!_tokenexpiry.HasValue
                || (_tokenexpiry.HasValue && _tokenexpiry <= DateTime.UtcNow))
            {
                var credential = new Azure.Identity.DefaultAzureCredential();
                _token = credential.GetToken(new Azure.Core.TokenRequestContext(new[] { "https://database.windows.net/.default" }));
                _tokenexpiry = _token.ExpiresOn;
            }
            sqlConnection.AccessToken = _token.Token;

            return result;
        }

        public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            var sqlConnection = (SqlConnection)connection;

            //var provider = new AzureServiceTokenProvider();
            //// Note: in some situations the access token may not be cached automatically the Azure Token Provider.
            //// Depending on the kind of token requested, you may need to implement your own caching here.
            //sqlConnection.AccessToken = await provider.GetAccessTokenAsync("https://database.windows.net/", null, cancellationToken);
            
            if (!_tokenexpiry.HasValue
                || (_tokenexpiry.HasValue && _tokenexpiry <= DateTime.UtcNow))
            {
                var credential = new Azure.Identity.DefaultAzureCredential();
                _token = await credential.GetTokenAsync(new Azure.Core.TokenRequestContext(new[] { "https://database.windows.net/.default" }));
                _tokenexpiry = _token.ExpiresOn;
            }
            sqlConnection.AccessToken = _token.Token;

            return result;
        }
    }
}
