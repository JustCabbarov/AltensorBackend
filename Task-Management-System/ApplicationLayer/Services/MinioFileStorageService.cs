using Contract.DTOs;
using Contract.Services;
using Minio;
using Minio.DataModel.Args;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class MinioFileStorageService : IFileStorageService
    {
        private readonly IMinioClient _minio;
        private readonly string _bucket = "uploads";

        public MinioFileStorageService(IMinioClient minio)
        {
            _minio = minio;
        }

        public async Task<string> UploadAsync(FileDto file)
        {
            bool exists = await _minio.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucket));
            if (!exists)
            {
                await _minio.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucket));
            }
            using var ms = new MemoryStream(file.Content);


            await _minio.PutObjectAsync(new PutObjectArgs()
                .WithBucket(_bucket)
                .WithObject(file.FileName) 
                .WithStreamData(ms)
                .WithObjectSize(ms.Length)
                .WithContentType(file.ContentType));

            return file.FileName;
        }

        public async Task<Stream> DownloadAsync(string objectName)
        {
            var ms = new MemoryStream();
            await _minio.GetObjectAsync(new GetObjectArgs()
                .WithBucket(_bucket)
                .WithObject(objectName)
                .WithCallbackStream(s => s.CopyTo(ms)));

            ms.Position = 0;
            return ms;
        }


        public async Task<string> GetPresignedUrlAsync(string objectName, int expiresInSeconds = 600)
        {
            return await _minio.PresignedGetObjectAsync(
                new PresignedGetObjectArgs()
                    .WithBucket(_bucket)
                    .WithObject(objectName)
                    .WithExpiry(expiresInSeconds) 
            );
        }
    }

}
