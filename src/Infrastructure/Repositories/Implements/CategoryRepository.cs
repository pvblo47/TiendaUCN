using Microsoft.EntityFrameworkCore;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Data;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Infrastructure.Repositories.Implements
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly DataContext _context;
        public CategoryRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Categories
                .AnyAsync(b =>
                    b.Name.ToLower() == name.ToLower() &&
                    b.IsDeleted == false);
        }

        public async Task<bool> CreateAsync(Category category)
        {
            _context.Categories.Add(category);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ExistsByIdAsync(int id)
        {
            return await _context.Categories.AnyAsync(b =>
                b.Id == id &&
                b.IsDeleted == false);
        }

        public async Task<bool> UpdateNameAsync(int id, string name)
        {
            var result = await _context.Categories
                .Where(b => b.Id == id)
                .ExecuteUpdateAsync(b => b.SetProperty(x => x.Name, name));

            return result > 0;
        }

        public async Task<bool> UpdateDescriptionAsync(int id, string description)
        {
            var result = await _context.Categories
                .Where(b => b.Id == id)
                .ExecuteUpdateAsync(b => b.SetProperty(x => x.Description, description));

            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var result = await _context.Categories
                .Where(b => b.Id == id)
                .ExecuteUpdateAsync(b => b.SetProperty(x => x.IsDeleted, true));

            return result > 0;
        }

        public async Task<int> GetIdByNameAsync(string name)
        {
            return await _context.Categories
                .Where(b => b.Name.ToLower() == name.ToLower() && !b.IsDeleted)
                .Select(b => b.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Category>> GetAllActiveAsync()
        {
            return await _context.Categories
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }
}