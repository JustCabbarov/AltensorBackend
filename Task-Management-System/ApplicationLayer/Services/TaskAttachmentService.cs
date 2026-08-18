using Contract.DTOs;
using Contract.Services;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Minio;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Application.Services
{
    public class TaskAttachmentService : ITaskAttachmentService
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly IGenericRepository<TaskAttachment> _attachmentRepository;
        private readonly IUnityOfWork _unitOfWork;

        public TaskAttachmentService(IFileStorageService fileStorageService, IGenericRepository<TaskAttachment> attachmentRepository, IUnityOfWork unit)
        {
            _fileStorageService = fileStorageService;
            _attachmentRepository = attachmentRepository;
            _unitOfWork = unit;
        }

        public async Task<string> GetPreviewUrlAsync(Guid attachmentId, string currentUserId)
        {
            var fileName = await GetFileNameFromDbAsync(attachmentId, currentUserId);
            if (string.IsNullOrEmpty(fileName))
                throw new FileNotFoundException("Attachment not found");
            return await _fileStorageService.GetPresignedUrlAsync(fileName);
        }

        public async Task<FileDto> DownloadAsync(Guid attachmentId, string currentUserId)
        {
            var fileName = await GetFileNameFromDbAsync(attachmentId, currentUserId);
            if (string.IsNullOrEmpty(fileName))
                throw new FileNotFoundException("Attachment not found");

            var stream = await _fileStorageService.DownloadAsync(fileName);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);

            return new FileDto
            {
                FileName = fileName,
                ContentType = GetContentType(fileName),
                Content = ms.ToArray()
            };
        }

        private async Task<string?> GetFileNameFromDbAsync(Guid attachmentId, string currentUserId)
        {
            var attachment = await _attachmentRepository.GetByIdAsync(
                attachmentId,
                include: q => q.Include(a => a.Task)
            );

            if (attachment == null)
                return null;

            var uid = Guid.Parse(currentUserId);
            if (attachment.Task.AssignedToUserId != uid && attachment.Task.CreatedByUserId != uid)
                return null;

            return attachment.ObjectName;
        }

        private string GetContentType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext switch
            {
                ".pdf"  => "application/pdf",
                ".jpg"  => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png"  => "image/png",
                ".txt"  => "text/plain",
                _       => "application/octet-stream"
            };
        }

        public async Task<TaskAttachment> UploadAndSaveAsync(Guid taskId, FileDto fileDto, string currentUserId)
        {
            var objectName = await _fileStorageService.UploadAsync(fileDto);

            var attachment = new TaskAttachment
            {
                TaskId = taskId,
                FileName = fileDto.FileName,
                ObjectName = objectName,
                ContentType = fileDto.ContentType,
                Size = fileDto.Content.Length
            };

            await _attachmentRepository.AddAsync(attachment);
            await _unitOfWork.SaveChangesAsync();

            return attachment;
        }
    }
}
