using ExpensesTracker.Models.Entities;
using ExpensesTracker.Models.Enums;
using ExpensesTracker.Services.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ExpensesTracker.Repositories.Interfaces
{
    public interface IEntryRepository
    {
        /// <summary>
        /// Adds a new Entry to the Database.
        /// </summary>
        /// <param name="entry">Entry to be added.</param>
        /// <returns>New Entry</returns>
        Task<Entry?> CreateEntryAsync(Entry entry);
        /// <summary>
        /// Retrieves and Sorts all Entry of an existing user from the Database.
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="searchQuery">Search Queries</param>
        /// <param name="searchModel">Search Model</param>
        /// <param name="sortBy">Sorting Column</param>
        /// <param name="orderBy">Ascending / Descending</param>
        /// <param name="toSkip">Number of Records to Skip</param>
        /// <param name="toTake">Number of Records to Take</param>
        /// <returns></returns>
        Task<List<Entry>> RetrieveAllEntriesAsync(Guid userId, List<string> searchQuery, EntrySearchModel searchModel, EntrySortOptions sortBy, OrderOptions orderBy, int toSkip = 0, int toTake = 0);
        /// <summary>
        /// Retrieves all Entry of an existing user from the Database.
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns>List of Entry</returns>
        Task<List<Entry>> RetrieveAllEntriesByUserIdAsync(Guid userId);
        /// <summary>
        /// Retrieves an existing Entry from the Database.
        /// </summary>
        /// <param name="guid">Entry ID</param>
        /// <returns>Existing Entry; null if not existing.</returns>
        Task<Entry?> RetrieveEntryByIdAsync(Guid guid);
        /// <summary>
        /// Updates an existing Entry from the Database.
        /// </summary>
        /// <param name="entry">Modified Entry</param>
        /// <returns>Updated Entry</returns>
        Task<Entry> UpdateEntryAsync(Entry entry);
        /// <summary>
        /// Deletes an existing Entry from the Database.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>True for successful deletion of Entry; false, otherwise.</returns>
        Task<bool> DeleteEntryAsync(Guid guid);
        /// <summary>
        /// Retries the number of Records of an existing User.
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns>Number of Records</returns>
        Task<int> RetrieveTotalCountAsync(Guid userId);
        /// <summary>
        /// Retrieves All Entries of an existing User as IQueryable<T>.
        /// </summary>
        /// <returns>IQueryable<Entry></returns>
        IQueryable<Entry> RetrieveAllEntriesAsQueryable(Guid userId);
    }
}
