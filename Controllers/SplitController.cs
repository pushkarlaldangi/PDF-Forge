using Microsoft.AspNetCore.Mvc;
using PDFForge.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PDFForge.Controllers
{
    public class SplitController : Controller
    {
        private readonly IPdfService _pdfService;
        private readonly ILogger<SplitController> _logger;

        public SplitController(IPdfService pdfService, ILogger<SplitController> logger)
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
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            try
            {
                var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    var pdfBytes = memoryStream.ToArray();
                    var pageCount = _pdfService.GetPageCount(pdfBytes);
                    
                    // Store in session as Base64
                    HttpContext.Session.SetString("FileName", fileName);
                    HttpContext.Session.SetString("PdfContentBase64", Convert.ToBase64String(pdfBytes));
                    
                    return Json(new 
                    { 
                        success = true, 
                        pageCount = pageCount, 
                        fileName = fileName
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading file: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SplitPages([FromBody] SplitRequest? request)
        {
            try
            {
                var pdfBase64 = HttpContext.Session.GetString("PdfContentBase64");
                var fileName = HttpContext.Session.GetString("FileName");

                if (string.IsNullOrEmpty(pdfBase64))
                    return BadRequest("No PDF uploaded");

                var pdfContent = Convert.FromBase64String(pdfBase64);
                var pageNumbers = request?.PageNumbers ?? new List<int>();

                if (pageNumbers.Count == 0)
                    return BadRequest("Please select at least one page");

                var splitPdf = await _pdfService.SplitPdf(pdfContent, pageNumbers);
                
                return File(splitPdf, "application/pdf", $"{fileName}_split.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error splitting PDF: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        public class SplitRequest
        {
            public List<int>? PageNumbers { get; set; }
        }
    }
}
