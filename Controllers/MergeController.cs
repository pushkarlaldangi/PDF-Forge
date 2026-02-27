using Microsoft.AspNetCore.Mvc;
using PDFForge.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PDFForge.Controllers
{
    public class MergeController : Controller
    {
        private readonly IPdfService _pdfService;
        private readonly ILogger<MergeController> _logger;

        public MergeController(IPdfService pdfService, ILogger<MergeController> logger)
        {
            _pdfService = pdfService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadFiles()
        {
            var files = Request.Form.Files;
            if (files.Count == 0)
                return BadRequest("No files uploaded");

            try
            {
                var pdfList = new List<byte[]>();
                var fileNames = new List<string>();

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await file.CopyToAsync(memoryStream);
                            pdfList.Add(memoryStream.ToArray());
                            fileNames.Add(Path.GetFileNameWithoutExtension(file.FileName));
                        }
                    }
                }

                // Store as byte arrays in a temporary cache or session with a key
                var sessionKey = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("MergeSessionKey", sessionKey);
                
                // Store file names
                HttpContext.Session.SetString("UploadedFileNames", string.Join("|", fileNames));
                
                // Store PDFs - convert to base64 for session storage
                var pdfBase64List = pdfList.Select(p => Convert.ToBase64String(p)).ToList();
                HttpContext.Session.SetString("UploadedPdfsBase64", JsonSerializer.Serialize(pdfBase64List));

                return Json(new { 
                    success = true, 
                    count = pdfList.Count, 
                    files = fileNames 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading files: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> MergePdfs([FromBody] MergeRequest? request)
        {
            try
            {
                var pdfBase64Json = HttpContext.Session.GetString("UploadedPdfsBase64");
                if (string.IsNullOrEmpty(pdfBase64Json))
                    return BadRequest("No PDFs uploaded");

                var pdfBase64List = JsonSerializer.Deserialize<List<string>>(pdfBase64Json);
                if (pdfBase64List == null || pdfBase64List.Count == 0)
                    return BadRequest("Session data corrupted");

                // Convert from Base64 back to byte arrays
                var pdfList = pdfBase64List.Select(p => Convert.FromBase64String(p)).ToList();

                // Reorder based on request
                var orderedPdfs = new List<byte[]>();
                if (request?.PageOrder != null && request.PageOrder.Count > 0)
                {
                    foreach (var index in request.PageOrder)
                    {
                        if (index >= 0 && index < pdfList.Count)
                            orderedPdfs.Add(pdfList[index]);
                    }
                }
                else
                {
                    orderedPdfs = pdfList;
                }

                var mergedPdf = await _pdfService.MergePdfs(orderedPdfs);
                
                return File(mergedPdf, "application/pdf", "merged.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error merging PDFs: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddFiles()
        {
            var files = Request.Form.Files;
            if (files.Count == 0)
                return BadRequest("No files uploaded");

            try
            {
                // Load existing session data
                var existingBase64Json = HttpContext.Session.GetString("UploadedPdfsBase64");
                var existingNamesRaw = HttpContext.Session.GetString("UploadedFileNames");

                var pdfBase64List = existingBase64Json != null
                    ? JsonSerializer.Deserialize<List<string>>(existingBase64Json) ?? new List<string>()
                    : new List<string>();

                var fileNames = existingNamesRaw != null
                    ? existingNamesRaw.Split('|').ToList()
                    : new List<string>();

                var newFileNames = new List<string>();

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        using var memoryStream = new MemoryStream();
                        await file.CopyToAsync(memoryStream);
                        pdfBase64List.Add(Convert.ToBase64String(memoryStream.ToArray()));
                        var name = Path.GetFileNameWithoutExtension(file.FileName);
                        fileNames.Add(name);
                        newFileNames.Add(name);
                    }
                }

                HttpContext.Session.SetString("UploadedFileNames", string.Join("|", fileNames));
                HttpContext.Session.SetString("UploadedPdfsBase64", JsonSerializer.Serialize(pdfBase64List));

                return Json(new { success = true, newFiles = newFileNames });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding files: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        public class MergeRequest
        {
            public List<int>? PageOrder { get; set; }
        }
    }
}

