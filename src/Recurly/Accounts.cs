using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml;

namespace Recurly
{
    public class Accounts
    {
        private const string URL_PREFIX = "/accounts/";
        private readonly HttpRequestFactory _requestFactory;

        internal Accounts(HttpRequestFactory requestFactory)
        {
            _requestFactory = requestFactory;
        }

        public async Task<Account> GetAsync(string accountCode)
        {
            var requestUri = _requestFactory.MakeRequestUri(URL_PREFIX, Uri.EscapeDataString(accountCode));
            using(var result = await _requestFactory.SendXmlGetRequestAsync(requestUri).ConfigureAwait(false))
            {
                switch(result.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        return Account.NotFound;

                    case HttpStatusCode.OK:
                        var account = await Account.CreateFromReaderAsync(result.ResponseReader, requestUri).ConfigureAwait(false);
                        return account;

                    default:
                        throw result.Exception ?? new Exception("Not sure what happened!");
                }
            }
        }

        public async Task<Account> CreateAsync(Account account)
        {
            var memoryStream = new MemoryStream();
            var xmlWriter = XmlWriter.Create(memoryStream);
            account.WriteXml(xmlWriter, "account");

            var requestUri = _requestFactory.MakeRequestUri(URL_PREFIX);
            var result = await _requestFactory.SendXmlPostRequestAsync(requestUri, memoryStream).ConfigureAwait(false);
            if(result.StatusCode == HttpStatusCode.OK)
                return await Account.CreateFromReaderAsync(result.ResponseReader, requestUri).ConfigureAwait(false);

            throw result.Exception ?? new Exception("Not sure what happened!");
        }

        internal static Uri MakeAccountUri(string accountCode)
        {
            return new Uri($"{URL_PREFIX}{accountCode}", UriKind.Relative);
        }
    }
}