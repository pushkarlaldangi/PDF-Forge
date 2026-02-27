using iTextSharp.text.pdf;
using iTextSharp.text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace PDFForge.Services
{
    public interface IPdfService
    {
        Task<byte[]> SplitPdf(byte[] pdfContent, List<int> pageNumbers);
        Task<byte[]> MergePdfs(List<byte[]> pdfContents);
        Task<byte[]> RotatePage(byte[] pdfContent, int pageNumber, int rotation);
        Task<List<PdfPagePreview>> GetPdfPreview(byte[] pdfContent);
        int GetPageCount(byte[] pdfContent);
    }

    public class PdfPagePreview
    {
        public int PageNumber { get; set; }
        public string? Base64Image { get; set; }
        public string? PageSize { get; set; }
    }

    public class PdfService : IPdfService
    {
        /// <summary>
        /// Split PDF and keep only specified pages
        /// </summary>
        public async Task<byte[]> SplitPdf(byte[] pdfContent, List<int> pageNumbers)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var sourceStream = new MemoryStream(pdfContent))
                    using (var destinationStream = new MemoryStream())
                    {
                        sourceStream.Position = 0;
                        var reader = new PdfReader(sourceStream);
                        
                        var document = new Document(reader.GetPageSizeWithRotation(1));
                        var writer = PdfWriter.GetInstance(document, destinationStream);
                        document.Open();

                        for (int i = 0; i < pageNumbers.Count; i++)
                        {
                            int pageNum = pageNumbers[i];
                            if (pageNum > 0 && pageNum <= reader.NumberOfPages)
                            {
                                if (i > 0) document.NewPage();
                                var page = writer.GetImportedPage(reader, pageNum);
                                writer.DirectContent.AddTemplate(page, 0, 0);
                            }
                        }

                        document.Close();
                        writer.Close();
                        reader.Close();
                        
                        return destinationStream.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error splitting PDF: {ex.Message}", ex);
                }
            });
        }

        /// <summary>
        /// Merge multiple PDFs into one
        /// </summary>
        public async Task<byte[]> MergePdfs(List<byte[]> pdfContents)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var destinationStream = new MemoryStream())
                    {
                        List<PdfReader> readers = new List<PdfReader>();
                        int totalPages = 0;

                        // Create readers and count pages
                        foreach (var pdfContent in pdfContents)
                        {
                            var sourceStream = new MemoryStream(pdfContent);
                            sourceStream.Position = 0;
                            var reader = new PdfReader(sourceStream);
                            readers.Add(reader);
                            totalPages += reader.NumberOfPages;
                        }

                        // Create document and merge
                        var document = new Document(readers[0].GetPageSizeWithRotation(1));
                        var writer = PdfWriter.GetInstance(document, destinationStream);
                        document.Open();

                        int pageNum = 0;
                        foreach (var reader in readers)
                        {
                            for (int i = 1; i <= reader.NumberOfPages; i++)
                            {
                                if (pageNum > 0) document.NewPage();
                                var page = writer.GetImportedPage(reader, i);
                                writer.DirectContent.AddTemplate(page, 0, 0);
                                pageNum++;
                            }
                        }

                        document.Close();
                        writer.Close();

                        // Clean up readers
                        foreach (var reader in readers)
                        {
                            reader.Close();
                        }

                        return destinationStream.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error merging PDFs: {ex.Message}", ex);
                }
            });
        }

        /// <summary>
        /// Rotate a specific page in the PDF
        /// </summary>
        public async Task<byte[]> RotatePage(byte[] pdfContent, int pageNumber, int rotation)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var sourceStream = new MemoryStream(pdfContent))
                    using (var destinationStream = new MemoryStream())
                    {
                        sourceStream.Position = 0;
                        var reader = new PdfReader(sourceStream);

                        // PdfStamper edits the PDF in-place, so rotation changes
                        // are correctly written to the page dictionary.
                        var stamper = new PdfStamper(reader, destinationStream);

                        var pageDic = reader.GetPageN(pageNumber);
                        int currentRotation = reader.GetPageRotation(pageNumber);
                        int newRotation = (currentRotation + rotation) % 360;
                        pageDic.Put(PdfName.Rotate, new PdfNumber(newRotation));

                        stamper.Close();
                        reader.Close();

                        return destinationStream.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error rotating PDF: {ex.Message}", ex);
                }
            });
        }

        /// <summary>
        /// Get preview of all pages in PDF with thumbnail images
        /// </summary>
        public async Task<List<PdfPagePreview>> GetPdfPreview(byte[] pdfContent)
        {
            return await Task.Run(() =>
            {
                var previews = new List<PdfPagePreview>();
                try
                {
                    using (var sourceStream = new MemoryStream(pdfContent))
                    {
                        sourceStream.Position = 0;
                        var reader = new PdfReader(sourceStream);
                        for (int i = 1; i <= reader.NumberOfPages; i++)
                        {
                            var rect = reader.GetPageSizeWithRotation(i);
                            previews.Add(new PdfPagePreview
                            {
                                PageNumber = i,
                                PageSize = $"{rect.Width:F0}x{rect.Height:F0}"
                            });
                        }
                        reader.Close();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error generating PDF previews: {ex.Message}", ex);
                }
                return previews;
            });
        }

        /// <summary>
        /// Get total number of pages in PDF
        /// </summary>
        public int GetPageCount(byte[] pdfContent)
        {
            try
            {
                using (var sourceStream = new MemoryStream(pdfContent))
                {
                    sourceStream.Position = 0;
                    var reader = new PdfReader(sourceStream);
                    return reader.NumberOfPages;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Invalid PDF file: {ex.Message}", ex);
            }
        }
    }
}
