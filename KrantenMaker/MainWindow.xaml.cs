using GongSolutions.Wpf.DragDrop;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using Path = System.IO.Path;

namespace KrantenMaker
{
    public partial class MainWindow : Window
    {
        private const string docextension = "docx"; // lowercase plz
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new DataModel();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            string temp_file_name = "temp_pdf_page_";
            int index = 0;
            // Due to licensing issues, I don't have any ez pz tool that does this all in one step. 
            // 1. Convert every page to a seperate pdf document
            int imax = ((DataModel)DataContext).magazinePages.Count;
            string randomness = "Temp_";
            for (int i = 0; i < 3; i++)
            {
                randomness += (int)(new Random(i).NextDouble() * 10000);
            }
            string tempPath = Path.Combine(Path.GetTempPath(), randomness);
            Directory.CreateDirectory(tempPath);
            for (int i = 0; i < imax; i++)
            {
                MagazinePage item = ((DataModel)DataContext).magazinePages[i];
                await Task.Run(() =>
                {
                    FileInfo file = new FileInfo($"{item.filename}.{docextension}");
                    var wordApp = new Microsoft.Office.Interop.Word.Application();
                    var doc = wordApp.Documents.Open(file.FullName, false, true, false);
                    doc.ExportAsFixedFormat(
                        Path.Combine(tempPath, $"{temp_file_name}{index++}.pdf"),
                        Microsoft.Office.Interop.Word.WdExportFormat.wdExportFormatPDF
                    );
                    wordApp.Quit(false);
                });
                progressBar.Value = (int)(80 * ((float)i / (float)imax));
            }
            // 2. Concatenate all the pdf pages
            PdfDocument magazine = new PdfDocument();
            for (int i = 0; i < index; i++)
            {
                await Task.Run(() =>
                {
                    FileInfo file = new FileInfo(Path.Combine(tempPath, $"{temp_file_name}{i}.pdf"));
                    PdfDocument section = PdfReader.Open(file.FullName, PdfDocumentOpenMode.Import);
                    foreach (var page in section.Pages)
                    {
                        magazine.AddPage(page);
                    }
                    File.Delete(file.FullName);
                });
                progressBar.Value = 20 + (int)(50 * ((float)i / (float)index));
            }
            Directory.Delete(tempPath);
            magazine.Save(
                Path.Combine(
                    Directory.GetCurrentDirectory(), 
                    "Magazine.pdf"
                )
            );
            progressBar.Value = 100;
            ((Button)sender).Content = "Klaar!";
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                button.IsEnabled = false;
                await Task.Run(()=>
                {
                    var wordApp = new Microsoft.Office.Interop.Word.Application();
                    wordApp.Quit();
                });
            } 
            catch (Exception ex)
            {
                MessageBox.Show("Dit programma werkt alleen op computers waar Microsoft Word op staat");
                Environment.Exit(0);
            }

            string currentPath = Directory.GetCurrentDirectory();
            string[] files = await GetLocalFiles(currentPath, $"*.{docextension}");
            foreach (string file in files)
            {
                if (file.ToLower().Contains(docextension))
                {
                    ((DataModel)DataContext).magazinePages.Add(
                        new MagazinePage(
                            new FileInfo(
                                file
                            ).Name.reversedSkip(docextension.Length + 1)
                        )
                    );
                }
            }
        }

        private async Task<string[]> GetLocalFiles(string path, string searchPattern)
        {
            string localPath = path;
            string localSearchPattern = searchPattern;
            return await Task<string[]>.Run(() =>
            {
                return Directory.GetFiles(localPath, localSearchPattern);
            });
        }
    }
    public static class StringExtensions
    {
        public static string reversedSkip(this string txt, int count)
        {
            return txt.Substring(0, txt.Length - count);
        }
    }
}
