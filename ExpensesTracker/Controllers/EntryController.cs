using ExpensesTracker.Models.Enums;
using ExpensesTracker.Services.DTOs;
using ExpensesTracker.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;
using System.Security.Claims;

namespace ExpensesTracker.Controllers
{
    public class EntryController : Controller
    {
        private readonly IEntryService _entryService;
        private readonly ILogger<EntryController> _logger;

        public EntryController(IEntryService entryService, ILogger<EntryController> logger)
        {
            _entryService = entryService;
            _logger = logger;
        }

        [Route("/")]
        [Route("entry")]
        public async Task<IActionResult> Index(EntrySearchModel searchModel, EntrySortModel sortModel)
        {
            try
            {
                if (User != null)
                {
                    searchModel.UserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                }

                if (sortModel.SortBy == 0 && sortModel.OrderBy == 0)
                {
                    sortModel.SortBy = EntrySortOptions.DATE;
                    sortModel.OrderBy = OrderOptions.DESCENDING;
                }

                List<EntryResponse> entries = await _entryService.SearchEntriesAsync(searchModel, sortModel);
                _entryService.UpdatePagination(searchModel);

                ViewBag.CardDisplayModel = await _entryService.UpdateCardDisplayAsync(searchModel);
                ViewBag.SearchModel = searchModel;
                ViewBag.SortModel = sortModel;

                return View(entries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "========== ERROR ==========\n" +
                    "Message: An error occured while processing the Entry/Index view.\n" +
                    "Controller: {ControllerName}\n" +
                    "Action: {ActionName}\n" +
                    "UserId: {UserId}\n"
                    , nameof(EntryController), nameof(Index), searchModel.UserId);

                throw;
            }
        }

        [HttpGet]
        [Route("entry/add")]
        public IActionResult Add()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "========== ERROR ==========\n" +
                    "Message: An error occured while rendering the Entry/Add view.\n" +
                    "Controller: {ControllerName}\n" +
                    "Action: {ActionName}\n" +
                    "UserId: {UserId}\n"
                    , nameof(EntryController), nameof(Add), Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));

                throw;
            }
        }

        [HttpPost]
        [Route("entry/add")]
        public async Task<IActionResult> Add(EntryAddRequest addRequest)
        {
            try
            {
                if (User != null)
                {
                    addRequest.UserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                }

                await _entryService.CreateEntryAsync(addRequest);

                return RedirectToAction("Index", "Entry");
            }
            catch (ArgumentException ex)
            {
                ViewBag.ErrorList = ex.Message.Split("\n");
                ViewBag.AddModel = addRequest;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "========== ERROR ==========\n" +
                    "Message: An error occured while creating a new Entry.\n" +
                    "Controller: {ControllerName}\n" +
                    "Action: {ActionName}\n" +
                    "UserId: {UserId}\n"
                    , nameof(EntryController), nameof(Add), Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));

                throw;
            }
        }

        [HttpGet]
        [Route("entry/update/{Id}")]
        public async Task<IActionResult> Update(Guid Id)
        {
            try
            {
                EntryResponse? entry = await _entryService.GetEntryAsync(Id);

                if (entry == null)
                {
                    return RedirectToAction("Index", "Entry");
                }
                else
                {
                    ViewBag.EntryId = entry.Id;
                    ViewBag.UpdateModel = entry;
                    return View();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "========== ERROR ==========\n" +
                    "Message: An error occured while rendering the Entry/Update view.\n" +
                    "Controller: {ControllerName}\n" +
                    "Action: {ActionName}\n" +
                    "UserId: {UserId}\n" +
                    "EntryId: {EntryId}\n"
                    , nameof(EntryController), nameof(Update), Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)), Id);

                throw;
            }            
        }

        [HttpPost]
        [Route("entry/update/{Id}")]
        public async Task<IActionResult> Update(EntryUpdateRequest updateRequest)
        {
            try
            {
                EntryResponse? entry = await _entryService.GetEntryAsync(updateRequest.Id);

                if (entry == null)
                {
                    return RedirectToAction("Index", "Entry");
                }
                else
                {
                    if (User != null)
                    {
                        updateRequest.UserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    }
                    await _entryService.UpdateEntryAsync(updateRequest);

                    return RedirectToAction("Index", "Entry");
                }
            }
            catch (ArgumentException ex)
            {
                ViewBag.ErrorList = ex.Message.Split("\n");
                ViewBag.UpdateModel = updateRequest;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "========== ERROR ==========\n" +
                    "Message: An error occured while updating an Entry.\n" +
                    "Controller: {ControllerName}\n" +
                    "Action: {ActionName}\n" +
                    "UserId: {UserId}\n" +
                    "EntryId: {EntryId}\n"
                    , nameof(EntryController), nameof(Update), Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)), updateRequest.Id);

                throw;
            }
        }

        [HttpGet]
        [Route("entry/delete/{Id}")]
        public async Task<IActionResult> Delete(Guid Id)
        {
            try
            {
                EntryResponse? entry = await _entryService.GetEntryAsync(Id);

                if (entry == null)
                {
                    return RedirectToAction("Index", "Entry");
                }
                else
                {
                    ViewBag.EntryId = entry.Id;
                    ViewBag.DeleteModel = entry;
                    return View();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "========== ERROR ==========\n" +
                    "Message: An error occured while rendering the Entry/Delete view.\n" +
                    "Controller: {ControllerName}\n" +
                    "Action: {ActionName}\n" +
                    "UserId: {UserId}\n" +
                    "EntryId: {EntryId}\n"
                    , nameof(EntryController), nameof(Delete), Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)), Id);

                throw;
            }
        }

        [HttpPost]
        [Route("entry/delete/{Id}")]
        public async Task<IActionResult> Delete(EntryUpdateRequest deleteRequest)
        {
            try
            {
                EntryResponse? entry = await _entryService.GetEntryAsync(deleteRequest.Id);

                if (entry == null)
                {
                    return RedirectToAction("Index", "Entry");
                }
                else
                {
                    await _entryService.DeleteEntryAsync(deleteRequest.Id);

                    return RedirectToAction("Index", "Entry");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "========== ERROR ==========\n" +
                    "Message: An error occured while deleting an Entry.\n" +
                    "Controller: {ControllerName}\n" +
                    "Action: {ActionName}\n" +
                    "UserId: {UserId}\n" +
                    "EntryId: {EntryId}\n"
                    , nameof(EntryController), nameof(Delete), Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)), deleteRequest.Id);

                throw;
            }
        }

        [HttpGet]
        [Route("entry/downloadAsPdf")]
        public async Task<IActionResult> DownloadAsPdf()
        {
            try
            {
                Guid userId = Guid.Empty;
                if (User != null)
                {
                    userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                }

                var entries = await _entryService.GetEntriesAsync(userId);

                return new ViewAsPdf("ToPdf", entries, ViewData)
                {
                    PageMargins = new Margins() { Top = 20, Right = 10, Bottom = 20, Left = 10 },
                    PageOrientation = Orientation.Landscape,
                    FileName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + "_Entries.pdf"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "========== ERROR ==========\n" +
                    "Message: An error occured while generating PDF.\n" +
                    "Controller: {ControllerName}\n" +
                    "Action: {ActionName}\n" +
                    "UserId: {UserId}\n"
                    , nameof(EntryController), nameof(DownloadAsPdf), Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));

                throw;
            }
        }

        [HttpGet]
        [Route("entry/downloadAsCsv")]
        public async Task<IActionResult> DownloadAsCsv()
        {
            try
            {
                Guid userId = Guid.Empty;
                if (User != null)
                {
                    userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                }

                MemoryStream memoryStream = await _entryService.GetEntryBytesAsync(userId);

                if (memoryStream == null)
                {
                    memoryStream = new MemoryStream();
                }

                return File(memoryStream, "application/octet-stream", DateTime.Now.ToString("yyyyMMddHHmmssffff") + "_Entries.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "========== ERROR ==========\n" +
                    "Message: An error occured while generating CSV.\n" +
                    "Controller: {ControllerName}\n" +
                    "Action: {ActionName}\n" +
                    "UserId: {UserId}\n"
                    , nameof(EntryController), nameof(DownloadAsCsv), Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));

                throw;
            }
        }
    }
}
