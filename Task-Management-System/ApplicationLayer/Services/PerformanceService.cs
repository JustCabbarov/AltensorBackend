using Contract.DTOs;
using Contract.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class PerformanceService : IPerformanceService
    {
        private readonly IGenericRepository<PerformancePoint> _performancePointRepository;
        private readonly IGenericRepository<TaskItem> _taskRepository;
        private readonly IGenericRepository<TaskTransaction> _transactionRepository;
        private readonly IUnityOfWork _unity;

        public PerformanceService(
            IGenericRepository<PerformancePoint> performancePointRepository,
            IGenericRepository<TaskItem> taskRepository,
            IGenericRepository<TaskTransaction> transactionRepository,
            IUnityOfWork unity)
        {
            _performancePointRepository = performancePointRepository;
            _taskRepository = taskRepository;
            _transactionRepository = transactionRepository;
            _unity = unity;
        }

        public async Task<int> GetTotalPointsAsync(string userId)
        {
            var uid = Guid.Parse(userId);
            var points = await _performancePointRepository.GetAllAsync(
                query => query.Where(p => p.UserId == uid)
            );
            return points.Sum(p => p.Points);
        }

        public async Task AddPerformancePointAsync(PerformancePointDTo performance)
        {
            var taskId = performance.taskId;
            var senderId = Guid.Parse(performance.senderId);
            var userId = Guid.Parse(performance.userId);

            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
                throw new Exception("Task tapılmadı.");

            if (task.CreatedByUserId != senderId)
                throw new UnauthorizedAccessException("Only the creator of the task can assign performance points.");

            var point = task.Difficulty switch
            {
                DifficultyLevel.Easy   => 10,
                DifficultyLevel.Medium => 20,
                DifficultyLevel.Hard   => 30,
                _ => 0
            };

            var performancePoint = new PerformancePoint
            {
                UserId = userId,
                Points = point,
                Reason = performance.reason,
            };

            task.Status = CurrentSituation.Completed;
            await _taskRepository.UpdateAsync(task);
            await _performancePointRepository.AddAsync(performancePoint);
            await _transactionRepository.AddAsync(new TaskTransaction
            {
                TaskItemId = task.Id,
                FromUserId = task.CreatedByUserId,
                ToUserId = userId,
                Comments = "Performance Point Added"
            });
            await _unity.SaveChangesAsync();
        }

        public async Task<List<LeaderBoardDTO>> leaderboard()
        {
            var points = await _performancePointRepository.GetAllAsync(
                include: q => q.Include(x => x.User)
            );

            return points
                .GroupBy(x => new { x.UserId, x.User.UserName })
                .Select(g => new LeaderBoardDTO
                {
                    UserId = g.Key.UserId.ToString(),
                    UserName = g.Key.UserName,
                    TotalPoints = g.Sum(x => x.Points)
                })
                .OrderByDescending(x => x.TotalPoints)
                .ToList();
        }
    }
}
