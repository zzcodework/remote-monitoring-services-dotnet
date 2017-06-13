// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace WebService.Test.helpers.Http
{
    public class HttpClient
    {
        private ITestOutputHelper logger;

        public HttpClient()
        {
        }

        public HttpClient(ITestOutputHelper logger)
        {
            this.logger = logger;
        }

        public async Task<HttpResponse> GetAsync(HttpRequest request)
        {
            return await this.SendAsync(request, HttpMethod.Get);
        }

        public async Task<HttpResponse> PostAsync(HttpRequest request)
        {
            return await this.SendAsync(request, HttpMethod.Post);
        }

        public async Task<HttpResponse> PutAsync(HttpRequest request)
        {
            return await this.SendAsync(request, HttpMethod.Put);
        }

        public async Task<HttpResponse> DeleteAsync(HttpRequest request)
        {
            return await this.SendAsync(request, HttpMethod.Delete);
        }

        private async Task<HttpResponse> SendAsync(HttpRequest request, HttpMethod httpMethod)
        {
            this.LogRequest(request);

            var clientHandler = new WebRequestHandler();
            using (var client = new System.Net.Http.HttpClient(clientHandler))
            {
                var httpRequest = new HttpRequestMessage
                {
                    Method = httpMethod,
                    RequestUri = request.Uri
                };

                SetServerSSLSecurity(request, clientHandler);
                SetTimeout(request, clientHandler, client);
                SetContent(request, httpMethod, httpRequest);
                SetHeaders(request, httpRequest);

                using (var response = await client.SendAsync(httpRequest))
                {
                    if (request.Options.EnsureSuccess) response.EnsureSuccessStatusCode();

                    var result = new HttpResponse
                    {
                        StatusCode = response.StatusCode,
                        Headers = response.Headers,
                        Content = await response.Content.ReadAsStringAsync(),
                    };

                    this.LogResponse(result);

                    return result;
                }
            }
        }

        private static void SetContent(HttpRequest request, HttpMethod httpMethod, HttpRequestMessage httpRequest)
        {
            if (httpMethod != HttpMethod.Post && httpMethod != HttpMethod.Put) return;

            httpRequest.Content = request.Content;
            if (request.ContentType != null && request.Content != null)
            {
                httpRequest.Content.Headers.ContentType = request.ContentType;
            }
        }

        private static void SetHeaders(HttpRequest request, HttpRequestMessage httpRequest)
        {
            foreach (var header in request.Headers)
            {
                httpRequest.Headers.Add(header.Key, header.Value);
            }
        }

        private static void SetServerSSLSecurity(HttpRequest request, WebRequestHandler clientHandler)
        {
            if (request.Options.AllowInsecureSSLServer)
            {
                clientHandler.ServerCertificateValidationCallback = delegate { return true; };
            }
        }

        private static void SetTimeout(
            HttpRequest request,
            WebRequestHandler clientHandler,
            System.Net.Http.HttpClient client)
        {
            clientHandler.ReadWriteTimeout = request.Options.Timeout;
            client.Timeout = TimeSpan.FromMilliseconds(request.Options.Timeout);
        }

        private void LogRequest(HttpRequest request)
        {
            if (this.logger == null) return;

            this.logger.WriteLine("### REQUEST ##############################");
            this.logger.WriteLine("# URI: " + request.Uri);
            this.logger.WriteLine("# Timeout: " + request.Options.Timeout);
            this.logger.WriteLine("# Headers:\n" + request.Headers);
        }

        private void LogResponse(HttpResponse response)
        {
            if (this.logger == null) return;

            this.logger.WriteLine("### RESPONSE ##############################");
            this.logger.WriteLine("# Status code: " + response.StatusCode);
            this.logger.WriteLine("# Headers:\n" + response.Headers.ToString());
            this.logger.WriteLine("# Content:");

            try
            {
                var o = JsonConvert.DeserializeObject(response.Content);
                var s = JsonConvert.SerializeObject(o, Formatting.Indented);
                this.logger.WriteLine(s);
            }
            catch (Exception e)
            {
                this.logger.WriteLine(response.Content);
            }
        }
    }
}
