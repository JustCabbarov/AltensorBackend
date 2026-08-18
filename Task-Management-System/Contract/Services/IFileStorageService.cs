using Contract.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Services
{
    public interface IFileStorageService
    {
        Task<string> UploadAsync(FileDto file);
        Task<Stream> DownloadAsync(string objectName);
        Task<string> GetPresignedUrlAsync(string objectName, int expiresInSeconds = 600);
    }
}
