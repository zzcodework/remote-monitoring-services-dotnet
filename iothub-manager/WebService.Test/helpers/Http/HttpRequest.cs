// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace WebService.Test.helpers.Http
{
    public class HttpRequest
    {
        private const string DefaultMediaType = "application/json";
        private readonly Encoding defaultEncoding = new UTF8Encoding();

        // Http***Headers classes don't have a public ctor, so we use this class
        // to hold the headers, this is also used for PUT/POST requests body
        private readonly HttpRequestMessage requestContent = new HttpRequestMessage();

        public Uri Uri { get; set; }

        public HttpHeaders Headers => this.requestContent.Headers;

        public MediaTypeHeaderValue ContentType { get; private set; }

        public HttpRequestOptions Options { get; } = new HttpRequestOptions();

        public HttpContent Content => this.requestContent.Content;

        public HttpRequest()
        {
        }

        public HttpRequest(Uri uri)
        {
            this.Uri = uri;
        }

        public HttpRequest(string uri)
        {
            this.SetUriFromString(uri);
        }

        public void AddHeader(string name, string value)
        {
            if (!this.Headers.TryAddWithoutValidation(name, value))
            {
                if (name.ToLowerInvariant() != "content-type")
                {
                    throw new ArgumentOutOfRangeException(name, "Invalid header name");
                }

                this.ContentType = new MediaTypeHeaderValue(value);
            }
        }

        public void SetUriFromString(string uri)
        {
            this.Uri = new Uri(uri);
        }

        public void SetContent(StringContent stringContent)
        {
            this.requestContent.Content = stringContent;
            this.ContentType = new MediaTypeHeaderValue(DefaultMediaType);
        }

        public void SetContent(string content)
        {
            this.requestContent.Content = new StringContent(content, this.defaultEncoding, DefaultMediaType);
            this.ContentType = new MediaTypeHeaderValue(DefaultMediaType);
        }

        public void SetContent(string content, Encoding encoding)
        {
            this.requestContent.Content = new StringContent(content, encoding, DefaultMediaType);
            this.ContentType = new MediaTypeHeaderValue(DefaultMediaType);
        }

        public void SetContent(string content, Encoding encoding, MediaTypeHeaderValue mediaType)
        {
            this.requestContent.Content = new StringContent(content, encoding, mediaType.MediaType);
            this.ContentType = mediaType;
        }

        public void SetContent(string content, Encoding encoding, String mediaType)
        {
            this.requestContent.Content = new StringContent(content, encoding, mediaType);
            this.ContentType = new MediaTypeHeaderValue(mediaType);
        }

        public void SetContent<T>(T sourceObject)
        {
            var content = JsonConvert.SerializeObject(sourceObject, Formatting.None);
            this.requestContent.Content = new StringContent(content, this.defaultEncoding, DefaultMediaType);
            this.ContentType = new MediaTypeHeaderValue(DefaultMediaType);
        }
    }
}
