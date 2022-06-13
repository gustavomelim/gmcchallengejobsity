using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JobsityNetChallenge.Domain.Extensions
{
    public static class WebJsonExtensions
    {
        public static Task<HttpResponseMessage> PostAsJsonAsync<T>(this HttpClient httpClient, string url, T obj, CancellationToken cancellationToken)
        {
            return PostAsJsonAsync(httpClient, url, obj, null, cancellationToken);
        }

        public static Task<HttpResponseMessage> PostAsJsonAsync<T>(this HttpClient httpClient, string url, T obj, JsonSerializerSettings settings, CancellationToken cancellationToken)
        {
            var jsonObject = JsonConvert.SerializeObject(obj, settings);
            var jsonAsString = jsonObject.ToString();
            var content = new StringContent(jsonAsString, Encoding.UTF8, "application/json");
            return httpClient.PostAsync(url, content, cancellationToken);
        }

        public static Task<HttpResponseMessage> PutAsJsonAsync<T>(this HttpClient httpClient, string url, T obj, CancellationToken cancellationToken)
        {
            return PutAsJsonAsync(httpClient, url, obj, null, cancellationToken);
        }

        public static Task<HttpResponseMessage> PutAsJsonAsync<T>(this HttpClient httpClient, string url, T obj, JsonSerializerSettings settings, CancellationToken cancellationToken)
        {
            var jsonObject = JsonConvert.SerializeObject(obj, settings);
            var content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
            return httpClient.PutAsync(url, content, cancellationToken);
        }

        public static Task<HttpResponseMessage> PatchAsJsonAsync<T>(this HttpClient httpClient, string url, T obj, CancellationToken cancellationToken)
        {
            return PatchAsJsonAsync(httpClient, url, obj, null, cancellationToken);
        }

        public static Task<HttpResponseMessage> PatchAsJsonAsync<T>(this HttpClient httpClient, string url, T obj, JsonSerializerSettings settings, CancellationToken cancellationToken)
        {
            var jsonObject = JsonConvert.SerializeObject(obj, settings);
            var content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), new Uri(url))
            {
                Content = content
            };
            return httpClient.SendAsync(request, cancellationToken);
        }

        public static async Task<T> ToResultAsync<T>(this HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken)
        {
            if (!httpResponseMessage.IsSuccessStatusCode && httpResponseMessage.StatusCode != HttpStatusCode.NotModified)
            {
                await DecodeException(httpResponseMessage);
            }
            return await httpResponseMessage.Content.ReadAsJsonAsync<T>(cancellationToken);
        }

        public static async Task<String> ToResultAsStringAsync(this HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken)
        {
            if (!httpResponseMessage.IsSuccessStatusCode && httpResponseMessage.StatusCode != HttpStatusCode.NotModified)
            {
                await DecodeException(httpResponseMessage);
            }

            return await httpResponseMessage.Content.ReadAsStringAsync();
        }

        public static async Task ToResultAsync(this HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken)
        {
            if (!httpResponseMessage.IsSuccessStatusCode && httpResponseMessage.StatusCode != HttpStatusCode.NotModified)
            {
                await DecodeException(httpResponseMessage);
            }
        }

        public static async Task DecodeException(HttpResponseMessage httpResponseMessage)
        {
            String message;
            try
            {
                message = await httpResponseMessage.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                message = httpResponseMessage.ReasonPhrase;
            }
            var tail = $"({httpResponseMessage.StatusCode}) [{httpResponseMessage.RequestMessage.RequestUri}] - {message}";
            switch (httpResponseMessage.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    throw new Exception($"Unable to find {tail}");
                case HttpStatusCode.InternalServerError:
                    throw new Exception($"Error from other side {tail}");
                case HttpStatusCode.GatewayTimeout:
                    throw new Exception($"Timedout from other side {tail}");
                case HttpStatusCode.Forbidden:
                    throw new Exception($"Other side block you access {tail}");
                case HttpStatusCode.ServiceUnavailable:
                    throw new Exception($"Other side is down! {tail}");
                case HttpStatusCode.BadRequest:
                    throw new Exception($"Other side does not accept our request! {tail}");
                default:
                    throw new Exception($"Other side breaks us! {tail}");
            }
        }

        public static async Task<T> ReadAsJsonAsync<T>(this HttpContent httpContent, CancellationToken cancellationToken)
        {
            var str = await httpContent.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(str);
        }

    }

}