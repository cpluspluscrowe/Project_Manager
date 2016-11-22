using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ListViewDemo;
using Outlook = Microsoft.Office.Interop.Outlook;


namespace Project_Manager
{
    /// <summary>
    /// Interaction logic for Files.xaml
    /// </summary>
    public partial class Files : Window
    {
        public int ProjectID;
        public int ArchivedMode;
        public string ProjectTitle;
        public Files(int projectID, int archivedMode)
        {
            this.ProjectID = projectID;
            this.ArchivedMode = archivedMode;
            InitializeComponent();

            using (SqlConnection conn = new SqlConnection("Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open(); //insert log, the creation_date is added by default
                string sql = @"
                SELECT HEIGHT,WIDTH,TOPDIM,LEFTDIM FROM [Project_Notes].[dbo].[DIMENSIONS] WHERE ID =
                (SELECT PROJECT_FILE_DIM_FK from [Project_Notes].[dbo].[PROJECT] WHERE ID = @projectId);
                ";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@projectId", this.ProjectID);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        object[] colVals = new object[4];//4 items
                        reader.GetValues(colVals);
                        this.Height = Double.Parse(colVals[0].ToString());
                        this.Width = Double.Parse(colVals[1].ToString());
                        this.Top = Double.Parse(colVals[2].ToString());
                        this.Left = Double.Parse(colVals[3].ToString());
                    }
                }
            }

            DataContext = new FileList();
            
            List<string> files = new List<string>();

            using (SqlConnection conn = new SqlConnection("Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open(); //insert log, the creation_date is added by default
                string sql = String.Format(@"
                            SELECT filepath from [Project_Notes].[dbo].[FILES] WHERE Project_ID = @projectId order by CREATION_DATE DESC;
                            ").Replace("\r\n", "");
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@projectId", this.ProjectID);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        object[] colVals = new object[300];//two items
                        reader.GetValues(colVals);

                        foreach (var instance in colVals)
                        {
                            if (instance != null)
                            {
                                files.Add(instance.ToString());
                            }
                        }
                    }
                }
            }

            int length = files.Count;

            using (SqlConnection conn = new SqlConnection("Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open(); //insert log, the creation_date is added by default
                string sql = String.Format(@"
                SELECT PROJECT_TITLE from [Project_Notes].[dbo].[PROJECT] WHERE ID = @projectId;
                ").Replace("\r\n", "");
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@projectId", this.ProjectID);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        object[] colVals = new object[1];//two items
                        reader.GetValues(colVals);
                        this.ProjectTitle = colVals[0].ToString();

                        RowDefinition gridRow0 = new RowDefinition();
                        //gridRow0.MinHeight = 20;
                        gridRow0.Height = new GridLength(1, GridUnitType.Star);
                        MainFileGrid.AllowDrop = true;
                        MainFileGrid.RowDefinitions.Add(gridRow0); //add row 0

                        RowDefinition gridRow1 = new RowDefinition();
                        //gridRow0.MinHeight = 20;
                        gridRow1.Height = new GridLength(4, GridUnitType.Star);
                        MainFileGrid.AllowDrop = true;
                        MainFileGrid.RowDefinitions.Add(gridRow1); //add row 0


                      
                        System.Windows.Controls.Label projectLabel = new Label();
                        projectLabel.Name = "projectLabel";
                        projectLabel.Content = this.ProjectTitle;
                        Grid.SetRow(projectLabel, 0);
                        Grid.SetColumn(projectLabel, 0);
                        Grid.SetColumnSpan(projectLabel, 1);
                        projectLabel.AllowDrop = true;
                        MainFileGrid.Children.Add(projectLabel);

                        ScrollViewer sv = new ScrollViewer();
                        sv.Name = "scrollViewer";
                        //Grid.SetRow(sv, 0);
                        //Grid.SetColumn(sv, 0);
                        //Grid.SetColumnSpan(sv, 2);
                        //sv.AllowDrop = true;
                        //MainFileGrid.Children.Add(sv);


                        ListView lv = new ListView();
                        Grid.SetRow(lv, 1);
                        Grid.SetColumn(projectLabel, 0);
                        Grid.SetColumnSpan(projectLabel, 1);
                        projectLabel.AllowDrop = true;
                        MainFileGrid.Children.Add(lv);

                        foreach (var file in files)
                        {
                            Label tb = new Label();
                            tb.Content = System.IO.Path.GetFileName(file.ToString());
                            tb.Tag = file.ToString();
                            tb.PreviewMouseDown += tbClick;
                            lv.Items.Add(tb);
                        }

                        sv.Content = projectLabel;

                        RowDefinition gridRow2 = new RowDefinition();
                        //gridRow0.MinHeight = 20;
                        gridRow2.Height = new GridLength(1, GridUnitType.Star);
                        MainFileGrid.AllowDrop = true;
                        MainFileGrid.RowDefinitions.Add(gridRow2); //add row 0


                        System.Windows.Controls.Button backBtn = new Button();
                        backBtn.Name = "BackButton";
                        backBtn.Content = "Back";
                        Grid.SetRow(backBtn, 2);
                        backBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(BackButton));
                        backBtn.AllowDrop = true;
                        MainFileGrid.Children.Add(backBtn);
                    }
                }
            }
        }

        private void tbClick(object sender, RoutedEventArgs args)
        {
            Label tb = (Label)sender;
            string documentPath = tb.Tag.ToString();

            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                System.Diagnostics.Process.Start(documentPath);
            }
            else
            {
                if (MessageBox.Show("Delete Selected File?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {

                    using (SqlConnection conn = new SqlConnection("Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
                    {
                        conn.Open(); //insert log, the creation_date is added by default
                        string sql = String.Format(@"
                            delete from [Project_Notes].[dbo].[FILES] WHERE Project_ID = @projectId and filepath = @filePath;
                            ").Replace("\r\n", "");
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.Add("@projectId", this.ProjectID);
                            cmd.Parameters.Add("@filePath", documentPath);
                            cmd.ExecuteNonQuery();
                            if (File.Exists(documentPath))
                            {
                                File.Delete(documentPath);
                            }

                            Files fileWindow = new Files(this.ProjectID, this.ArchivedMode);
                            App.Current.MainWindow = fileWindow;
                            this.Close();
                            fileWindow.Show();
                        }
                    }
                }
                else
                {
                    //leave alone
                }
            }
        }

        private void BackButton(object sender, RoutedEventArgs args)
        {
            Projects projectWindow = new Projects(this.ProjectID, this.ArchivedMode);//PASS AROUND THE PROJECT ID
            App.Current.MainWindow = projectWindow;
            this.Close();
            projectWindow.Show();
        }

        private void ImagePanel_Drop(object sender, DragEventArgs e)
        {

            string[] FileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            if (FileList == null)
            {
                Outlook.Application outlook = new Outlook.Application();
                Outlook.Explorer oExplorer = outlook.ActiveExplorer();
                Outlook.Selection oSelection = oExplorer.Selection;
                string baseProjDir = System.IO.Path.Combine("C:\\Project_Notes\\Stored_Content",
                    this.ProjectID.ToString());
                if (!Directory.Exists(baseProjDir))
                {
                    Directory.CreateDirectory(baseProjDir);
                }

                foreach (object item in oSelection)
                {
                    Outlook.MailItem mi = (Outlook.MailItem)item;
                    string filePath = System.IO.Path.Combine(baseProjDir, mi.Subject);

                    using (
                        SqlConnection conn =
                            new SqlConnection(
                                "Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
                    {
                        conn.Open(); //insert log, the creation_date is added by default
                        string sql = @"
                INSERT INTO FILES(filepath,Project_ID) VALUES(@filepath,@projectId);
                ";
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            string cd = mi.CreationTime.ToString("MM-dd-yy HH:mm");
                            string inside = mi.Subject + " (" + cd + ").msg";
                            inside = inside.Replace("/", "").Replace(":", "-");
                            string saveString = System.IO.Path.Combine(baseProjDir, inside);
                            cmd.Parameters.Add("@filepath", saveString);
                            cmd.Parameters.Add("@projectId", this.ProjectID);
                            mi.SaveAs(saveString, Outlook.OlSaveAsType.olMSG);
                            mi.Save();
                            if (File.Exists(saveString))
                            {
                                cmd.ExecuteNonQuery();
                            }
                            else
                            {
                                MessageBox.Show("Failed to save email");
                            }

                        }
                    }
                }
            }
            else if (FileList[0] != null)
            {

                string baseProjDir = System.IO.Path.Combine("C:\\Project_Notes\\Stored_Content",
                    this.ProjectID.ToString());
                if (!Directory.Exists(baseProjDir))
                {
                    Directory.CreateDirectory(baseProjDir);
                }


                foreach (var file in FileList)
                {
                    string filePath = System.IO.Path.Combine(baseProjDir,
                        System.IO.Path.GetFileName(file.ToString()).Replace("/", "").Replace(":", ""));

                    using (
                        SqlConnection conn =
                            new SqlConnection(
                                "Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
                    {
                        conn.Open(); //insert log, the creation_date is added by default
                        string sql = @"
                INSERT INTO FILES(filepath,Project_ID) VALUES(@filepath,@projectId);
                ";
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.Add("@filepath", filePath);
                            cmd.Parameters.Add("@projectId", this.ProjectID);
                            File.Copy(file, filePath, overwrite: true);
                            cmd.ExecuteNonQuery();
                        }
                    }

                }
            }
            Files fileWindow = new Files(this.ProjectID, this.ArchivedMode);
            App.Current.MainWindow = fileWindow;
            this.Close();
            fileWindow.Show();
        }

        private void Files_OnClosed(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection("Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open(); //insert log, the creation_date is added by default
                string sql = @"
                UPDATE [Project_Notes].[dbo].[DIMENSIONS] SET HEIGHT = @height,WIDTH = @width,TOPDIM = @top,LEFTDIM = @left WHERE ID = 
                (SELECT PROJECT_FILE_DIM_FK from [Project_Notes].[dbo].[PROJECT] WHERE ID = @projectId)
                ";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@projectId", this.ProjectID);
                    cmd.Parameters.Add("@height", this.Height);
                    cmd.Parameters.Add("@width", this.Width);
                    cmd.Parameters.Add("@top", this.Top);
                    cmd.Parameters.Add("@left", this.Left);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        object[] colVals = new object[4];//4 items
                        reader.GetValues(colVals);
                        this.Height = Double.Parse(colVals[0].ToString());
                        this.Width = Double.Parse(colVals[1].ToString());
                        this.Top = Double.Parse(colVals[2].ToString());
                        this.Left = Double.Parse(colVals[3].ToString());
                    }
                }
            }
        }
    }

}
