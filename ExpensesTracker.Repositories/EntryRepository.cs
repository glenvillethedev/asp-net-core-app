using ExpensesTracker.Models;
using ExpensesTracker.Models.Entities;
using ExpensesTracker.Models.Enums;
using ExpensesTracker.Repositories.Interfaces;
using ExpensesTracker.Services.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ExpensesTracker.Repositories
{
    public class EntryRepository : IEntryRepository
    {
        private ApplicationDbContext _dbContext;
        public EntryRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Entry?> CreateEntryAsync(Entry entry)
        {
            _dbContext.Entries.Add(entry);
            await _dbContext.SaveChangesAsync();

            return entry;
        }

        public async Task<List<Entry>> RetrieveAllEntriesAsync(Guid userId, List<string> searchQueries, EntrySearchModel searchModel, EntrySortOptions sortBy, OrderOptions orderBy, int toSkip = 0, int toTake = 0)
        {
            var query = _dbContext.Entries.Where(entry => entry.UserId == userId);

            foreach (var sq in searchQueries)
            {
                switch (sq)
                {
                    case nameof(EntrySearchModel.SearchString):
                        query = query.Where(entry => entry.Name!.Contains(searchModel.SearchString!)
                        || entry.Details!.Contains(searchModel.SearchString!));
                        break;
                    case nameof(EntrySearchModel.MinAmount):
                        query = query.Where(entry => entry.Amount >= searchModel.MinAmount);
                        break;
                    case nameof(EntrySearchModel.MaxAmount):
                        query = query.Where(entry => entry.Amount <= searchModel.MaxAmount);
                        break;
                    case nameof(EntrySearchModel.DateFrom):
                        query = query.Where(entry => entry.Date >= searchModel.DateFrom);
                        break;
                    case nameof(EntrySearchModel.DateTo):
                        query = query.Where(entry => entry.Date <= searchModel.DateTo);
                        break;
                    case nameof(EntrySearchModel.Category):
                        query = query.Where(entry => entry.Category == searchModel.Category);
                        break;
                }
            }

            query = (sortBy, orderBy)switch
            {
                (EntrySortOptions.NAME, OrderOptions.ASCENDING) => query.OrderBy(entry => entry.Name),
                (EntrySortOptions.NAME, OrderOptions.DESCENDING) => query.OrderByDescending(entry => entry.Name),
                (EntrySortOptions.AMOUNT, OrderOptions.ASCENDING) => query.OrderBy(entry => entry.Amount),
                (EntrySortOptions.AMOUNT, OrderOptions.DESCENDING) => query.OrderByDescending(entry => entry.Amount),
                (EntrySortOptions.DATE, OrderOptions.ASCENDING) => query.OrderBy(entry => entry.Date),
                (EntrySortOptions.DATE, OrderOptions.DESCENDING) => query.OrderByDescending(entry => entry.Date),
                (EntrySortOptions.CATEGORY, OrderOptions.ASCENDING) => query.OrderBy(entry => entry.Category),
                (EntrySortOptions.CATEGORY, OrderOptions.DESCENDING) => query.OrderByDescending(entry => entry.Category),
                _ => query.OrderBy(entry => entry.Name),
            };

            searchModel.TotalRecords = query.Count();

            return await query.Skip(toSkip).Take(toTake).ToListAsync();
        }

        public async Task<List<Entry>> RetrieveAllEntriesByUserIdAsync(Guid userId)
        {
            return await _dbContext.Entries.Where(entry => entry.UserId == userId).OrderByDescending(entry => entry.CreateDt).ToListAsync();
        }

        public async Task<Entry?> RetrieveEntryByIdAsync(Guid guid)
        {
            return await _dbContext.Entries.FirstOrDefaultAsync(entry => entry.Id == guid);
        }

        public async Task<Entry> UpdateEntryAsync(Entry entry)
        {
            var existingEntry = await RetrieveEntryByIdAsync(entry.Id);

            if (existingEntry != null)
            {
                existingEntry.Name = entry.Name;
                existingEntry.Amount = entry.Amount;
                existingEntry.Date = entry.Date;
                existingEntry.Category = entry.Category;
                existingEntry.Details = entry.Details;
                existingEntry.UpdateDt = DateTime.Now;

                await _dbContext.SaveChangesAsync();
            }

            return entry;
        }

        public async Task<bool> DeleteEntryAsync(Guid guid)
        {
            var existingEntry = await RetrieveEntryByIdAsync(guid);

            if (existingEntry == null)
            {
                return false;
            }

            _dbContext.Remove(existingEntry!);
            var affectedRows = await _dbContext.SaveChangesAsync();

            return affectedRows > 0;
        }

        public async Task<int> RetrieveTotalCountAsync(Guid userId)
        {
            return await _dbContext.Entries.CountAsync(entry => entry.UserId == userId);
        }

        public IQueryable<Entry> RetrieveAllEntriesAsQueryable(Guid userId)
        {
            return _dbContext.Entries.Where(entry => entry.UserId == userId);
        }
    }
}
