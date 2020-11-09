using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.IO;

namespace HttpListenerApp
{
    /// <summary>
    /// HttpRequest逻辑处理
    /// </summary>
    public class HttpProvider
    {
        public static string filepath;
        public static HttpListener httpFiledownload; //文件下载处理请求监听

        /// <summary>
        /// 开启HttpListener监听
        /// </summary>
        public static void Init(string url, string path)
        {
            filepath = path;
            httpFiledownload = new HttpListener(); //创建监听实例
            httpFiledownload.Prefixes.Add(url); //添加监听地址 注意是以/结尾。
            httpFiledownload.Start(); //允许该监听地址接受请求的传入。
            Thread ThreadhttpFiledownload = new Thread(new ThreadStart(GethttpFiledownload)); //创建开启一个线程监听该地址得请求
            ThreadhttpFiledownload.Start();
        }

        /// <summary>
        /// 执行文件下载处理请求监听行为
        /// </summary>
        public static void GethttpFiledownload()
        {
            while(true)
            {
                HttpListenerContext requestContext = httpFiledownload.GetContext(); //接受到新的请求
                try
                {
                    //reecontext 为开启线程传入的 requestContext请求对象
                    Thread subthread = new Thread(new ParameterizedThreadStart((reecontext) =>  
                    {
                        var request = (HttpListenerContext)reecontext;
                        using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                        {
                            byte[] buffer = new byte[fs.Length];
                            fs.Read(buffer, 0, (int)fs.Length); //将文件读到缓存区
                            request.Response.StatusCode = 200;
                            request.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                            request.Response.ContentType = "mp4"; 
                            request.Response.ContentLength64 = buffer.Length;
                            var output = request.Response.OutputStream; //获取请求流
                            output.Write(buffer, 0, buffer.Length);  //将缓存区的字节数写入当前请求流返回
                            output.Close();
                        }
                    }));
                    subthread.Start(requestContext); //开启处理线程处理下载文件
                }
                catch { }
            }
        }
    }

}