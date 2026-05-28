
using NPOI.SS.UserModel;
using NPOI.XWPF.UserModel;
using System.Text;
using UglyToad.PdfPig;
using ICell = NPOI.SS.UserModel.ICell;

namespace BaleAiBotV2.Helpers
{
    public static class DocumentTextExtractor
    {
        public static string ExtractText(byte[] fileBytes, string fileName)
        {
            string extension = Path.GetExtension(fileName ?? "").ToLower();

            try
            {
                using var memoryStream = new MemoryStream(fileBytes);

                switch (extension)
                {
                    case ".pdf":
                        return ExtractPdfText(fileBytes);

                    case ".docx":
                        return ExtractWordText(memoryStream);

                    case ".xlsx":
                    case ".xls":
                        return ExtractExcelText(memoryStream);

                    case ".txt":
                    case ".csv":
                        return Encoding.UTF8.GetString(fileBytes);

                    default:
                        return "فرمت فایل پشتیبانی نمی‌شود.";
                }
            }
            catch (System.Exception ex)
            {
                return $"خطا در خواندن محتوای فایل: {ex.Message}";
            }
        }

        private static string ExtractPdfText(byte[] pdfBytes)
        {
            var text = new StringBuilder();
            using (PdfDocument document = PdfDocument.Open(pdfBytes))
            {
                foreach (var page in document.GetPages())
                {
                    text.AppendLine(page.Text);
                }
            }
            return text.ToString();
        }

        private static string ExtractWordText(Stream stream)
        {
            var text = new StringBuilder();
            XWPFDocument doc = new XWPFDocument(stream);
            foreach (var para in doc.Paragraphs)
            {
                text.AppendLine(para.ParagraphText);
            }
            return text.ToString();
        }

        private static string ExtractExcelText(Stream stream)
        {
            var text = new StringBuilder();
            IWorkbook workbook = WorkbookFactory.Create(stream);

            for (int i = 0; i < workbook.NumberOfSheets; i++)
            {
                ISheet sheet = workbook.GetSheetAt(i);
                text.AppendLine($"--- Sheet: {sheet.SheetName} ---");

                for (int rowIndex = 0; rowIndex <= sheet.LastRowNum; rowIndex++)
                {
                    IRow row = sheet.GetRow(rowIndex);
                    if (row == null) continue;

                    var rowData = new System.Collections.Generic.List<string>();
                    for (int colIndex = 0; colIndex < row.LastCellNum; colIndex++)
                    {
                        ICell cell = row.GetCell(colIndex);
                        rowData.Add(cell?.ToString() ?? "");
                    }
                    text.AppendLine(string.Join(" | ", rowData));
                }
            }
            return text.ToString();
        }
    }
}
