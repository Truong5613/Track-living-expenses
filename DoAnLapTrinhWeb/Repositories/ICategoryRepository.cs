using DoAnLapTrinhWeb.Models;

namespace DoAnLapTrinhWeb.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync(string userid);
        Task<Category> GetByIdAsync(string userid,int id);
        Task AddAsync(string userid, Category category);
        Task UpdateAsync(string userid, Category category);
        Task DeleteAsync(string userid, int id);
    }
}
