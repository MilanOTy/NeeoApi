using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Home.Neeo.Interfaces
{
    public interface IRestClient
    {
        Task<TResp>     HttpGet<TResp>              (string uri);
        Task<TResp>     HttpPost<TResp, TRequest>   (TRequest request, string uri);
        /*
        string Uri                  { get; set; }
        HttpStatusCode LastStatus   { get; }
        int Timeout                 { get; set; }

        string          CreateUriByRootAttribute    (string baseUri, object request, string verb, string format);
        string          CreateUri                   (string baseUri, object request, string verb, string apipath, string format);
        Task<string>    HttpGetString               (string uri);
        Task<TResp>     HttpGet<TResp>              (string uri);
        Task<TResp>     HttpPostString<TResp>       (string request, string uri);
        Task<TResp>     HttpPost<TResp, TRequest>   (TRequest request, string uri);
        Task<TResp>     HttpPut<TResp, TRequest>    (TRequest request, string uri);
        Task<TResp>     HttpDelete<TResp>           (string uri);
        Task<TResp>     UploadFile<TResp>           (string uri, string contentType, Stream stream, int timeout = 0);
        Task<bool>      DownloadFile                (string uri, Stream destStream, int timeout = 0);
        Task<TResp>     HttpGet<TResp, TRequest>    (TRequest request);
        Task<TResp>     HttpPost<TResp, TRequest>   (TRequest request);
        Task<TResp>     HttpPut<TResp, TRequest>    (TRequest request);
        Task<TResp>     HttpDelete<TResp, TRequest> (TRequest request);
        Task<TResp>     UploadFile<TResp, TRequest> (TRequest request, string contentType, Stream stream, int timeout = 0);
        Task<bool>      DownloadFile<TRequest>      (TRequest request, Stream stream, int timeout = 0);
        bool            Cancel                      ();
        */
    }
}
