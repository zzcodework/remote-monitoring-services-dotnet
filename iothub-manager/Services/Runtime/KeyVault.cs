using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Diagnostics;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Runtime
{
    public class KeyVault {
        
        // Key Vault details and access
        private readonly string name;
        private readonly string clientId;
        private readonly string clientSecret;

        // Key Vault Client
        private readonly KeyVaultClient keyVaultClient;

        // Constants
        private const string KEY_VAULT_URI = "https://{0}.vault.azure.net/secrets/{1}";

        public KeyVault(string name, string clientId, string clientSecret)
        {
            this.name = name;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.keyVaultClient = new KeyVaultClient(
                                    new KeyVaultClient.AuthenticationCallback(this.GetToken));
        }

        public string GetKeyVaultSecret(string secretKey)
        {
            secretKey = secretKey.Split(':').Last();
            var uri = string.Format(KEY_VAULT_URI, this.name, secretKey);

            try
            {
                return this.keyVaultClient.GetSecretAsync(uri).Result.Value;
            }
            catch (Exception)
            {
                return null;
            }
        }

        //the method that will be provided to the KeyVaultClient
        private async Task<string> GetToken(string authority, string resource, string scope)
        {
            var authContext = new AuthenticationContext(authority);
            ClientCredential clientCred = new ClientCredential(this.clientId, this.clientSecret);
            AuthenticationResult result = await authContext.AcquireTokenAsync(resource, clientCred);

            if (result == null)
                throw new System.InvalidOperationException("Failed to obtain the JWT token");

            return result.AccessToken;
        }
    }
}