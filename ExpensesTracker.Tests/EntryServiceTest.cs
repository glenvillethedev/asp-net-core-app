using AutoFixture;
using ExpensesTracker.Models.Entities;
using ExpensesTracker.Models.Enums;
using ExpensesTracker.Repositories.Interfaces;
using ExpensesTracker.Services;
using ExpensesTracker.Services.DTOs;
using ExpensesTracker.Services.Interfaces;
using FluentAssertions;
using Moq;
using Xunit.Abstractions;

namespace ExpensesTracker.Tests
{
    public class EntryServiceTest
    {
        private ITestOutputHelper _output;
        private Mock<IEntryRepository> _repositoryMock;
        private IEntryService _entryService;
        private IFixture _fixture;

        public EntryServiceTest(ITestOutputHelper output)
        {
            //// DbContext Mocking
            //var mockDbContext = new DbContextMock<ApplicationDbContext>(new DbContextOptionsBuilder<ApplicationDbContext>().Options);
            //mockDbContext.CreateDbSetMock(db => db.Entries, new List<Entry>());
            
            // Repository Mocking
            _repositoryMock = new Mock<IEntryRepository>();

            _entryService = new EntryService(_repositoryMock.Object);
            _fixture = new Fixture();
            _output = output;
        }

        #region Create Entry
        [Fact]
        public async Task CreateEntry_NullRequest_ToBeError()
        {
            //Arrange
            EntryAddRequest request = null!;
            var action = async () =>
            {
                await _entryService.CreateEntryAsync(request!);
            };

            //Act & Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task CreateEntry_InvalidValues_ToBeError()
        {
            EntryAddRequest invalidName1 = _fixture.Build<EntryAddRequest>().With(e => e.Name, null as string).Create();
            EntryAddRequest invalidName2 = _fixture.Build<EntryAddRequest>().With(e => e.Name, "111111111111111111111111111111111111111111111111111").Create();

            // amount = required, notEqualToZero
            EntryAddRequest invalidAmount1 = _fixture.Build<EntryAddRequest>().With(e => e.Amount, null as decimal?).Create();
            EntryAddRequest invalidAmount2 = _fixture.Build<EntryAddRequest>().With(e => e.Amount, 0).Create();

            // date = required
            EntryAddRequest invalidDate = _fixture.Build<EntryAddRequest>().With(e => e.Date, null as DateTime?).Create();

            // details = maxlength100
            EntryAddRequest invalidDetails = _fixture.Build<EntryAddRequest>().With(e => e.Details, "11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111").Create();

            // category = required, 10/20

            EntryAddRequest invalidCategory1 = _fixture.Build<EntryAddRequest>().With(e => e.Category, null as EntryCategoryOptions?).Create();
            EntryAddRequest invalidCategory2 = _fixture.Build<EntryAddRequest>().With(e => e.Category, (EntryCategoryOptions?)0).Create();

            // Act & Assert
            var action1 = async () => await _entryService.CreateEntryAsync(invalidName1);
            await action1.Should().ThrowAsync<ArgumentException>();

            var action2 = async () => await _entryService.CreateEntryAsync(invalidName2);
            await action2.Should().ThrowAsync<ArgumentException>();

            var action3 = async () => await _entryService.CreateEntryAsync(invalidAmount1);
            await action3.Should().ThrowAsync<ArgumentException>();

            var action4 = async () => await _entryService.CreateEntryAsync(invalidAmount2);
            await action4.Should().ThrowAsync<ArgumentException>();

            var action5 = async () => await _entryService.CreateEntryAsync(invalidDate);
            await action5.Should().ThrowAsync<ArgumentException>();

            var action6 = async () => await _entryService.CreateEntryAsync(invalidDetails);
            await action6.Should().ThrowAsync<ArgumentException>();

            var action7 = async () => await _entryService.CreateEntryAsync(invalidCategory1);
            await action7.Should().ThrowAsync<ArgumentException>();

            var action8 = async () => await _entryService.CreateEntryAsync(invalidCategory2);
            await action8.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task CreateEntry_ValidValues_ToBeSuccess()
        {
            // Arrange

            EntryAddRequest addRequest = _fixture.Create<EntryAddRequest>();            
            _repositoryMock.Setup(repo => repo.CreateEntryAsync(It.IsAny<Entry>())).ReturnsAsync(addRequest.ToEntry());

            // Act
            var actual = await _entryService.CreateEntryAsync(addRequest);

            // Assert
            actual.Id.Should().NotBe(Guid.Empty);
            actual.Name.Should().Be(addRequest.Name);
            actual.Amount.Should().Be(addRequest.Amount);
            actual.Date.Should().Be(addRequest.Date);
            actual.Details.Should().Be(addRequest.Details);
            actual.Category.Should().Be(addRequest.Category);

            _output.WriteLine(actual.ToString());
        }
        #endregion

        #region Retrieve Entry
        [Fact]
        public async Task GetEntry_NullArgument_ToBeError()
        {
            // Arrange
            var emptyGuid = Guid.Empty;

            // Act & Assert
            var action = async () => await _entryService.GetEntryAsync(emptyGuid);
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task GetEntry_InvalidId_ToBeNull()
        {
            // Arrange
            _repositoryMock.Setup(repo => repo.RetrieveEntryByIdAsync(It.IsAny<Guid>())).ReturnsAsync(null as Entry);

            // Act
            var actual = await _entryService.GetEntryAsync(Guid.NewGuid());

            // Assert
            Assert.Null(actual);

        }

        [Fact]
        public async Task GetEntry_ValidId_ToBeNotNull()
        {
            // Arrange
            var expected = _fixture.Create<Entry>();
            _repositoryMock.Setup(repo => repo.RetrieveEntryByIdAsync(It.IsAny<Guid>())).ReturnsAsync(expected);

            // Act
            var actual = await _entryService.GetEntryAsync(expected.Id);

            // Assert
            actual.Should().NotBeNull();
            actual.Name.Should().Be(expected.Name);
            actual.Amount.Should().Be(expected.Amount);
            actual.Date.Should().Be(expected.Date);
            actual.Details.Should().Be(expected.Details);
            actual.Category.Should().Be(expected.Category);

            _output.WriteLine("Actual: " + actual.ToString());
        }

        [Fact]
        public async Task GetEntries_Initial_ToBeEmpty()
        {
            // Arrange
            var expected = new List<Entry>();
            _repositoryMock.Setup(repo => repo.RetrieveAllEntriesByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync(expected);

            // Act & Assert
            var someValue = (await _entryService.GetEntriesAsync(Guid.NewGuid()));
            (await _entryService.GetEntriesAsync(Guid.NewGuid())).Should().BeEmpty();
        }

        [Fact]
        public async Task GetEntries_WithData_ToBeNotEmpty()
        {
            // Arrange
            var expected = new List<Entry>(){ _fixture.Create<Entry>(), _fixture.Create<Entry>(), _fixture.Create<Entry>(), _fixture.Create<Entry>() };
            _repositoryMock.Setup(repo => repo.RetrieveAllEntriesByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync(expected);

            // Act
            var actual = await _entryService.GetEntriesAsync(Guid.NewGuid());

            actual.Should().NotBeEmpty();
            actual.Count.Should().Be(expected.Count);

            _output.WriteLine("Actual: " + actual.Count);
        }

        [Fact]
        public async Task SearchEntries_NullArgument_ToBeError()
        {
            // Arrange
            EntrySearchModel searchRequest = null!;
            EntrySortModel sortRequest = null!;

            // Act & Assert
            var action = async () => await _entryService.SearchEntriesAsync(searchRequest, sortRequest);
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task SearchEntries_InvalidSearchModel_ToBeError()
        {
            // Arrange
            var currentDate = DateTime.Now;
            var expected = new List<Entry>();
            _repositoryMock.Setup(repo => repo.RetrieveAllEntriesByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync(expected);

            // searchString = maxLength50
            EntrySearchModel searchString = _fixture.Build<EntrySearchModel>().With(e => e.SearchString, "111111111111111111111111111111111111111111111111111").Create();

            // minmax_amount = greaterthanZero, validRange
            EntrySearchModel amount1 = _fixture.Build<EntrySearchModel>().With(e => e.MinAmount, 0).Create();
            EntrySearchModel amount2 = _fixture.Build<EntrySearchModel>().With(e => e.MaxAmount, 0).Create();
            EntrySearchModel amount3 = _fixture.Build<EntrySearchModel>().With(e => e.MinAmount, 350).With(e => e.MaxAmount, 150).Create();

            // fromto_date = validRange
            EntrySearchModel date = _fixture.Build<EntrySearchModel>().With(e => e.DateFrom, currentDate.AddMonths(-2)).With(e => e.DateTo, currentDate.AddMonths(-3)).Create();

            // category = 10/20 
            EntrySearchModel category = _fixture.Build<EntrySearchModel>().With(e => e.Category, (EntryCategoryOptions)0).Create();

            EntrySortModel sortModel = _fixture.Build<EntrySortModel>().Create();

            // Act & Assert
            var action1 = async () => await _entryService.SearchEntriesAsync(searchString, sortModel);
            await action1.Should().ThrowAsync<ArgumentException>();

            var action2 = async () => await _entryService.SearchEntriesAsync(amount1, sortModel);
            await action2.Should().ThrowAsync<ArgumentException>();

            var action3 = async () => await _entryService.SearchEntriesAsync(amount2, sortModel);
            await action3.Should().ThrowAsync<ArgumentException>();

            var action4 = async () => await _entryService.SearchEntriesAsync(amount3, sortModel);
            await action4.Should().ThrowAsync<ArgumentException>();

            var action5 = async () => await _entryService.SearchEntriesAsync(date, sortModel);
            await action5.Should().ThrowAsync<ArgumentException>();

            var action6 = async () => await _entryService.SearchEntriesAsync(category, sortModel);
            await action6.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task SearchEntries_ValidSearchModel_ToBeSuccess()
        {
            // Arrange
            var currentDate = DateTime.Now;
            var expected = new List<Entry>()
            {
                _fixture.Build<Entry>().With(e => e.Name, "test_item1").With(e => e.Amount, 100).With(e => e.Date, currentDate.AddMonths(-1)).With(e => e.Details, "test_details1").With(e => e.Category, EntryCategoryOptions.WANTS).Create(),
                _fixture.Build<Entry>().With(e => e.Name, "test_item2").With(e => e.Amount, 200).With(e => e.Date, currentDate.AddMonths(-2)).With(e => e.Details, "test_details2").With(e => e.Category, EntryCategoryOptions.NEEDS).Create(),
                _fixture.Build<Entry>().With(e => e.Name, "test_item3").With(e => e.Amount, 300).With(e => e.Date, currentDate.AddMonths(-3)).With(e => e.Details, "test_details3").With(e => e.Category, EntryCategoryOptions.NEEDS).Create(),
                _fixture.Build<Entry>().With(e => e.Name, "test_item4").With(e => e.Amount, 400).With(e => e.Date, currentDate.AddMonths(-4)).With(e => e.Details, "test_details4").With(e => e.Category, EntryCategoryOptions.WANTS).Create(),
            };
            _repositoryMock.Setup(repo => repo.RetrieveAllEntriesAsync(It.IsAny<Guid>(), It.IsAny<List<string>>(), It.IsAny<EntrySearchModel>(), It.IsAny<EntrySortOptions>(), It.IsAny<OrderOptions>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(expected);

            EntrySearchModel valid = _fixture.Build<EntrySearchModel>()
                .With(e => e.SearchString, "test")
                .With(e => e.MinAmount, 150)
                .With(e => e.MaxAmount, 350)
                .With(e => e.DateFrom, currentDate.AddMonths(-3))
                .With(e => e.DateTo, currentDate.AddMonths(-2))
                .With(e => e.Category, EntryCategoryOptions.NEEDS)
                .Create();

            EntrySearchModel searchString = _fixture.Build<EntrySearchModel>()
                .With(e => e.SearchString, "3")
                .With(e => e.MinAmount, null as decimal?)
                .With(e => e.MaxAmount, null as decimal?)
                .With(e => e.DateFrom, null as DateTime?)
                .With(e => e.DateTo, null as DateTime?)
                .With(e => e.Category, (EntryCategoryOptions?)null)
                .Create();

            EntrySearchModel amount1 = _fixture.Build<EntrySearchModel>()
                .With(e => e.SearchString, null as string)
                .With(e => e.MinAmount, 100)
                .With(e => e.MaxAmount, 100)
                .With(e => e.DateFrom, null as DateTime?)
                .With(e => e.DateTo, null as DateTime?)
                .With(e => e.Category, (EntryCategoryOptions?)null)
                .Create();
            EntrySearchModel amount2 = _fixture.Build<EntrySearchModel>()
                .With(e => e.SearchString, null as string)
                .With(e => e.MinAmount, 150)
                .With(e => e.MaxAmount, 350)
                .With(e => e.DateFrom, null as DateTime?)
                .With(e => e.DateTo, null as DateTime?)
                .With(e => e.Category, (EntryCategoryOptions?)null)
                .Create();
            EntrySearchModel amount3 = _fixture.Build<EntrySearchModel>()
                .With(e => e.SearchString, null as string)
                .With(e => e.MinAmount, 250)
                .With(e => e.MaxAmount, null as decimal?)
                .With(e => e.DateFrom, null as DateTime?)
                .With(e => e.DateTo, null as DateTime?)
                .With(e => e.Category, (EntryCategoryOptions?)null)
                .Create();
            EntrySearchModel amount4 = _fixture.Build<EntrySearchModel>()
                .With(e => e.SearchString, null as string)
                .With(e => e.MinAmount, null as decimal?)
                .With(e => e.MaxAmount, 150)
                .With(e => e.DateFrom, null as DateTime?)
                .With(e => e.DateTo, null as DateTime?)
                .With(e => e.Category, (EntryCategoryOptions?)null)
                .Create();


            EntrySearchModel date1 = _fixture.Build<EntrySearchModel>()
                .With(e => e.SearchString, null as string)
                .With(e => e.MinAmount, null as decimal?)
                .With(e => e.MaxAmount, null as decimal?)
                .With(e => e.DateFrom, currentDate.AddMonths(-3))
                .With(e => e.DateTo, currentDate.AddMonths(-3))
                .With(e => e.Category, (EntryCategoryOptions?)null)
                .Create();
            EntrySearchModel date2 = _fixture.Build<EntrySearchModel>()
                .With(e => e.SearchString, null as string)
                .With(e => e.MinAmount, null as decimal?)
                .With(e => e.MaxAmount, null as decimal?)
                .With(e => e.DateFrom, currentDate.AddMonths(-3))
                .With(e => e.DateTo, currentDate.AddMonths(-2))
                .With(e => e.Category, (EntryCategoryOptions?)null)
                .Create();
            EntrySearchModel date3 = _fixture.Build<EntrySearchModel>()
                .With(e => e.SearchString, null as string)
                .With(e => e.MinAmount, null as decimal?)
                .With(e => e.MaxAmount, null as decimal?)
                .With(e => e.DateFrom, currentDate.AddMonths(-1))
                .With(e => e.DateTo, null as DateTime?)
                .With(e => e.Category, (EntryCategoryOptions?)null)
                .Create();
            EntrySearchModel date4 = _fixture.Build<EntrySearchModel>()
                .With(e => e.SearchString, null as string)
                .With(e => e.MinAmount, null as decimal?)
                .With(e => e.MaxAmount, null as decimal?)
                .With(e => e.DateFrom, null as DateTime?)
                .With(e => e.DateTo, currentDate.AddMonths(-2))
                .With(e => e.Category, (EntryCategoryOptions?)null)
                .Create();


            EntrySearchModel category1 = _fixture.Build<EntrySearchModel>()
                .With(e => e.SearchString, null as string)
                .With(e => e.MinAmount, null as decimal?)
                .With(e => e.MaxAmount, null as decimal?)
                .With(e => e.DateFrom, null as DateTime?)
                .With(e => e.DateTo, null as DateTime?)
                .With(e => e.Category, EntryCategoryOptions.NEEDS)
                .Create();
            EntrySearchModel category2 = _fixture.Build<EntrySearchModel>()
                .With(e => e.SearchString, null as string)
                .With(e => e.MinAmount, null as decimal?)
                .With(e => e.MaxAmount, null as decimal?)
                .With(e => e.DateFrom, null as DateTime?)
                .With(e => e.DateTo, null as DateTime?)
                .With(e => e.Category, EntryCategoryOptions.WANTS)
                .Create();

            EntrySortModel sortModel = _fixture.Build<EntrySortModel>().Create();


            // Act & Assert
            var actual = await _entryService.SearchEntriesAsync(valid, sortModel);
            actual.Count.Should().Be(4);

            actual = await _entryService.SearchEntriesAsync(searchString, sortModel);
            actual.Count.Should().Be(4);

            actual = await _entryService.SearchEntriesAsync(amount1, sortModel);
            actual.Count.Should().Be(4);

            actual = await _entryService.SearchEntriesAsync(amount2, sortModel);
            actual.Count.Should().Be(4);

            actual = await _entryService.SearchEntriesAsync(amount3, sortModel);
            actual.Count.Should().Be(4);

            actual = await _entryService.SearchEntriesAsync(amount4, sortModel);
            actual.Count.Should().Be(4);

            actual = await _entryService.SearchEntriesAsync(date1, sortModel);
            actual.Count.Should().Be(4);

            actual = await _entryService.SearchEntriesAsync(date2, sortModel);
            actual.Count.Should().Be(4);

            actual = await _entryService.SearchEntriesAsync(date3, sortModel);
            actual.Count.Should().Be(4);

            actual = await _entryService.SearchEntriesAsync(date4, sortModel);
            actual.Count.Should().Be(4);

            actual = await _entryService.SearchEntriesAsync(category1, sortModel);
            actual.Count.Should().Be(4);

            actual = await _entryService.SearchEntriesAsync(category2, sortModel);
            actual.Count.Should().Be(4);
        }

        [Fact]
        public void SortEntries_NullArgument_ToBeError()
        {
            // Arrange
            List<EntryResponse> unsortedList = null!;
            EntrySortModel sortModel = null!;

            // Act & Assert
            var action = () => _entryService.SortEntries(unsortedList, sortModel);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void SortEntries_EmptyList_ToBeEmpty()
        {
            // Arrange
            List<EntryResponse> unsortedList = new List<EntryResponse>();
            EntrySortModel sortModel = new EntrySortModel()
            {
                SortBy = EntrySortOptions.NAME,
                OrderBy = OrderOptions.ASCENDING
            };

            // Act & Assert
            Assert.Empty(_entryService.SortEntries(unsortedList, sortModel));
        }

        [Fact]
        public void SortEntries_ValidListAndValues_ToBeSuccess()
        {
            // Arrange
            var currentDate = DateTime.Now;
            var expected = new List<EntryResponse>()
            {
                _fixture.Build<EntryResponse>().With(e => e.Name, "test_item1").With(e => e.Amount, 100).With(e => e.Date, currentDate.AddMonths(-1)).With(e => e.Details, "test_details1").With(e => e.Category, EntryCategoryOptions.WANTS).Create(),
                _fixture.Build<EntryResponse>().With(e => e.Name, "test_item2").With(e => e.Amount, 200).With(e => e.Date, currentDate.AddMonths(-2)).With(e => e.Details, "test_details2").With(e => e.Category, EntryCategoryOptions.NEEDS).Create(),
                _fixture.Build<EntryResponse>().With(e => e.Name, "test_item3").With(e => e.Amount, 300).With(e => e.Date, currentDate.AddMonths(-3)).With(e => e.Details, "test_details3").With(e => e.Category, EntryCategoryOptions.NEEDS).Create(),
                _fixture.Build<EntryResponse>().With(e => e.Name, "test_item4").With(e => e.Amount, 400).With(e => e.Date, currentDate.AddMonths(-4)).With(e => e.Details, "test_details4").With(e => e.Category, EntryCategoryOptions.WANTS).Create(),
            };

            // Act & Assert - w/o FluentAssertions
            EntrySortModel sortModel = new EntrySortModel()
            {
                SortBy = EntrySortOptions.AMOUNT,
                OrderBy = OrderOptions.DESCENDING
            };

            var actual = _entryService.SortEntries(expected, sortModel);
            Assert.True(actual[0].Name == "test_item4");

            sortModel = new EntrySortModel()
            {
                SortBy = EntrySortOptions.NAME,
                OrderBy = OrderOptions.DESCENDING
            };
            actual = _entryService.SortEntries(expected, sortModel);
            Assert.True(actual[0].Name == "test_item4");

            sortModel = new EntrySortModel()
            {
                SortBy = EntrySortOptions.DATE,
                OrderBy = OrderOptions.ASCENDING
            };
            actual = _entryService.SortEntries(expected, sortModel);
            Assert.True(actual[0].Name == "test_item4");

            sortModel = new EntrySortModel()
            {
                SortBy = EntrySortOptions.CATEGORY,
                OrderBy = OrderOptions.ASCENDING
            };
            actual = _entryService.SortEntries(expected, sortModel);
            Assert.True(actual[0].Name == "test_item2");
        }
        #endregion

        #region Update Entry
        [Fact]
        public async Task UpdateEntry_NullArgument_ToBeError()
        {
            // Arrange
            EntryUpdateRequest request = null!;

            // Act & Assert
            var action = async () => await _entryService.UpdateEntryAsync(request);
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task UpdateEntry_InvalidValues_ToBeError()
        {
            // Arrange, Act & Assert
            // ID - required
            EntryUpdateRequest invalidID = _fixture.Build<EntryUpdateRequest>().With(e => e.Id, Guid.Empty).Create();
            var action1 = async () => await _entryService.UpdateEntryAsync(invalidID);
            await action1.Should().ThrowAsync<ArgumentException>();

            EntryAddRequest validReq = new EntryAddRequest()
            {
                Name = "test_item",
                Amount = 100,
                Date = DateTime.Now,
                Details = "test_details",
                Category = EntryCategoryOptions.NEEDS
            };

            // Name = required, maxlength 50
            EntryUpdateRequest invalidName = _fixture.Create<EntryUpdateRequest>();

            invalidName.Name = null;
            var action2 = async () => await _entryService.UpdateEntryAsync(invalidName);
            await action2.Should().ThrowAsync<ArgumentException>();

            invalidName.Name = "111111111111111111111111111111111111111111111111111";
            var action3 = async () => await _entryService.UpdateEntryAsync(invalidName);
            await action3.Should().ThrowAsync<ArgumentException>();

            // amount = required, notEqualToZero
            EntryUpdateRequest invalidAmount = _fixture.Create<EntryUpdateRequest>();

            invalidAmount.Amount = null;
            var action4 = async () => await _entryService.UpdateEntryAsync(invalidAmount);
            await action4.Should().ThrowAsync<ArgumentException>();

            invalidAmount.Amount = 0;
            var action5 = async () => await _entryService.UpdateEntryAsync(invalidAmount);
            await action5.Should().ThrowAsync<ArgumentException>();

            // date = required
            EntryUpdateRequest invalidDate = _fixture.Create<EntryUpdateRequest>();

            invalidDate.Date = null;
            var action6 = async () => await _entryService.UpdateEntryAsync(invalidDate);
            await action6.Should().ThrowAsync<ArgumentException>();

            // details = maxlength100
            EntryUpdateRequest invalidDetails = _fixture.Create<EntryUpdateRequest>();

            invalidDetails.Details = "11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111";
            var action7 = async () => await _entryService.UpdateEntryAsync(invalidDetails);
            await action7.Should().ThrowAsync<ArgumentException>();

            // category = required, 10/20
            EntryUpdateRequest invalidCategory = _fixture.Create<EntryUpdateRequest>();

            invalidCategory.Category = null;
            var action8 = async () => await _entryService.UpdateEntryAsync(invalidCategory);
            await action8.Should().ThrowAsync<ArgumentException>();

            invalidCategory.Category = 0;
            var action9 = async () => await _entryService.UpdateEntryAsync(invalidCategory);
            await action9.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task UpdateEntry_ValidValues_ToBeSuccess()
        {
            // Arrange
            var entry = _fixture.Create<Entry>();
            var validRequest = _fixture.Create<EntryUpdateRequest>();
            validRequest.Id = entry.Id;

            _repositoryMock.Setup(repo => repo.RetrieveEntryByIdAsync(It.IsAny<Guid>())).ReturnsAsync(entry);
            _repositoryMock.Setup(repo => repo.UpdateEntryAsync(It.IsAny<Entry>())).ReturnsAsync(validRequest.ToEntry());

            // Act
            var actual = await _entryService.UpdateEntryAsync(validRequest);

            // Assert
            actual.Id.Should().Be(validRequest.Id);
            actual.Name.Should().Be(validRequest.Name);
            actual.Amount.Should().Be(validRequest.Amount);
            actual.Date.Should().Be(validRequest.Date);
            actual.Details.Should().Be(validRequest.Details);
            actual.Category.Should().Be(validRequest.Category);
        }
        #endregion

        #region Delete Entry
        [Fact]
        public async Task DeleteEntry_InvalidId_ToBeFalse()
        {
            // Arrange
            _repositoryMock.Setup(repo => repo.RetrieveEntryByIdAsync(It.IsAny<Guid>())).ReturnsAsync(null as Entry);

            // Act & Assert
            (await _entryService.DeleteEntryAsync(Guid.Empty)).Should().BeFalse();
        }

        [Fact]
        public async Task DeleteEntry_Success() {
            // Arrange
            var entry = _fixture.Create<Entry>();
            _repositoryMock.Setup(repo => repo.RetrieveEntryByIdAsync(It.IsAny<Guid>())).ReturnsAsync(entry);
            _repositoryMock.Setup(repo => repo.DeleteEntryAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            // Act & Assert
            Assert.True(await _entryService.DeleteEntryAsync(entry.Id));
        }
        #endregion
    }
}
