using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Recurly
{
    internal class HttpRequestFactory
    {
        private readonly string _authHeader;
        private static readonly HttpClient _httpClient;

        static HttpRequestFactory()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("X-Api-Version", "2.10"); // TODO: Configurable?
            var clientVersion = "Recurly C# Client v" + Assembly.GetExecutingAssembly().GetName().Version;
            _httpClient.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue.Parse(clientVersion));
        }

        public HttpRequestFactory(Uri recurlyApiBaseUri, string apiKey)
        {
            RecurlyApiBaseUri = recurlyApiBaseUri;
            _authHeader = $"Basic {Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(apiKey))}";
        }

        public Uri RecurlyApiBaseUri { get; }

        public async Task<RecurlyRequestResult> SendXmlGetRequestAsync(Uri requestUri, CancellationToken cancellation = default(CancellationToken))
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml"));
            request.Headers.Add("Authorization", _authHeader);

            var response = await _httpClient.SendAsync(request, cancellation).ConfigureAwait(false);
            return await ParseResponseAsync(response).ConfigureAwait(false);
        }

        public async Task<RecurlyRequestResult> SendXmlPostRequestAsync(Uri requstUri, Stream contentStream)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, requstUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/xml"));
            request.Headers.Add("Authorization", _authHeader);
            request.Content = new StreamContent(contentStream);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/xml; charset=utf-8");

            var originalTimeout = _httpClient.Timeout;
            _httpClient.Timeout = TimeSpan.FromSeconds(60);
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            _httpClient.Timeout = originalTimeout;

            return await ParseResponseAsync(response).ConfigureAwait(false);
        }

        private static async Task<RecurlyRequestResult> ParseResponseAsync(HttpResponseMessage response)
        {
            var statusCode = response.StatusCode;
            RecurlyException exception = null;
            XmlReader reader = null;
            switch(statusCode)
            {
                case HttpStatusCode.OK:
                    reader = await GetXmlReaderFromResponseAsync(response).ConfigureAwait(false);
                    break;

                case ValidationException.HttpStatusCode:
                case HttpStatusCode.PreconditionFailed:
                {
                    var errors = await ReadErrorsFromResponseAsync(response).ConfigureAwait(false);
                    exception = new ValidationException(errors);
                    break;
                }
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.Forbidden:
                {
                    var errors = await ReadErrorsFromResponseAsync(response).ConfigureAwait(false);
                    exception = new InvalidCredentialsException(errors);
                    break;
                }
                case HttpStatusCode.ServiceUnavailable:
                    exception = new TemporarilyUnavailableException();
                    break;
                case HttpStatusCode.InternalServerError:
                {
                    var errors = await ReadErrorsFromResponseAsync(response).ConfigureAwait(false);
                    exception = new ServerException(errors);
                    break;
                }

                default:
                {
                    var errors = await ReadErrorsFromResponseAsync(response).ConfigureAwait(false);
                    exception = new RecurlyException("Unhandled response status code", errors);
                    break;
                }
            }

            return new RecurlyRequestResult(statusCode, reader, exception);
        }

        private static async Task<Errors> ReadErrorsFromResponseAsync(HttpResponseMessage response)
        {
            using(var reader = await GetXmlReaderFromResponseAsync(response).ConfigureAwait(false))
            {
                return await Errors.ReadFromXmlAsync(reader).ConfigureAwait(false);
            }
        }

        private static async Task<XmlReader> GetXmlReaderFromResponseAsync(HttpResponseMessage response)
        {
            var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var reader = XmlReader.Create(responseStream);
            return reader;
        }

        internal Uri MakeRequestUri(params string[] urlParts)
        {
            if(urlParts?.Any() != true)
                throw new ArgumentException("urlParts must include at least one part");

            var result = RecurlyApiBaseUri;
            foreach(var part in urlParts)
            {
                result = new Uri(result, part);
            }

            return result;
        }
    }

    internal struct RecurlyRequestResult : IDisposable
    {
        public RecurlyRequestResult(HttpStatusCode statusCode, XmlReader responseReader, RecurlyException exception)
        {
            StatusCode = statusCode;
            ResponseReader = responseReader;
            Exception = exception;
        }

        public HttpStatusCode StatusCode { get; }
        public XmlReader ResponseReader { get; }
        public RecurlyException Exception { get; }

        public void Dispose()
        {
            ResponseReader?.Dispose();
        }
    }

    public class RecurlyClient
    {
        private readonly HttpRequestFactory _requestFactory;

        public RecurlyClient(string recurlySubdomain, string apiKey)
        {
            var recurlyBaseUri = new Uri($"https://{recurlySubdomain}.recurly.com/v2");
            _requestFactory = new HttpRequestFactory(recurlyBaseUri, apiKey);
            Accounts = new Accounts(_requestFactory);
        }

        public Accounts Accounts { get; }
    }
}