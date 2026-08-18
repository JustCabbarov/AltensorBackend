using Contract.DTOs;
using Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Contract.Services
{
    public interface ITaskAttachmentService
    {
        Task<string> GetPreviewUrlAsync(Guid attachmentId, string currentUserId);
        Task<FileDto> DownloadAsync(Guid attachmentId, string currentUserId);
        Task<TaskAttachment> UploadAndSaveAsync(Guid taskId, FileDto fileDto, string currentUserId);
    }
}
