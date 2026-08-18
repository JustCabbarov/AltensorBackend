using Contract.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Services
{
    public interface IPerformanceService
    {
        Task<int> GetTotalPointsAsync(string userId);
        Task AddPerformancePointAsync(PerformancePointDTo performance);
        Task<List<LeaderBoardDTO>> leaderboard();
    }
}
