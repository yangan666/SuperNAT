using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SuperNAT.Common;
using SuperNAT.Bll;
using SuperNAT.Model;
using System.Linq;

namespace SuperNAT.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommonController : BaseController
    {
        private IWebHostEnvironment hostingEnvironment;
        public CommonController(IWebHostEnvironment env)
        {
            hostingEnvironment = env;
        }

        [HttpPost]
        [Route("GetEnumList")]
        public IActionResult GetEnumList(string type)
        {
            ReflectionLesson reflectionLesson = new ReflectionLesson(AppDomain.CurrentDomain.BaseDirectory + "SuperNAT.Model.dll", "SuperNAT.Model", type);
            Type t = reflectionLesson.ReflectionType();
            var rst = new ApiResult<List<KeyValue>>
            {
                Result = true,
                Data = EnumHelper.EnumToList(t),
                Message = "获取成功"
            };

            return Json(rst);
        }


        [HttpPost]
        [Route("Upload")]
        public IActionResult Upload(IFormFile file)
        {
            var rst = new ApiResult<string>();

            try
            {
                //获取请求报文传递过来的文件
                var name = Request.Query["name"];
                //获取文件扩展名
                //var ext = Path.GetExtension(file.FileName);
                //为保证每次存储的文件名不一样，避免重名，使用guid对文件名进行处理
                var clientFileName = $"{DateTime.Now.ToString("yyyyMMddHHmmssffff")}_{file.FileName}";
                var savePath = $"/Upload/{name}/";
                var filePath = hostingEnvironment.WebRootPath + savePath;
                string FileFullPath = filePath + clientFileName;
                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);
                //保存
                using (FileStream fileStream = System.IO.File.Create(FileFullPath))
                {
                    file.CopyTo(fileStream);
                    fileStream.Flush();
                }

                rst.Result = true;
                rst.Data = savePath + clientFileName;
                rst.Message = "上传成功";
            }
            catch (Exception ex)
            {
                rst.Message = ex.Message;
            }

            return Json(rst);
        }

        [HttpGet]
        [Route("GetSessions")]
        public IActionResult GetSessions(string name)
        {
            IActionResult result = null;
            switch (name)
            {
                case "nat":
                    result = Json(ServerHanlder.NATServer.GetAll().Select(s => new
                    {
                        s.Client,
                        s.ConnectTime,
                        s.Local,
                        s.MapList,
                        s.Remote,
                        s.SessionId
                    }));
                    break;
                case "http":
                    result = Json(ServerHanlder.HttpServerList.SelectMany(s => s.ContextDict));
                    break;
                case "https":
                    result = Json(ServerHanlder.HttpsServerList.SelectMany(s => s.GetAll()).Select(s => new
                    {
                        s.NatSession.Client.name,
                        s.ConnectTime,
                        s.Local,
                        s.Map,
                        s.Remote,
                        s.SessionId
                    }));
                    break;
                case "tcp":
                    result = Json(ServerHanlder.TcpServerList.SelectMany(s => s.GetAll()).Select(s => new
                    {
                        s.NatSession.Client.name,
                        s.ConnectTime,
                        s.Local,
                        s.Map,
                        s.Remote,
                        s.SessionId
                    }));
                    break;
                case "udp":
                    break;
            }

            return result;
        }
    }
}
