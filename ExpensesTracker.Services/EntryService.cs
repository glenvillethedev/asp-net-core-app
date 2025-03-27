using CsvHelper;
using CsvHelper.Configuration;
using ExpensesTracker.Models;
using ExpensesTracker.Models.Enums;
using ExpensesTracker.Repositories.Interfaces;
using ExpensesTracker.Services.DTOs;
using ExpensesTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ExpensesTracker.Services
{
    public class EntryService : IEntryService
    {
        private IEntryRepository _entryRepository;
        public EntryService(IEntryRepository entryRepository)
        {
            _entryRepository = entryRepository;
        }

        public async Task<EntryResponse> CreateEntryAsync(EntryAddRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            ValidateAddRequest(request);

            var newEntry = request.ToEntry();
            newEntry.Id = Guid.NewGuid();
            newEntry.CreateDt = DateTime.Now;

            await _entryRepository.CreateEntryAsync(newEntry);

            return newEntry.ToEntryResponse();
        }

        public async Task<EntryResponse?> GetEntryAsync(Guid entryId)
        {
            if (entryId == Guid.Empty)
            {
                throw new ArgumentNullException("ID must not be empty.");
            }

            var newEntry = await _entryRepository.RetrieveEntryByIdAsync(entryId);

            return newEntry?.ToEntryResponse() ?? null;
        }

        public async Task<List<EntryResponse>> GetEntriesAsync(Guid userId)
        {
            return (await _entryRepository.RetrieveAllEntriesByUserIdAsync(userId)).Select(entry => entry.ToEntryResponse()).ToList();
        }

        public async Task<List<EntryResponse>> SearchEntriesAsync(EntrySearchModel searchModel, EntrySortModel sortModel)
        {
            if (searchModel == null)
            {
                throw new ArgumentNullException("Search Data must not be null.");
            }
            if (sortModel == null)
            {
                throw new ArgumentNullException("List/Sort Data must not be null.");
            }

            var searchQueries = ValidateSearchModel(searchModel);
            if (searchQueries.Any() && !searchModel.IsPagination) // Reset Page Number to 1 if Searching
            {
                searchModel.PageNumber = 1;
            }
            var result = await _entryRepository.RetrieveAllEntriesAsync(searchModel.UserId, searchQueries, searchModel, sortModel.SortBy, sortModel.OrderBy, toSkip: (searchModel.PageNumber - 1) * searchModel.PageSize, toTake: searchModel.PageSize);

            return result.Select(entry => entry.ToEntryResponse()).ToList();
        }

        public List<EntryResponse> SortEntries(List<EntryResponse> unsortedList, EntrySortModel sortModel)
        {
            if (unsortedList == null || sortModel == null)
            {
                throw new ArgumentNullException("List/Sort Data must not be null.");
            }

            List<EntryResponse> sortedList = new List<EntryResponse>();

            switch (sortModel.SortBy, sortModel.OrderBy)
            {
                case (EntrySortOptions.NAME, OrderOptions.ASCENDING):
                    sortedList = unsortedList.OrderBy(entry => entry.Name).ToList();
                    break;
                case (EntrySortOptions.NAME, OrderOptions.DESCENDING):
                    sortedList = unsortedList.OrderByDescending(entry => entry.Name).ToList();
                    break;
                case (EntrySortOptions.AMOUNT, OrderOptions.ASCENDING):
                    sortedList = unsortedList.OrderBy(entry => entry.Amount).ToList();
                    break;
                case (EntrySortOptions.AMOUNT, OrderOptions.DESCENDING):
                    sortedList = unsortedList.OrderByDescending(entry => entry.Amount).ToList();
                    break;
                case (EntrySortOptions.DATE, OrderOptions.ASCENDING):
                    sortedList = unsortedList.OrderBy(entry => entry.Date).ToList();
                    break;
                case (EntrySortOptions.DATE, OrderOptions.DESCENDING):
                    sortedList = unsortedList.OrderByDescending(entry => entry.Date).ToList();
                    break;
                case (EntrySortOptions.CATEGORY, OrderOptions.ASCENDING):
                    sortedList = unsortedList.OrderBy(entry => entry.Category).ToList();
                    break;
                case (EntrySortOptions.CATEGORY, OrderOptions.DESCENDING):
                    sortedList = unsortedList.OrderByDescending(entry => entry.Category).ToList();
                    break;
                default:
                    sortedList = unsortedList.OrderBy(entry => entry.Name).ToList();
                    break;
            }

            return sortedList;
        }

        public async Task<EntryResponse> UpdateEntryAsync(EntryUpdateRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            ValidateUpdateRequest(request);
            var existingEntry = await _entryRepository.RetrieveEntryByIdAsync(request.Id);

            if (existingEntry == null)
            {
                throw new ArgumentException("Entry ID does not exist.");
            }

            var updateEntry = await _entryRepository.UpdateEntryAsync(request.ToEntry());

            return updateEntry.ToEntryResponse();
        }

        public async Task<bool> DeleteEntryAsync(Guid entryId)
        {
            var existingEntry = await _entryRepository.RetrieveEntryByIdAsync(entryId);

            if (existingEntry == null)
            {
                return false;
            }
            else
            {
                return await _entryRepository.DeleteEntryAsync(entryId);
            }
        }

        public async Task<MemoryStream> GetEntryBytesAsync(Guid userId)
        {
            MemoryStream memoryStream = new MemoryStream();
            StreamWriter streamWriter = new StreamWriter(memoryStream);

            // configure CsvWriter
            CsvConfiguration csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture);
            CsvWriter csvWriter = new CsvWriter(streamWriter, csvConfiguration);

            // writeHeader
            csvWriter.WriteField(nameof(EntryResponse.Name));
            csvWriter.WriteField(nameof(EntryResponse.Amount));
            csvWriter.WriteField(nameof(EntryResponse.Date));
            csvWriter.WriteField(nameof(EntryResponse.Category));
            csvWriter.WriteField(nameof(EntryResponse.Details));

            // writeRecords & Flush
            csvWriter.NextRecord();
            List<EntryResponse> entries = (await _entryRepository.RetrieveAllEntriesByUserIdAsync(userId)).OrderBy(entry => entry.CreateDt).Select(entry => entry.ToEntryResponse()).ToList();
            foreach (EntryResponse entry in entries)
            {
                csvWriter.WriteField(entry.Name);
                csvWriter.WriteField($"$ {entry.Amount.ToString("N2")}");
                csvWriter.WriteField(entry.Date.ToString("MM-dd-yyyy"));
                csvWriter.WriteField(entry.Category.ToString());
                csvWriter.WriteField(entry.Details);
                csvWriter.NextRecord();
                csvWriter.Flush();
            }

            // reset memmorystream cursor to zero.
            memoryStream.Position = 0;

            return memoryStream;
        }

        public void UpdatePagination(EntrySearchModel searchModel)
        {
            int pageNumberLimit = 5;
            int currentPage = searchModel.PageNumber;
            int numberOfPages = (int)Math.Ceiling((searchModel.TotalRecords / (float)searchModel.PageSize));

            searchModel.PageList = GeneratePageNumbers(numberOfPages, currentPage, pageNumberLimit);

            int min = ((currentPage - 1) * searchModel.PageSize) + 1;
            int max = currentPage * searchModel.PageSize;

            if (currentPage == numberOfPages)
            {
                max = searchModel.TotalRecords;
            }

            if (min == max)
            {
                searchModel.CurrentRecords = max.ToString();
            }
            else
            {
                searchModel.CurrentRecords = $"{min}-{max}";
            }
        }

        public async Task<CardDisplayModel> UpdateCardDisplayAsync(EntrySearchModel searchModel)
        {
            CardDisplayModel result = new CardDisplayModel();

            result.TotalExpenses = await _entryRepository.RetrieveAllEntriesAsQueryable(searchModel.UserId).SumAsync(entry => entry.Amount);
            result.TotalNeeds = await _entryRepository.RetrieveAllEntriesAsQueryable(searchModel.UserId).Where(entry => entry.Category == EntryCategoryOptions.NEEDS).SumAsync(entry => entry.Amount);
            result.TotalWants = await _entryRepository.RetrieveAllEntriesAsQueryable(searchModel.UserId).Where(entry => entry.Category == EntryCategoryOptions.WANTS).SumAsync(entry => entry.Amount);

            return result;
        }

        #region Private Methods
        private void ValidateAddRequest(EntryAddRequest addRequest)
        {
            List<string> errors = new List<string>();

            // Name
            if (addRequest.Name == null)
            {
                errors.Add("Name must not be empty.");
            }
            else
            {
                if (addRequest.Name.Length > 50)
                {
                    errors.Add("Name must not exceed 50 characters.");
                }
            }

            // Amount
            if (addRequest.Amount == null)
            {
                errors.Add("Amount must not be empty.");
            }
            else
            {
                if (addRequest.Amount <= 0)
                {
                    errors.Add("Amount must be more than zero (0).");
                }
            }

            // Date
            if (addRequest.Date == null)
            {
                errors.Add("Date must not be empty.");
            }

            // Details
            if (addRequest.Details?.Length > 0 && addRequest.Details.Length > 100)
            {
                errors.Add("Details must not exceed 100 characters.");
            }

            // Category
            if (addRequest.Category == null
                || (addRequest.Category != EntryCategoryOptions.NEEDS && addRequest.Category != EntryCategoryOptions.WANTS))
            {
                errors.Add("Category must not be empty.");
            }

            if (errors.Any())
            {
                throw new ArgumentException(string.Join("\n", errors));
            }

            // UserId
            if (addRequest.UserId == Guid.Empty)
            {
                errors.Add("User does not exist.");
            }
        }

        private void ValidateUpdateRequest(EntryUpdateRequest updateRequest)
        {
            List<string> errors = new List<string>();

            // Name
            if (updateRequest.Name == null)
            {
                errors.Add("Name must not be empty.");
            }
            else
            {
                if (updateRequest.Name.Length > 50)
                {
                    errors.Add("Name must not exceed 50 characters.");
                }
            }

            // Amount
            if (updateRequest.Amount == null)
            {
                errors.Add("Amount must not be empty.");
            }
            else
            {
                if (updateRequest.Amount <= 0)
                {
                    errors.Add("Amount must be more than zero (0).");
                }
            }

            // Date
            if (updateRequest.Date == null)
            {
                errors.Add("Date must not be empty.");
            }

            // Details
            if (updateRequest.Details?.Length > 0 && updateRequest.Details.Length > 100)
            {
                errors.Add("Details must not exceed 100 characters.");
            }

            // Category
            if (updateRequest.Category == null
                || (updateRequest.Category != EntryCategoryOptions.NEEDS && updateRequest.Category != EntryCategoryOptions.WANTS))
            {
                errors.Add("Category must not be empty.");
            }

            // UserId
            if (updateRequest.UserId == Guid.Empty)
            {
                errors.Add("User does not exist.");
            }

            if (errors.Any())
            {
                throw new ArgumentException(string.Join("\n", errors));
            }
        }

        private List<string> ValidateSearchModel(EntrySearchModel model)
        {
            List<string> errors = new List<string>();
            List<string> searchOptions = new List<string>();

            // Search string
            if (model.SearchString != null)
            {
                if (model.SearchString.Length > 50)
                {
                    errors.Add("Search text must not exceed 50 characters.");
                }
                else
                {
                    searchOptions.Add(nameof(EntrySearchModel.SearchString));
                }
            }

            // Amount Range
            if (model.MinAmount != null && model.MaxAmount != null)
            {
                if (model.MinAmount <= 0)
                {
                    errors.Add("Minimum Amount must be greater than zero (0).");
                }
                if (model.MaxAmount <= 0)
                {
                    errors.Add("Maximum Amount must be greater than zero (0).");
                }
                if (model.MinAmount > model.MaxAmount)
                {
                    errors.Add("Search Amount must be a valid range.");
                }
                else
                {
                    searchOptions.Add(nameof(EntrySearchModel.MinAmount));
                    searchOptions.Add(nameof(EntrySearchModel.MaxAmount));
                }
            }
            else if (model.MinAmount != null)
            {
                if (model.MinAmount <= 0)
                {
                    errors.Add("Minimum Amount must be greater than zero (0).");
                }
                else
                {
                    searchOptions.Add(nameof(EntrySearchModel.MinAmount));
                }
            }
            else if (model.MaxAmount != null)
            {
                if (model.MaxAmount <= 0)
                {
                    errors.Add("Maximum Amount must be greater than zero (0).");
                }
                else
                {
                    searchOptions.Add(nameof(EntrySearchModel.MaxAmount));
                }
            }

            // Date Range
            if (model.DateFrom != null && model.DateTo != null)
            {
                if (model.DateFrom > model.DateTo)
                {
                    errors.Add("Search Date must be a valid range.");
                }
                else
                {
                    searchOptions.Add(nameof(EntrySearchModel.DateFrom));
                    searchOptions.Add(nameof(EntrySearchModel.DateTo));
                }
            }
            else if (model.DateFrom != null)
            {
                searchOptions.Add(nameof(EntrySearchModel.DateFrom));
            }
            else if (model.DateTo != null)
            {
                searchOptions.Add(nameof(EntrySearchModel.DateTo));
            }

            // Category
            if (model.Category != null)
            {
                if (model.Category != EntryCategoryOptions.NEEDS && model.Category != EntryCategoryOptions.WANTS)
                {
                    errors.Add("Search Category must be a valid Category.");
                }
                else
                {
                    searchOptions.Add(nameof(EntrySearchModel.Category));
                }
            }

            // UserId
            if (model.UserId == Guid.Empty)
            {
                errors.Add("User does not exist.");
            }

            if (errors.Any())
            {
                throw new ArgumentException(string.Join("\n", errors));
            }

            return searchOptions;
        }

        private List<object> GeneratePageNumbers(int totalPages, int currentPage, int maxVisiblePages)
        {
            if (totalPages <= maxVisiblePages)
            {
                return Enumerable.Range(1, totalPages).Cast<object>().ToList();
            }

            int startPage = Math.Max(1, currentPage - maxVisiblePages / 2);
            int endPage = Math.Min(totalPages, currentPage + (int)Math.Ceiling(maxVisiblePages / 2.0) - 1);

            if (endPage - startPage < maxVisiblePages - 1)
            {
                if (startPage > 1)
                {
                    startPage = Math.Max(1, endPage - maxVisiblePages + 1);
                }
                else
                {
                    endPage = Math.Min(totalPages, startPage + maxVisiblePages - 1);
                }
            }

            var pageNumbers = new List<object>();

            if (startPage > 1)
            {
                pageNumbers.Add(1);
                if (startPage > 2)
                {
                    pageNumbers.Add("...");
                }
            }

            pageNumbers.AddRange(Enumerable.Range(startPage, endPage - startPage + 1).Cast<object>());

            if (endPage < totalPages)
            {
                if (endPage < totalPages - 1)
                {
                    pageNumbers.Add("...");
                }
                pageNumbers.Add(totalPages);
            }

            return pageNumbers;
        }
    }
    #endregion
}

