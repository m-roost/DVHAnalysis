// NOTE: The original source code of this component was lost. This file was recovered from the
// compiled assembly UMRO.Aria.Access.Rest.dll (version 1.0.0.0) by decompilation (ILSpy) in September 2026.
// Copyright (C) The Regents of the University of Michigan. Licensed under GPL-3.0 (see LICENSE.txt).

using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UMRO.Aria.Access.Rest
{
    public class AriaAccessClient
    {
        private readonly string _address;

        private readonly string _username;

        private readonly string _password;

        private readonly string _apiKey;

        public AriaAccessClient(string address, string username, string password, string apiKey)
        {
            _address = address;
            _username = username;
            _password = password;
            _apiKey = apiKey;
        }

        public async Task SendRequestAsync(Request request)
        {
            using (HttpClient httpClient = CreateHttpClient())
            {
                HttpRequestMessage request2 = CreateHttpRequest(request);
                await ThrowIfNotSuccessful(await httpClient.SendAsync(request2));
            }
        }

        private HttpClient CreateHttpClient()
        {
            return new HttpClient(new HttpClientHandler
            {
                Credentials = new NetworkCredential(_username, _password)
            });
        }

        private HttpRequestMessage CreateHttpRequest(Request request)
        {
            return new HttpRequestMessage(HttpMethod.Post, _address)
            {
                Headers = { { "ApiKey", _apiKey } },
                Content = GetContent(request)
            };
        }

        private StringContent GetContent(Request request)
        {
            return new StringContent(request.ToJson(), Encoding.UTF8, "application/json");
        }

        private async Task ThrowIfNotSuccessful(HttpResponseMessage httpResponse)
        {
            string text = await httpResponse.Content.ReadAsStringAsync();
            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new AriaAccessException($"The HTTP request was not successful.\n{httpResponse}\n{text}");
            }
            ApplicationError applicationError = JsonConvert.DeserializeObject<ApplicationError>(text);
            if (applicationError.StackTrace != null)
            {
                throw new AriaAccessException($"The service threw an exception.\n{applicationError.Message}\n{applicationError.StackTrace}");
            }
        }
    }
}
