using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace ListViewDemo
{
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Windows;

    public class FileList
    {
        public FileList()
        {
            Files = new ObservableCollection<FileInfo>();
        }

        public ObservableCollection<FileInfo> Files { get; private set; }

        public string Path { get; set; }

        public void RefreshFiles()
        {
            try
            {
                Files.Clear();
                foreach (FileInfo file in new DirectoryInfo(Path).GetFiles())
                {
                    Files.Add(file);
                }
            }
            catch
            {
                MessageBox.Show("Unable to read folder", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
