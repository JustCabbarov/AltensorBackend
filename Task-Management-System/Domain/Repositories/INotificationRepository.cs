using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface INotificationRepository
    {
        
            Task AddAsync(Notification notification);
            Task AddRangeAsync(List<Notification> notifications);
            Task<List<Notification>> GetUserNotificationsAsync(Guid userId);
            Task MarkAsReadAsync(Guid notificationId);
        
    }
}
