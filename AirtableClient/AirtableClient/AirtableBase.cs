using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Utf8Json;

namespace Airtable
{
    public class AirtableBase
    {
        AirtableClient client;
        string baseId;

        internal AirtableBase(AirtableClient client, string baseId)
        {
            this.client = client;
            this.baseId = baseId;
        }

        public async Task<T[]> LoadTableAsync<T>()
        {
            var url = $"https://api.airtable.com/v0/{baseId}/{typeof(T).Name}?api_key={client.ApiKey}";
            var message = await client.Get(url);

            if (message.StatusCode != HttpStatusCode.OK)
            {
                switch (message.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        throw new AirtableBadRequestException();
                    case HttpStatusCode.Forbidden:
                        throw new AirtableForbiddenException();
                    case HttpStatusCode.NotFound:
                        throw new AirtableNotFoundException();
                    case HttpStatusCode.PaymentRequired:
                        throw new AirtablePaymentRequiredException();
                    case HttpStatusCode.Unauthorized:
                        throw new AirtableUnauthorizedException();
                    case HttpStatusCode.RequestEntityTooLarge:
                        throw new AirtableRequestEntityTooLargeException();
                    case (HttpStatusCode)422:
                        var error = JsonSerializer.Deserialize<dynamic>(await message.Content.ReadAsByteArrayAsync());
                        throw new AirtableInvalidRequestException(error?["error"]?["message"]);
                    case (HttpStatusCode)429:
                        throw new AirtableTooManyRequestsException();
                    default:
                        throw new AirtableUnrecognizedException(message.StatusCode);
                }
            }

            var jsonBody = JsonSerializer.Deserialize<JsonBody<T>>(await message.Content.ReadAsByteArrayAsync());

            return jsonBody.Records.Select(x => x.Body).ToArray();
        }
    }

    public class JsonBody<T>
    {
        [DataMember(Name = "records")]
        public Record<T>[] Records;

        [DataMember(Name = "offset")]
        public string Offset;
    }

    public class Record<T>
    {
        [DataMember(Name = "id")]
        public string ID;

        [DataMember(Name = "fields")]
        public T Body;

        [DataMember(Name = "createdTime")]
        public string createdTime;
    }
}