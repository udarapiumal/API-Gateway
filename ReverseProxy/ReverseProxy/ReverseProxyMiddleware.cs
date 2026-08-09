using ReverseProxy.Models;
using System.Net.Sockets;

namespace ReverseProxy
{
    public class ReverseProxyMiddleware
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly RequestDelegate _nextMiddleware;
        private readonly IConfiguration _config;

        public ReverseProxyMiddleware(RequestDelegate nextMiddleware , IConfiguration config)
        {
            _nextMiddleware = nextMiddleware;
            _config = config;
        }

        public async Task Invoke(HttpContext context)
        {
            var targetUri = BuildTargetUri(context.Request);
            Console.WriteLine("hello: " + context.Request.Path);

            if (targetUri!= null)
            {
                var targetRequestMessage = CreateRequestMessage(context, targetUri);
                Console.WriteLine("hyyo:" + targetRequestMessage);
                try
                {
                    using (var responseMessage = await _httpClient.SendAsync(targetRequestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted))
                    {
                        context.Response.StatusCode = (int)responseMessage.StatusCode;
                        CopyFromTargetResponseHeaders(responseMessage, context);

                        await responseMessage.Content.CopyToAsync(context.Response.Body);

                    }
                }
                catch (HttpRequestException ex)
                {
                    if(ex.InnerException is SocketException socketEx)
                    {
                        await context.Response.WriteAsync(socketEx.Message);
                    }

                    
                }

               
                return;

            }



            await _nextMiddleware(context);

        }


        private Uri? BuildTargetUri(HttpRequest request)
        {
            var routes = _config.GetSection("Routes").Get<RouteConfig[]>();

            if (routes == null) return null;

            foreach (var route in routes)
            {
                if (request.Path.StartsWithSegments(route.path, out var remaining))
                {
                    return new Uri(route.Target + remaining);
                }
            }
            return null;
        }

        private HttpRequestMessage CreateRequestMessage(HttpContext context,Uri targetUri)
        {
            var requestMessage = new HttpRequestMessage();


            CopyFromOriginalContentAndHeaders(context,requestMessage);

            requestMessage.RequestUri = targetUri;
            requestMessage.Headers.Host = targetUri.Host;
            requestMessage.Method = GetMethod(context.Request);

            return requestMessage;
        }

        private void CopyFromOriginalContentAndHeaders(HttpContext context,HttpRequestMessage requestMessage)
        {
            var requestMethod = context.Request.Method;

            if (!HttpMethods.IsGet(requestMethod) &&
                !HttpMethods.IsHead(requestMethod) &&
                !HttpMethods.IsDelete(requestMethod) &&
                !HttpMethods.IsTrace(requestMethod)
                
                )
            {
                var streamContent = new StreamContent(context.Request.Body);
                requestMessage.Content = streamContent;
            }

            foreach(var header in context.Request.Headers)
            {
                requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }

        private static HttpMethod GetMethod(HttpRequest request)
        {
            var someMethod = new HttpMethod(request.Method);

            if (HttpMethods.IsGet(request.Method)) return HttpMethod.Get;
            if (HttpMethods.IsHead(request.Method)) return HttpMethod.Head;
            if (HttpMethods.IsDelete(request.Method)) return HttpMethod.Delete;
            if (HttpMethods.IsPut(request.Method)) return HttpMethod.Put;
            if (HttpMethods.IsPost(request.Method)) return HttpMethod.Post;
            if (HttpMethods.IsOptions(request.Method)) return HttpMethod.Options;
            if (HttpMethods.IsTrace(request.Method)) return HttpMethod.Trace;

            return someMethod;

        }
        private void CopyFromTargetResponseHeaders(HttpResponseMessage responseMessage,HttpContext context)
        {
            foreach(var header in responseMessage.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in responseMessage.Content.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }
            context.Response.Headers.Remove("transfer-encoding");
        }
    }


}
