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
using Path = System.IO.Path;

namespace KrantenMaker
{
    public static class TempDirInstance
    {
        static TempDirInstance()
        {
            string randomness = "Temp_";
            for (int i = 0; i < 3; i++)
            {
                randomness += (int)(new Random(i).NextDouble() * 10000);
            }
            string tempPath = Path.Combine(Path.GetTempPath(), randomness);
            Directory.CreateDirectory(tempPath);
            path = tempPath;
        }
        public static string path;
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new DataModel();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            // Due to licensing issues, I don't have any ez pz tool that does this all in one step. 
            PdfDocument magazine = new PdfDocument();
            int imax = ((DataModel)DataContext).magazinePages.Count;
            for (int i = 0; i < imax; i++)
            {
                MagazinePage item = ((DataModel)DataContext).magazinePages[i];
                await item.processTask;
                await Task.Run(() =>
                {
                    FileInfo file = new FileInfo(item.processedPath);
                    PdfDocument section = PdfReader.Open(file.FullName, PdfDocumentOpenMode.Import);
                    foreach (var page in section.Pages)
                    {
                        magazine.AddPage(page);
                    }
                    File.Delete(file.FullName);
                });
                progressBar.Value = (int)(100 * ((float)i / (float)imax));
            }
            Directory.Delete(TempDirInstance.path);
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
            string[] files = await GetLocalFiles(currentPath, $"*.{MagazinePage.docextension}");
            foreach (string file in files)
            {
                if (file.ToLower().Contains(MagazinePage.docextension))
                {
                    ((DataModel)DataContext).magazinePages.Add(
                        new MagazinePage(
                            new FileInfo(
                                file
                            ).Name.reversedSkip(MagazinePage.docextension.Length + 1), 
                            Increment.value
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
