using AutoFixture;
using ExpensesTracker.Controllers;
using ExpensesTracker.Services.DTOs;
using ExpensesTracker.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Rotativa.AspNetCore;
using System.Security.Claims;
using Xunit.Abstractions;

namespace ExpensesTracker.Tests
{
    public class EntryControllerTest
    {
        private IFixture _fixture;

        private ITestOutputHelper _output;

        private EntryController _entryController;

        private Mock<IEntryService> _entryServiceMock;
        private Mock<ILogger<EntryController>> _loggerMock;

        public EntryControllerTest(ITestOutputHelper output)
        {
            _fixture = new Fixture();
            _output = output;

            _entryServiceMock = new Mock<IEntryService>();
            _loggerMock = new Mock<ILogger<EntryController>>();

            _entryController = new EntryController(_entryServiceMock.Object, _loggerMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()), new Claim("custom-claim", "example claim value") }, "mock"));

            _entryController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        #region Index
        [Fact]
        public async Task Index_ValidRequest_ToReturnIndexViewWithList()
        {
            // Arrange
            List<EntryResponse> list = _fixture.Create<List<EntryResponse>>();
            _entryServiceMock.Setup(s => s.SearchEntriesAsync(It.IsAny<EntrySearchModel>(), It.IsAny<EntrySortModel>())).ReturnsAsync(list);
            _entryServiceMock.Setup(s => s.SortEntries(It.IsAny<List<EntryResponse>>(), It.IsAny<EntrySortModel>())).Returns(list);

            // Act
            var result = await _entryController.Index(_fixture.Create<EntrySearchModel>(), _fixture.Create<EntrySortModel>());

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            viewResult.Model.Should().BeAssignableTo<IEnumerable<EntryResponse>>();
        }
        #endregion

        #region Add
        [Fact]
        public async Task Add_InvalidRequest_ToReturnViewWithErrors()
        {
            // Arrange
            ArgumentException exception = new ArgumentException(_fixture.Create<string>());
            _entryServiceMock.Setup(s => s.CreateEntryAsync(It.IsAny<EntryAddRequest>())).ThrowsAsync(exception);

            // Act
            var result = await _entryController.Add(_fixture.Create<EntryAddRequest>());

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            var errorList = viewResult.ViewData["ErrorList"] as string[];
            errorList.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Add_ServerError_ToReturnViewWithErrors()
        {
            // Arrange
            Exception exception = new Exception(_fixture.Create<string>());
            _entryServiceMock.Setup(s => s.CreateEntryAsync(It.IsAny<EntryAddRequest>())).ThrowsAsync(exception);

            // Act
            var result = async () => await _entryController.Add(_fixture.Create<EntryAddRequest>());

            // Assert
            await result.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task Add_ValidRequest_ToReturnIndexView()
        {
            // Arrange
            _entryServiceMock.Setup(s => s.CreateEntryAsync(It.IsAny<EntryAddRequest>())).ReturnsAsync(_fixture.Create<EntryResponse>());

            // Act
            var result = await _entryController.Add(_fixture.Create<EntryAddRequest>());

            // Assert
            RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);
            redirectResult.ActionName.Should().Be("Index");
        }
        #endregion

        #region Update
        [Fact]
        public async Task Update_GET_InvalidId_ToReturnIndex()
        {
            // Arrange
            _entryServiceMock.Setup(s => s.GetEntryAsync(It.IsAny<Guid>())).ReturnsAsync(null as EntryResponse);

            // Act
            var result = await _entryController.Update(_fixture.Create<Guid>());

            // Assert
            RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);
            redirectResult.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Update_GET_ServerError_ToReturnIndex()
        {
            // Arrange
            Exception exception = new(_fixture.Create<string>());
            _entryServiceMock.Setup(s => s.CreateEntryAsync(It.IsAny<EntryAddRequest>())).ThrowsAsync(exception);

            // Act
            var result = await _entryController.Update(_fixture.Create<Guid>());

            // Assert
            RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);
            redirectResult.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Update_GET_ValidRequest_ToReturnIndexView()
        {
            // Arrange
            var entry = _fixture.Create<EntryResponse>();
            _entryServiceMock.Setup(s => s.GetEntryAsync(It.IsAny<Guid>())).ReturnsAsync(entry);

            // Act
            var result = await _entryController.Update(_fixture.Create<Guid>());

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            var entryId = viewResult.ViewData["EntryId"] as Guid?;
            entryId.Should().Be(entry.Id);
        }

        [Fact]
        public async Task Update_POST_InvalidId_ToReturnIndex()
        {
            // Arrange
            _entryServiceMock.Setup(s => s.GetEntryAsync(It.IsAny<Guid>())).ReturnsAsync(null as EntryResponse);

            // Act
            var result = await _entryController.Update(_fixture.Create<EntryUpdateRequest>());

            // Assert
            RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);
            redirectResult.ActionName.Should().Be("Index");
        }
        [Fact]
        public async Task Update_POST_InvalidRequest_ToReturnViewWithErrors()
        {
            // Arrange
            _entryServiceMock.Setup(s => s.GetEntryAsync(It.IsAny<Guid>())).ReturnsAsync(_fixture.Create<EntryResponse>());
            ArgumentException exception = new ArgumentException(_fixture.Create<string>());
            _entryServiceMock.Setup(s => s.UpdateEntryAsync(It.IsAny<EntryUpdateRequest>())).ThrowsAsync(exception);

            // Act
            var result = await _entryController.Update(_fixture.Create<EntryUpdateRequest>());

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            var errorList = viewResult.ViewData["ErrorList"] as string[];
            errorList.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Update_POST_ServerError_ToReturnViewWithErrors()
        {
            // Arrange
            _entryServiceMock.Setup(s => s.GetEntryAsync(It.IsAny<Guid>())).ReturnsAsync(_fixture.Create<EntryResponse>());
            Exception exception = new(_fixture.Create<string>());
            _entryServiceMock.Setup(s => s.UpdateEntryAsync(It.IsAny<EntryUpdateRequest>())).ThrowsAsync(exception);

            // Act
            var result = async () => await _entryController.Update(_fixture.Create<EntryUpdateRequest>());

            // Assert
            await result.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task Update_POST_ValidRequest_ToReturnIndexView()
        {
            // Arrange
            _entryServiceMock.Setup(s => s.GetEntryAsync(It.IsAny<Guid>())).ReturnsAsync(_fixture.Create<EntryResponse>());
            _entryServiceMock.Setup(s => s.UpdateEntryAsync(It.IsAny<EntryUpdateRequest>())).ReturnsAsync(_fixture.Create<EntryResponse>());

            // Act
            var result = await _entryController.Update(_fixture.Create<EntryUpdateRequest>());

            // Assert
            RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);
            redirectResult.ActionName.Should().Be("Index");
        }
        #endregion

        #region Delete
        [Fact]
        public async Task Delete_GET_InvalidId_ToReturnIndex()
        {
            // Arrange
            _entryServiceMock.Setup(s => s.GetEntryAsync(It.IsAny<Guid>())).ReturnsAsync(null as EntryResponse);

            // Act
            var result = await _entryController.Delete(_fixture.Create<Guid>());

            // Assert
            RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);
            redirectResult.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Delete_GET_ServerError_ToReturnIndex()
        {
            // Arrange
            Exception exception = new(_fixture.Create<string>());
            _entryServiceMock.Setup(s => s.GetEntryAsync(It.IsAny<Guid>())).ThrowsAsync(exception);

            // Act
            var result = async () => await _entryController.Delete(_fixture.Create<Guid>());

            // Assert
            await result.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task Delete_GET_ValidRequest_ToReturnIndexView()
        {
            // Arrange
            var entry = _fixture.Create<EntryResponse>();
            _entryServiceMock.Setup(s => s.GetEntryAsync(It.IsAny<Guid>())).ReturnsAsync(entry);

            // Act
            var result = await _entryController.Delete(_fixture.Create<Guid>());

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            var entryId = viewResult.ViewData["EntryId"] as Guid?;
            entryId.Should().Be(entry.Id);
        }

        [Fact]
        public async Task Delete_POST_InvalidId_ToReturnIndex()
        {
            // Arrange
            _entryServiceMock.Setup(s => s.GetEntryAsync(It.IsAny<Guid>())).ReturnsAsync(null as EntryResponse);

            // Act
            var result = await _entryController.Delete(_fixture.Create<EntryUpdateRequest>());

            // Assert
            RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);
            redirectResult.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Delete_POST_ServerError_ToReturnViewWithErrors()
        {
            // Arrange
            _entryServiceMock.Setup(s => s.GetEntryAsync(It.IsAny<Guid>())).ReturnsAsync(_fixture.Create<EntryResponse>());
            Exception exception = new(_fixture.Create<string>());
            _entryServiceMock.Setup(s => s.DeleteEntryAsync(It.IsAny<Guid>())).ThrowsAsync(exception);

            // Act
            var result = async () => await _entryController.Delete(_fixture.Create<EntryUpdateRequest>());

            // Assert
            await result.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task Delete_POST_ValidRequest_ToReturnIndexView()
        {
            // Arrange
            _entryServiceMock.Setup(s => s.GetEntryAsync(It.IsAny<Guid>())).ReturnsAsync(_fixture.Create<EntryResponse>());
            _entryServiceMock.Setup(s => s.DeleteEntryAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            // Act
            var result = await _entryController.Delete(_fixture.Create<EntryUpdateRequest>());

            // Assert
            RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);
            redirectResult.ActionName.Should().Be("Index");
        }
        #endregion

        #region Download
        [Fact]
        public async Task DownloadAsPdf_ReturnsPdf()
        {
            // Arrange
            _entryServiceMock.Setup(s => s.GetEntriesAsync(It.IsAny<Guid>())).ReturnsAsync(_fixture.Create<List<EntryResponse>>());

            // Act
            var result = await _entryController.DownloadAsPdf();

            // Assert
            ViewAsPdf pdfResult = Assert.IsType<ViewAsPdf>(result);
            pdfResult.ViewName.Should().Be("ToPdf");
            pdfResult.FileName?.ToLower().Should().Contain(".pdf");
        }

        [Fact]
        public async Task DownloadAsCsv_ReturnsCsv()
        {
            // Arrange
            var test = new MemoryStream();
            _entryServiceMock.Setup(s => s.GetEntryBytesAsync(It.IsAny<Guid>())).ReturnsAsync(test);

            // Act
            var result = await _entryController.DownloadAsCsv();

            // Assert
            FileStreamResult fileResult = Assert.IsType<FileStreamResult>(result);
            fileResult.ContentType.Should().Be("application/octet-stream");
            fileResult.FileDownloadName.ToLower().Should().Contain(".csv");
        }
        #endregion
    }
}
