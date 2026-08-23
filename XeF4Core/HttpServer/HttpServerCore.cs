using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XeF4Core;



internal static class HttpServerCore
{
    // 全局单例 HttpClient（根据你的需求配置）
    private static readonly HttpClient _httpClient = new HttpClient(new HttpClientHandler
    {
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
        UseCookies = false
    });

    internal static async Task<byte[]> SendRequest(
        string url,
        HttpMethod? method = null,
        object? content = null,
        KeyValuePair<string,string>[]? headers = null,
        bool simulateBrowserHeaders = false,
        int TimeOut = 25000,
        Encoding? encoding = null)
    {
        method ??= HttpMethod.Get;
        encoding ??= Encoding.UTF8;

        using var cts = new CancellationTokenSource(TimeOut);
        using var request = new HttpRequestMessage(method, url);

        // 处理请求体
        if (content != null && method != HttpMethod.Get && method != HttpMethod.Head)
        {
            request.Content = content switch
            {
                HttpContent httpContent => httpContent,
                byte[] bytes => new ByteArrayContent(bytes),
                _ => new ByteArrayContent(encoding.GetBytes(content.ToString()))
            };
        }

        // 添加自定义请求头
        if (headers != null)
        {
            foreach (var keyValuePair in headers)
            {
                if (string.Equals(keyValuePair.Key, "content-type", StringComparison.OrdinalIgnoreCase))
                    request.Content?.Headers.TryAddWithoutValidation(keyValuePair.Key, keyValuePair.Value);
                else
                    request.Headers.TryAddWithoutValidation(keyValuePair.Key, keyValuePair.Value);
            }
        }

        // 模拟浏览器头（自定义方法）
        HeadersSign(url, request, simulateBrowserHeaders);

        // 发送请求
        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cts.Token).ConfigureAwait(false);


        // 读取响应体（无论成功失败都要读）
        byte[] responseBytes;
        {
            using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var memoryStream = new MemoryStream();
            await responseStream.CopyToAsync(memoryStream, 81920, cts.Token).ConfigureAwait(false);
            responseBytes = memoryStream.ToArray();
        }

        if (response.IsSuccessStatusCode)
        {
            return responseBytes;
        }
        else
        {

            string errorMessage = encoding.GetString(responseBytes);
            throw new HttpRequestException(
                $"{response.StatusCode} :发送网络请求失败\n{url},{method}\n返回信息: {errorMessage}");
        }
    }

    internal static void HeadersSign(string url, HttpRequestMessage req, bool simulateBrowserHeaders = false)
    {
        if (simulateBrowserHeaders)
        {
            if (url.Contains("baidupcs.com") || url.Contains("baidu.com"))
            {
                req.Headers.Add("User-Agent", "LogStatistic");
            }
            else if (simulateBrowserHeaders)
            {
                req.Headers.Add("User-Agent", $"{HttpServer.DefaultHeaderSign} Mozilla/5.0 AppleWebKit/537.36 Chrome/63.0.3239.132 Safari/537.36");
            }
            else
            {
                req.Headers.Add("User-Agent", $"{HttpServer.DefaultHeaderSign}");
            }
        }


        if (!simulateBrowserHeaders)
        {
            req.Headers.Add("Referer", "http://C#.PCL.Alpha/");
        }

    }

    public static async Task<bool> IsRangeSupportedAsync(string url, int timeoutMilliseconds = 10000)
    {
        using var cts = new CancellationTokenSource(timeoutMilliseconds);
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(0, 0); // bytes=0-0

        // 关键：只读取响应头，不自动读取正文
        using var response = await _httpClient.SendAsync(request,
            HttpCompletionOption.ResponseHeadersRead,
            cts.Token).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.PartialContent) // 206
        {
            // 支持 Range，此时响应体只有 1 个字节（我们不会读取它）
            return true;
        }
        else if (response.StatusCode == HttpStatusCode.OK) // 200
        {
            // 服务器忽略了 Range 头，返回完整文件
            // 但我们没有读取响应体，直接关闭连接，避免浪费
            return false;
        }
        else if (response.StatusCode == HttpStatusCode.RequestedRangeNotSatisfiable) // 416 Range Not Satisfiable
        {
            // 文件大小为 0 或请求范围无效
            return false;
        }
        else
        {
            return false;
        }
    }

}
