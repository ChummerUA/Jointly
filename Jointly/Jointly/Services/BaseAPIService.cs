﻿using Jointly.Extensions;
using Jointly.Interfaces;
using Jointly.Models.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Jointly.Services
{
    public abstract class BaseAPIService
    {
        private readonly HttpClient Client;
        protected readonly IAnalyticsService AnalyticsService;

        public BaseAPIService(
            HttpClient client,
            IAnalyticsService analyticsService)
        {
            Client = client;
            AnalyticsService = analyticsService;
        }

        protected abstract Task<APIResponse<T>> ProcessResponseAsync<T>(HttpResponseMessage responseMessage, Task<APIResponse<T>> retryIfTokenRefreshed);

        protected virtual async Task<APIResponse<T>> ProcessResponseAsync<T>(HttpResponseMessage responseMessage)
        {
            var response = new APIResponse<T>
            {
                StatusCode = responseMessage.StatusCode,
                IsSuccess = responseMessage.IsSuccessStatusCode,
            };
            if (responseMessage.IsSuccessStatusCode)
            {
                var stream = await responseMessage.Content.ReadAsStreamAsync();
                response.Result = DeserializeJsonFromStream<T>(stream);
            }
            return response;
        }

        private async Task<APIResponse<T>> SendAsync<T>(HttpRequestMessage message, bool retryOnUnauthorized, CancellationToken cToken = default)
        {
            try
            {
                cToken = cToken != default ? cToken : new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;

                var response = await Client.SendAsync(message, cToken);

                return retryOnUnauthorized ? await ProcessResponseAsync(response, SendAsync<T>(message, false, cToken)) :
                    await ProcessResponseAsync<T>(response);
            }
            catch (Exception ex)
            {
                AnalyticsService.TrackError(ex);
                throw ex;
            }
        }

        private T DeserializeJsonFromStream<T>(Stream stream)
        {
            using var sr = new StreamReader(stream);
            using var jtr = new JsonTextReader(sr);
            var js = new JsonSerializer();
            var searchResult = js.Deserialize<T>(jtr);
            return searchResult;
        }

        public Task<APIResponse<T>> GetAsync<T>(string url, NameValueCollection nvc = null, List<KeyValuePair<string, string>> headers = null, bool retryOnUnauthorized = true, CancellationToken cToken = default)
        {
            var query = nvc.ToQuery();
            var message = CreateRequestMessage($"{url}{query}", HttpMethod.Get, headers);
            return SendAsync<T>(message, retryOnUnauthorized, cToken);
        }

        public Task<APIResponse<T>> PostAsync<T>(string url, object obj = null, List<KeyValuePair<string, string>> headers = null, bool retryOnUnauthorized = true, CancellationToken cToken = default)
        {
            var message = CreateRequestMessage(url, HttpMethod.Post, obj, headers);
            return SendAsync<T>(message, retryOnUnauthorized, cToken);
        }

        public Task<APIResponse<T>> PutAsync<T>(string url, object obj = null, List<KeyValuePair<string, string>> headers = null, bool retryOnUnauthorized = true, CancellationToken cToken = default)
        {
            var message = CreateRequestMessage(url, HttpMethod.Put, obj, headers);
            return SendAsync<T>(message, retryOnUnauthorized, cToken);
        }

        public Task<APIResponse<T>> DeleteAsync<T>(string url, object obj = null, List<KeyValuePair<string, string>> headers = null, bool retryOnUnauthorized = true, CancellationToken cToken = default)
        {
            var message = CreateRequestMessage(url, HttpMethod.Delete, obj, headers);
            return SendAsync<T>(message, retryOnUnauthorized, cToken);
        }

        public Task<APIResponse<T>> PatchAsync<T>(string url, object obj = null, List<KeyValuePair<string, string>> headers = null, bool retryOnUnauthorized = true, CancellationToken cToken = default)
        {
            var message = CreateRequestMessage(url, HttpMethod.Patch, obj, headers);
            return SendAsync<T>(message, retryOnUnauthorized, cToken);
        }

        public Task<APIResponse<T>> UploadAsync<T>(string url, string filepath, object obj = null, List<KeyValuePair<string, string>> headers = null, bool retryOnUnauthorized = true, CancellationToken cToken = default)
        {
            throw new NotImplementedException();
        }

        private HttpRequestMessage CreateRequestMessage(string url, HttpMethod method, List<KeyValuePair<string, string>> headers = null)
        {
            var message = new HttpRequestMessage(method, url);
            headers?.ForEach(x =>
            {
                message.Headers.Add(x.Key, x.Value);
            });

            return message;
        }

        private HttpRequestMessage CreateRequestMessage(string url, HttpMethod method, object obj = null, List<KeyValuePair<string, string>> headers = null)
        {
            var message = CreateRequestMessage(url, method, headers);

            var json = JsonConvert.SerializeObject(obj);
            message.Content = new StringContent(json);

            return message;
        }
    }
}