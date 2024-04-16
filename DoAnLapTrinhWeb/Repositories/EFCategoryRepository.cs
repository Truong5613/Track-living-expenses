using DoAnLapTrinhWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnLapTrinhWeb.Repositories
{
    public class EFCategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;
        public async Task AddAsync(string userid, Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string userid, int id)
        {
            var category = await _context.Categories.FindAsync(id);
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Category>> GetAllAsync(string userid)
        {
            return await _context.Categories.Where(x=>x.UserID==userid).ToListAsync();
        }

        public async Task<Category> GetByIdAsync(string userid, int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task UpdateAsync(string userid, Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }
    }
}
