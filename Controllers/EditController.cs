using Microsoft.AspNetCore.Mvc;
using PDFForge.Services;
using System.IO;
using System.Threading.Tasks;

namespace PDFForge.Controllers
{
    public class EditController : Controller
    {
        private readonly IPdfService _pdfService;
        private readonly ILogger<EditController> _logger;

        public EditController(IPdfService pdfService, ILogger<EditController> logger)
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
        public async Task<IActionResult> Upload(IFormFile? file)
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
                    HttpContext.Session.SetString("EditFileName", fileName);
                    HttpContext.Session.SetString("EditPdfContentBase64", Convert.ToBase64String(pdfBytes));
                    
                    return Json(new { success = true, pageCount = pageCount, fileName = fileName });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading file: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RotatePage([FromBody] RotateRequest? request)
        {
            try
            {
                var pdfBase64 = HttpContext.Session.GetString("EditPdfContentBase64");
                var fileName = HttpContext.Session.GetString("EditFileName");

                if (string.IsNullOrEmpty(pdfBase64))
                    return BadRequest("No PDF uploaded");

                var pdfContent = Convert.FromBase64String(pdfBase64);

                if (request == null || request.PageNumber <= 0 || request.Rotation == 0)
                    return BadRequest("Invalid page number or rotation");

                var rotatedPdf = await _pdfService.RotatePage(pdfContent, request.PageNumber, request.Rotation);
                
                // Update session with modified PDF
                HttpContext.Session.SetString("EditPdfContentBase64", Convert.ToBase64String(rotatedPdf));
                
                return Json(new { success = true, message = "Page rotated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error rotating page: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpGet]
        public IActionResult GetPdf()
        {
            try
            {
                var pdfBase64 = HttpContext.Session.GetString("EditPdfContentBase64");
                if (string.IsNullOrEmpty(pdfBase64))
                    return BadRequest("No PDF available");

                var pdfContent = Convert.FromBase64String(pdfBase64);
                return File(pdfContent, "application/pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error serving PDF: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult Download()
        {
            try
            {
                var pdfBase64 = HttpContext.Session.GetString("EditPdfContentBase64");
                var fileName = HttpContext.Session.GetString("EditFileName");

                if (string.IsNullOrEmpty(pdfBase64))
                    return BadRequest("No PDF available");

                var pdfContent = Convert.FromBase64String(pdfBase64);
                return File(pdfContent, "application/pdf", $"{fileName}_edited.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error downloading file: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        public class RotateRequest
        {
            public int PageNumber { get; set; }
            public int Rotation { get; set; }
        }
    }
}
