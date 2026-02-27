# PDF Forge - Advanced PDF Manipulation Tool

A comprehensive .NET Core MVC application for PDF manipulation with modern UI and interactive features.

## Features

### 📄 Split PDF
- Upload PDF documents
- Preview all pages with responsive grid layout
- Select multiple pages to extract
- Download extracted PDF with selected pages only
- Supports various page selection modes (select all, deselect all)

### 🔗 Merge PDF
- Upload multiple PDF files simultaneously
- **Drag-and-drop** interface to rearrange pages
- Visual page ordering with easy reordering
- Merge multiple PDFs into a single document
- Maintains page order as arranged

### ✏️ Edit PDF
- Rotate individual pages (90°, 180°, 270°)
- Page-by-page navigation with sidebar
- Visual page selection
- Download edited PDF with applied changes

## Technology Stack

- **Framework**: ASP.NET Core MVC (.NET 10.0)
- **PDF Library**: iTextSharp 5.5.13.3
- **Frontend**: Bootstrap 5, JavaScript
- **Session Management**: Distributed Memory Cache
- **UI Library**: Font Awesome Icons

## Project Structure

```
PDFForge/
├── Controllers/
│   ├── HomeController.cs       # Dashboard and landing page
│   ├── SplitController.cs      # PDF split operations
│   ├── MergeController.cs      # PDF merge operations
│   └── EditController.cs       # PDF edit/rotate operations
├── Services/
│   └── PdfService.cs           # PDF manipulation logic
├── Views/
│   ├── Home/
│   │   └── Index.cshtml        # Dashboard with feature overview
│   ├── Split/
│   │   └── Index.cshtml        # Split PDF interface
│   ├── Merge/
│   │   └── Index.cshtml        # Merge PDF interface with drag-drop
│   ├── Edit/
│   │   └── Index.cshtml        # Edit PDF interface
│   └── Shared/
│       └── _Layout.cshtml      # Master layout with navigation
├── wwwroot/                    # Static files (CSS, JS)
├── Program.cs                  # Application configuration
└── PDFForge.csproj            # Project file
```

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later ([Download](https://dotnet.microsoft.com/download))
- Windows, macOS, or Linux

### Installation & Setup

1. **Navigate to the project directory**:
   ```bash
   cd "E:\PDF Forge\PDFForge"
   ```

2. **Restore NuGet packages** (if not already done):
   ```bash
   dotnet restore
   ```

3. **Build the project**:
   ```bash
   dotnet build
   ```

4. **Run the application**:
   ```bash
   dotnet run
   ```

5. **Access the application**:
   - Open your browser and navigate to: `https://localhost:5001`
   - The application will start on the secure HTTPS port

## Usage Guide

### Splitting PDF

1. Navigate to **Split** section from the menu
2. Upload your PDF by dragging or clicking the upload area
3. View page thumbnails in the grid
4. Click individual pages to select them (selected pages show green highlight)
5. Use **Select All** / **Deselect All** buttons for bulk selection
6. Click **Download Split PDF** to save

### Merging PDFs

1. Go to **Merge** section
2. Upload multiple PDF files (select multiple files at once)
3. Files will appear in a list
4. **Drag pages to reorder** - simply click and drag to rearrange
5. Click **Merge PDFs** to combine in the arranged order
6. Download the merged document

### Editing PDF

1. Open **Edit** section
2. Upload a PDF file
3. Use the sidebar to navigate between pages
4. Select rotation angle (90°, 180°, or 270°)
5. Changes are applied immediately
6. Click **Download Edited PDF** to save

## API Endpoints

### Split Controller
- `GET /Split` - Display split interface
- `POST /Split/Upload` - Upload PDF for splitting
- `POST /Split/SplitPages` - Process split request

### Merge Controller
- `GET /Merge` - Display merge interface
- `POST /Merge/UploadFiles` - Upload multiple PDFs
- `POST /Merge/MergePdfs` - Process merge request

### Edit Controller
- `GET /Edit` - Display edit interface
- `POST /Edit/Upload` - Upload PDF for editing
- `POST /Edit/RotatePage` - Rotate specific page
- `POST /Edit/Download` - Download edited PDF

## Session Management

The application uses ASP.NET Core session management to temporarily store:
- PDF files (as Base64-encoded strings)
- File names and metadata
- User selections

**Session Settings**:
- Idle timeout: 30 minutes
- Storage: Distributed Memory Cache
- Cookie: HttpOnly, Essential

> Files are stored only in session memory and are automatically cleared after session expiration or browser close.

## Key Features Explained

### Drag-and-Drop Merge
- **User Experience**: Intuitive file reordering
- **Visual Feedback**: Hover effects and active states
- **Touch Support**: Works on desktop and mobile browsers

### Page Selection
- **Visual Indicators**: Clear selection highlighting
- **Bulk Operations**: Select/Deselect all pages
- **Page Preview**: File icon with page numbers

### Session-Based Processing
- **Security**: Files stored temporarily in memory
- **Performance**: Fast file operations
- **Privacy**: Automatic cleanup after session ends

## Styling & UI

- **Responsive Design**: Works on desktop, tablet, and mobile
- **Modern UI**: Gradient header with smooth transitions
- **Accessibility**: High contrast color scheme
- **Icons**: Font Awesome 6.4.0 for intuitive controls

### Color Scheme
- **Primary**: Purple/Blue Gradient (#667eea, #764ba2)
- **Success**: Green (#28a745) for selections
- **Neutral**: Light gray (#f8f9fa) for backgrounds

## Error Handling

The application includes comprehensive error handling:
- File upload validation
- PDF format verification
- Page number validation
- Session timeout handling
- Friendly error messages to users

## Troubleshooting

### Application won't start
```bash
# Check if port 5001 is in use
# Try running on a different port:
dotnet run --urls="https://localhost:5002"
```

### PDF upload fails
- Ensure file is a valid PDF
- Check file size (typically under 50MB)
- Verify browser allows file uploads

### Session expires
- Session timeout is 30 minutes of inactivity
- Start a new operation to create a fresh session

## Performance Optimization

- Asynchronous file processing
- Memory streaming for large files
- Efficient PDF page indexing
- Session-based caching

## Browser Compatibility

- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

## Security Considerations

- Files are stored only in session memory
- No persistent storage on server
- HTTPS enforced
- HttpOnly cookies for sessions
- Input validation on all endpoints

## Future Enhancements

- [ ] PDF compression
- [ ] Watermark support
- [ ] PDF password protection
- [ ] OCR capabilities
- [ ] Batch processing
- [ ] Database persistence option
- [ ] User authentication
- [ ] Multiple file format support

## License

MIT License - Feel free to use and modify

## Support

For issues or features requests, please create an issue in the project repository.

## Notes

- **iTextSharp**: An older but stable PDF library. For production environments with higher security requirements, consider upgrading to iText 7 or iText Core.
- **.NET 10.0**: The project uses the latest .NET framework. Ensure your SDK is up to date.
- **Dependencies**: Some NuGet packages have known vulnerabilities. For production use, consider using alternative libraries like PdfSharpCore or QuestPDF.

---

**Created**: February 26, 2026
**Version**: 1.0.0
**Status**: Fully Functional
