using GongSolutions.Wpf.DragDrop;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Path = System.IO.Path;

namespace KrantenMaker
{
    public class MagazinePage
    {
        public int id { get; set; }
        public string filename { get; set; }
        public string processedPath { get; set; }
        public Task processTask { get; set; }
        public static string docextension = "docx"; // lowercase plz
        public MagazinePage(string filename, int index)
        {
            this.id = index;
            this.filename = filename;
            this.processTask = Task.Run(() =>
            {
                string temp_file_name = "temp_pdf_page_";
                FileInfo file = new FileInfo($"{filename}.{docextension}");
                var wordApp = new Microsoft.Office.Interop.Word.Application();
                var doc = wordApp.Documents.Open(file.FullName, false, true, false);
                this.processedPath = Path.Combine(TempDirInstance.path, $"{temp_file_name}{index++}.pdf");
                doc.ExportAsFixedFormat(
                    this.processedPath,
                    Microsoft.Office.Interop.Word.WdExportFormat.wdExportFormatPDF
                );
                wordApp.Quit(false);
            });
        }
    }
}
