using ExpensesTracker.Services.DTOs;

namespace ExpensesTracker.Services.Interfaces
{
    public interface IEntryService
    {
        /// <summary>
        /// Create a new Entry.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Ceated Entry</returns>
        Task<EntryResponse> CreateEntryAsync(EntryAddRequest request);
        /// <summary>
        /// Retrieves an existing Entry by Id.
        /// </summary>
        /// <param name="entryId"></param>
        /// <returns>Returns the existing Entry</returns>
        Task<EntryResponse?> GetEntryAsync(Guid entryId);
        /// <summary>
        /// Retrieves all Entry of an existing user.
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns>List of existing Entries</returns>
        Task<List<EntryResponse>> GetEntriesAsync(Guid userId);
        /// <summary>
        /// Retrieves all entries that matches the given search criteria.
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns>List of filtered Entries</returns>
        Task<List<EntryResponse>> SearchEntriesAsync(EntrySearchModel searchModel, EntrySortModel sortModel);
        /// <summary>
        /// Sorts an unsorted Entry list based on the given sort order.
        /// </summary>
        /// <param name="sortModel"></param>
        /// <returns>List of sorted Entries</returns>
        List<EntryResponse> SortEntries(List<EntryResponse> unsortedList, EntrySortModel sortModel);
        /// <summary>
        /// Updates an existing Entry
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Updated Entry</returns>
        Task<EntryResponse> UpdateEntryAsync(EntryUpdateRequest request);
        /// <summary>
        /// Deletes an existing Entry.
        /// </summary>
        /// <param name="entryId"></param>
        /// <returns>True if entry is deleted successfully, False if otherwise.</returns>
        Task<bool> DeleteEntryAsync(Guid entryId);
        /// <summary>
        /// Retrieves all Entry for an existing user in Bytes.
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns>Byte[]</returns>
        Task<MemoryStream> GetEntryBytesAsync(Guid userId);
        /// <summary>
        /// Updates Pagination details for the View.
        /// </summary>
        /// <param name="searchModel">Entry Search Model</param>
        void UpdatePagination(EntrySearchModel searchModel);
        /// <summary>
        /// Updates The Card details on the Dashboard.
        /// </summary>
        /// <param name="searchModel">Entry Search Model</param>
        /// <returns>Updated Card Display Model</returns>
        Task<CardDisplayModel> UpdateCardDisplayAsync(EntrySearchModel searchModel);
    }
}
