using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Airtable
{
    public class AirtableClient : IDisposable
    {
        internal string ApiKey { get; }

        readonly HttpClient httpClient = new HttpClient();

        public AirtableClient(string apiKey)
        {
            this.ApiKey = apiKey;
        }

        public void Dispose()
        {
            httpClient.Dispose();
        }

        public AirtableBase GetBase(string baseId)
        {
            return new AirtableBase(this, baseId);
        }

        internal Task<HttpResponseMessage> Get(string url)
        {
            return httpClient.GetAsync(url);
        }
    }
}
