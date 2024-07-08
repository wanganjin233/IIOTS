using IIOTS.WebRMS.Models;
using Microsoft.AspNetCore.Mvc;
using SharpCifs.Smb;
using System.Text;

namespace IIOTS.WebRMS.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class SMBController : Controller
    {
        /// <summary>
        /// 获取文件列表
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpPost]
        public List<SmbFileInfo> GetFileList(SmbFileSearch search)
        {
            var auth = new NtlmPasswordAuthentication($"{search.Name}:{search.Password}");
            var folder = new SmbFile(search.Url, auth);
            var epocDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            List<SmbFileInfo> smbFileInfos = [];
            foreach (SmbFile item in folder.ListFiles())
            {
                var lastModDate = epocDate.AddMilliseconds(item.LastModified())
                                          .ToLocalTime();
                if (item.GetName().Contains(search.FileName))
                {  
                    smbFileInfos.Add(new SmbFileInfo()
                    {
                        Name = item.GetName(),
                        Path = item.GetPath(),
                        IsDirectory = item.IsDirectory(),
                        LastDateTime = lastModDate
                    });
                    
                }
            }
            return smbFileInfos;
        }
        /// <summary>
        /// 获取文本内容
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult<string> GetFileText(SmbFileSearch search)
        {
            try
            {
                var auth = new NtlmPasswordAuthentication($"{search.Name}:{search.Password}");
                var file = new SmbFile(search.Url, auth);
                var readStream = file.GetInputStream();
                var memStream = new MemoryStream();
                ((Stream)readStream).CopyTo(memStream);
                readStream.Dispose();
                return Ok(Encoding.UTF8.GetString(memStream.ToArray()));
            }
            catch (Exception)
            {
                return NoContent();
            }
        }
    }
}
