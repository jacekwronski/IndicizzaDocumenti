using Novacode;
using Raven.Abstractions.FileSystem;
using Raven.Client;
using Raven.Client.FileSystem;
using Raven.Client.FileSystem.Shard;
using Raven.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

namespace IndicizzaDocumenti
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ReloadFiles();
        }

        private async void ReloadFiles()
        {
            RavenManager ravenManager = new RavenManager();

            var command = new AsyncShardedFilesServerClient(FileStore.ShardStrategy);

            var folders = await command.GetFoldersAsync();


            folders.ToList().ForEach(async x =>
            {
                filesList.Items.Clear();

                var files = await command.GetFilesAsync(x);

                foreach (var file in files.Files.Select(y => y.Name))
                {
                    filesList.Items.Add(file);
                }
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            // Set filter for file extension and default file extension 


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            string country = ((Button)cbCountry.SelectedValue).Content.ToString();
            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                string filename = dlg.FileName;

                using (FileStream fileStream = File.Open(filename, FileMode.Open))
                {
                   
                    var command = new AsyncShardedFilesServerClient(FileStore.ShardStrategy);

                    string ravenFileName = command.UploadAsync(System.IO.Path.GetFileName(filename), new RavenJObject() { { "Country", country } }, fileStream).Result;

                    string content = String.Empty;
                    DocX doc = DocX.Load(fileStream);

                    content = doc.Text;

                    fileStream.Position = 0;


                    using (IDocumentSession session = RavenConnection.DocumentStore.OpenSession())
                    {
                        

                        Documento documento = new Documento()
                        {
                            Titolo = System.IO.Path.GetFileName(filename),
                            Contenuto = content,
                            NomeFile = System.IO.Path.GetFileName(filename),
                            Indirizzo = ravenFileName
                        };

                        session.Store(documento);

                        session.SaveChanges();
                    }

                    ReloadFiles();

                }
            }
        }

        private async void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = ((TextBox)sender).Text;

            RavenManager ravenManager = new RavenManager();

            var array = Task.Factory.StartNew(() => ravenManager.SearchText(RavenConnection.DocumentStore, text)).Result;

            lstFiles.Items.Clear();

            array.ForEach(x => { lstFiles.Items.Add(x.Indirizzo); });

            //sugg.Text = Task.Factory.StartNew(() => ravenManager.Suggestions(RavenConnection.DocumentStore, text)).Result;

        }


        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();

            saveFileDialog.ShowDialog();

            string locazioneFile = String.Empty;

            if (lstFiles.SelectedItem != null)
                locazioneFile = lstFiles.SelectedValue.ToString();
            if (filesList.SelectedItem != null)
                locazioneFile = filesList.SelectedValue.ToString();

            Stream file = null;

            var command = new AsyncShardedFilesServerClient(FileStore.ShardStrategy);

            file = command.DownloadAsync(locazioneFile).Result;


            if (String.IsNullOrEmpty(saveFileDialog.FileName) == false)
            {
                string fileName = saveFileDialog.FileName;

                using (FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    file.CopyTo(stream);
                    stream.Close();
                    file.Close();
                }
            }
        }

        private void lstFiles_MouseUp(object sender, MouseButtonEventArgs e)
        {
            lstFiles.Focus();
            filesList.SelectedItem = null;
        }

        private void filesList_MouseUp(object sender, MouseButtonEventArgs e)
        {
            filesList.Focus();
            lstFiles.SelectedItem = null;
        }

        private void button_Click_2(object sender, RoutedEventArgs e)
        {
            string locazioneFile = String.Empty;

            if (lstFiles.SelectedItem != null)
                locazioneFile = lstFiles.SelectedValue.ToString();
            if (filesList.SelectedItem != null)
                locazioneFile = filesList.SelectedValue.ToString();

            using (IDocumentSession session = RavenConnection.DocumentStore.OpenSession())
            {
                Documento documento = session.Query<Documento>().Where(x => x.Indirizzo == locazioneFile).FirstOrDefault();

                session.Delete<Documento>(documento);

                var command = new AsyncShardedFilesServerClient(FileStore.ShardStrategy);

                command.DeleteAsync(documento.Indirizzo);

                session.SaveChanges();
            }

            ReloadFiles();


        }
    }
}
