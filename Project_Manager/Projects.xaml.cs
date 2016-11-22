using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
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
using Outlook = Microsoft.Office.Interop.Outlook;
using Path = System.IO.Path;
namespace Project_Manager
{
    /// <summary>
    /// Interaction logic for Projects.xaml
    /// </summary>
    public partial class Projects : Window
    {
        public int ArchivedMode;
        public Dictionary<string, FrameworkElement> Controls; 
        public int ProjectID;
        public string ProjectTitle;
        public Point startPoint;
        public Projects(int projectID,int archivedMode)
        {
            this.ArchivedMode = archivedMode;
            this.ProjectID = projectID;
            

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
                    }
                }
            }
            
            //must initialize xaml values before creating the gui!
            InitializeComponent();

            using (SqlConnection conn = new SqlConnection("Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open(); //insert log, the creation_date is added by default
                string sql = @"
                SELECT HEIGHT,WIDTH,TOPDIM,LEFTDIM FROM [Project_Notes].[dbo].[DIMENSIONS] WHERE ID =
                (SELECT PROJECT_MANAGER_DIM_FK from [Project_Notes].[dbo].[PROJECT] WHERE ID = @projectId);
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




            if (this.ArchivedMode == 1)
            {
                this.ArchiveProject.Content = "Restore Project";
            }
            this.Controls = new Dictionary<string, FrameworkElement>();
            

            int i = 0;
            using (SqlConnection conn = new SqlConnection("Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open(); //insert log, the creation_date is added by default
                string sql = @"
                DECLARE @ID INT;
                SET @ID = (SELECT ID from [Project_Notes].[dbo].[PROJECT] WHERE ID = @projectId);--THIS IS THE CURRENT PROJECT'S ID
                SELECT NOTE_TITLE,NOTE_CONTENT,ID,ARCHIVED FROM NOTES WHERE PROJECT_ID = @ID;--GRAB NOTES BELONGING TO THIS PROJECT
                ";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@projectId", this.ProjectID);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        object[] colVals = new object[4];//two items
                        reader.GetValues(colVals);//put into colVals

                        if (bool.Parse(colVals[3].ToString()) == false || this.ArchivedMode == 1)//only display if not archived, unless the whole project is archived.
                        {

                            RowDefinition gridRow0 = new RowDefinition();
                            //gridRow0.MinHeight = 20;
                            gridRow0.Height = new GridLength(1, GridUnitType.Star);
                            ProjectNotes.AllowDrop = true;
                            ProjectNotes.RowDefinitions.Add(gridRow0); //add row 0

                            RowDefinition gridRow1 = new RowDefinition();
                            gridRow1.Height = new GridLength(3, GridUnitType.Star);
                            //gridRow1.MinHeight = 20;
                            ProjectNotes.RowDefinitions.Add(gridRow1); //add row 1

                            System.Windows.Controls.RichTextBox titletb = new RichTextBox();
                            titletb.Name = "titletb" + colVals[2].ToString();
                            if (colVals[0].ToString().Length > 4 && colVals[0].ToString() != "Note Title...")
                            {
                                titletb.AppendText(colVals[0].ToString().Substring(0, colVals[0].ToString().Length - 2));
                            }
                            else
                            {
                                titletb.AppendText(colVals[0].ToString());
                            }
                            titletb.Tag = colVals[2].ToString();
                            Grid.SetRow(titletb, 2*i);
                            Grid.SetColumn(titletb, 0);
                            Grid.SetColumnSpan(titletb, 5);
                            titletb.PreviewMouseDown += TitleClick;
                            //titletb.PreviewMouseUp += TitleUpClick;
                            titletb.AllowDrop = true;
                            ProjectNotes.Children.Add(titletb);
                            this.Controls[titletb.Name] = titletb;
                            


                            //PreviewMouseDown="UcTextBox_PreviewMouseDown" 

                            System.Windows.Controls.RichTextBox contenttb = new RichTextBox();
                            contenttb.Name = "contenttb" + colVals[2].ToString();
                            if (colVals[1].ToString().Length > 4 && colVals[1].ToString() != "Note Content goes here...")
                            {
                                contenttb.AppendText(colVals[1].ToString().Substring(0, colVals[1].ToString().Length - 2));
                            }
                            else
                            {
                                contenttb.AppendText(colVals[1].ToString());
                            }
                            contenttb.Tag = colVals[2].ToString();
                            Grid.SetRow(contenttb, 2*i + 1);
                            Grid.SetColumn(contenttb, 0);
                            Grid.SetColumnSpan(contenttb, 8);
                            Grid.SetRowSpan(contenttb, 1);
                            contenttb.PreviewMouseDown += ContentClick;
                            //contenttb.PreviewMouseUp += ContentUpClick;
                            contenttb.AllowDrop = true;
                            ProjectNotes.Children.Add(contenttb);
                            this.Controls[contenttb.Name] = contenttb;


                            System.Windows.Controls.Button removeBtn = new Button();
                            removeBtn.Content = "Remove";
                            removeBtn.Name = "RemoveButton" + colVals[2].ToString();
                            removeBtn.Tag = colVals[2].ToString();
                            Grid.SetRow(removeBtn, 2 * i);
                            Grid.SetColumn(removeBtn, 6);
                            Grid.SetZIndex(removeBtn, 1);
                            removeBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(RemoveButton));
                            ProjectNotes.Children.Add(removeBtn);
                            this.Controls[removeBtn.Name] = removeBtn;


                            System.Windows.Controls.Button archiveBtn = new Button();
                            archiveBtn.Content = "Archive";
                            archiveBtn.Name = "ArchiveButton" + colVals[2].ToString();
                            archiveBtn.Tag = colVals[2].ToString();
                            Grid.SetRow(archiveBtn, 2*i);
                            Grid.SetColumn(archiveBtn, 7);
                            //int delindex = Grid.GetZIndex(archiveBtn);
                            archiveBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(ArchiveButton));
                            archiveBtn.AllowDrop = true;
                            ProjectNotes.Children.Add(archiveBtn);
                            this.Controls[archiveBtn.Name] = archiveBtn;

                            System.Windows.Controls.Button saveBtn = new Button();
                            saveBtn.Content = "Save";
                            saveBtn.Name = "SaveButton" + colVals[2].ToString();
                            saveBtn.Tag = colVals[2].ToString();
                            Grid.SetRow(saveBtn, 2*i);
                            Grid.SetColumn(saveBtn, 5);
                            //Grid.SetZIndex(deleteBtn, 1);
                            saveBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(SaveButton));
                            saveBtn.AllowDrop = true;
                            ProjectNotes.Children.Add(saveBtn);
                            this.Controls[saveBtn.Name] = saveBtn;

                            if (ArchivedMode == 1)
                            {
                                saveBtn.Background = new SolidColorBrush(Colors.GhostWhite);
                                archiveBtn.Background = new SolidColorBrush(Colors.GhostWhite);
                            }
                            i++;
                        }
                    }
                }
            }
            if (ArchivedMode == 1)
            {
                //add extra row for deleting the archived project
                RowDefinition gridRowLast = new RowDefinition();
                gridRowLast.Height = new GridLength(20, GridUnitType.Star);
                MainProjectGrid.RowDefinitions.Add(gridRowLast); //add row 0


                System.Windows.Controls.Button deleteBtn = new Button();
                deleteBtn.Content = "Delete Project";
                deleteBtn.Tag = "Delete" + this.ProjectID;
                Grid.SetRow(deleteBtn, 6);
                Grid.SetColumn(deleteBtn, 0);
                Grid.SetColumnSpan(deleteBtn,8);

                deleteBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(DeleteButton));
                deleteBtn.Background = new SolidColorBrush(Colors.GhostWhite);
                MainProjectGrid.Children.Add(deleteBtn);

                var backButton = (Button) this.FindName("BackButton");
                backButton.Background = new SolidColorBrush(Colors.GhostWhite);

                this.Title = this.Title + "- Archived";
                IEnumerable<Button> collection = MainProjectGrid.Children.OfType<Button>();
                foreach (var button in collection)
                {
                    button.Background = new SolidColorBrush(Colors.GhostWhite);
                }


            }

        }


        /*private void List_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Store the mouse position
            this.startPoint = e.GetPosition(null);
        }

        private void List_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                // Get the dragged ListViewItem

                // Find the data behind the ListViewItem
                string contact = "Yes";

                // Initialize the drag & drop operation
                DataObject dragData = new DataObject("myFormat", e);
                DragDrop.DoDragDrop(MainProjectGrid, dragData, DragDropEffects.Move);
            }
        }*/



        private void DeleteButton(object sender, RoutedEventArgs args)
        {
            using (
                SqlConnection conn =
                    new SqlConnection(
                        "Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open(); //insert log, the creation_date is added by default
                string sql = String.Format(@"
                DELETE FROM LOGS WHERE TASK_ID IN (SELECT ID FROM TASKS WHERE PROJECT_ID = '{0}');
                DELETE FROM TASKS WHERE PROJECT_ID = '{0}';
                DELETE FROM NOTES WHERE PROJECT_ID = '{0}';
                DELETE FROM PROJECT WHERE ID = '{0}';
                ", this.ProjectID);
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();

                }
            }
            this.Button_Click(sender,args);//back button
        }


        private void TitleClick(object sender, RoutedEventArgs args)
        {
            RichTextBox rtb = (RichTextBox)sender;//this is the button I clicked
            string titleString = rtb.Name;//extra
            var title = (RichTextBox)this.Controls["titletb" + rtb.Tag.ToString()];

            TextRange noteTitle = new TextRange(title.Document.ContentStart, title.Document.ContentEnd);
            if (noteTitle.Text.Contains("Note Title..."))
            {
                rtb.Document.Blocks.Clear();
                rtb.Focus();
            }

        }

        private void ContentClick(object sender, RoutedEventArgs args)
        {
            RichTextBox rtb = (RichTextBox)sender;//this is the button I clicked
            string titleString = rtb.Name;//extra
            var content = (RichTextBox)this.Controls["contenttb" + rtb.Tag.ToString()];

            TextRange noteTitle = new TextRange(content.Document.ContentStart, content.Document.ContentEnd);
            if (noteTitle.Text.Contains("Note Content goes here..."))
            {
                rtb.Document.Blocks.Clear();
                rtb.Focus();
            }
        }

        private void Files_Click(object sender, RoutedEventArgs args)
        {
            Files fileWindow = new Files(this.ProjectID, this.ArchivedMode);
            App.Current.MainWindow = fileWindow;
            this.Close();
            fileWindow.Show();
        }

        private void RemoveButton(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;//this is the button I clicked
            string titleString = "titletb" + button.Tag.ToString();//extra
            var title = (RichTextBox)this.Controls["titletb" + button.Tag.ToString()];
            var content = (RichTextBox)this.Controls["contenttb" + button.Tag.ToString()];

            TextRange noteTitle = new TextRange(title.Document.ContentStart, title.Document.ContentEnd);
            TextRange noteContent = new TextRange(content.Document.ContentStart, content.Document.ContentEnd);

            int noteId = Int32.Parse(button.Name.Replace("RemoveButton", ""));

            using (
                SqlConnection conn =
                    new SqlConnection(
                        "Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open(); //insert log, the creation_date is added by default
                string sql = String.Format(@"
                DELETE NOTES WHERE ID = '{2}'
                ", noteTitle.Text, noteContent.Text, noteId);
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();

                }
            }
            //Now reassembly this window
            Projects projectWindow = new Projects(this.ProjectID, this.ArchivedMode);//PASS AROUND THE PROJECT ID
            App.Current.MainWindow = projectWindow;
            this.Close();
            projectWindow.Show();
        }


        private void ArchiveButton(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;//this is the button I clicked
            string titleString = "titletb" + button.Tag.ToString();//extra
            var title = (RichTextBox)this.Controls["titletb" + button.Tag.ToString()];
            var content = (RichTextBox)this.Controls["contenttb" + button.Tag.ToString()];

            TextRange noteTitle = new TextRange(title.Document.ContentStart, title.Document.ContentEnd);
            TextRange noteContent = new TextRange(content.Document.ContentStart, content.Document.ContentEnd);

            int noteId = Int32.Parse(button.Name.Replace("ArchiveButton", ""));

            using (
                SqlConnection conn =
                    new SqlConnection(
                        "Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open(); //insert log, the creation_date is added by default
                string sql = @"
                UPDATE NOTES SET ARCHIVED = 1 WHERE ID = @noteId
                ";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@noteId", noteId);
                    cmd.ExecuteNonQuery();

                }
            }
            //Now reassembly this window
            Projects projectWindow = new Projects(this.ProjectID,this.ArchivedMode);//PASS AROUND THE PROJECT ID
            App.Current.MainWindow = projectWindow;
            this.Close();
            projectWindow.Show();
        }

        private void SaveButton(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;//this is the button I clicked
            string titleString = "titletb" + button.Tag.ToString();//extra
            var title = (RichTextBox)this.Controls["titletb" + button.Tag.ToString()];
            var content = (RichTextBox)this.Controls["contenttb" + button.Tag.ToString()];

            TextRange noteTitle = new TextRange(title.Document.ContentStart, title.Document.ContentEnd);
            TextRange noteContent = new TextRange(content.Document.ContentStart, content.Document.ContentEnd);

            int noteId = Int32.Parse(button.Name.Replace("SaveButton",""));

            using (
                SqlConnection conn =
                    new SqlConnection(
                        "Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open(); //insert log, the creation_date is added by default
                string sql = @"
                UPDATE NOTES SET NOTE_TITLE = @noteTitleText, NOTE_CONTENT = @noteContentText WHERE ID = @noteId
                ";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@noteTitleText", noteTitle.Text);
                    cmd.Parameters.Add("@noteContentText", noteContent.Text);
                    cmd.Parameters.Add("@noteId", noteId);
                    try
                    {
                        cmd.ExecuteNonQuery();
                        //Will create the database Project_Notes if it does not already exist.
                    }
                    catch (Exception excp)
                    {
                        MessageBox.Show("Project Name cannot be empty");
                    }

                }
            }

        }


        public string getProjectName { get { return this.ProjectTitle; } }

        private void LogNavigate(object sender, RoutedEventArgs e)
        {
            Logs logsWindow = new Logs(this.ProjectID,this.ArchivedMode,0);
            App.Current.MainWindow = logsWindow;
            this.Close();
            logsWindow.Show();
        }
        private void TaskNavigate(object sender, RoutedEventArgs e)
        {
            Task_Manager taskWindow = new Task_Manager(this.ProjectID, this.ArchivedMode);
            App.Current.MainWindow = taskWindow;
            this.Close();
            taskWindow.Show();
        }
        private void WeeklyReportNavigate(object sender, RoutedEventArgs e)
        {
            WeeklyReport weeklyReportWindow = new WeeklyReport(this.ProjectID, this.ArchivedMode);
            App.Current.MainWindow = weeklyReportWindow;
            this.Close();
            weeklyReportWindow.Show();
        }

        private void Note_Content_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void InfoButtonClick(object sender, RoutedEventArgs e)
        {
            Info infoWindow = new Info(this.ProjectID,this.ArchivedMode);
            App.Current.MainWindow = infoWindow;
            this.Close();
            infoWindow.Show();
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow(this.ArchivedMode);
            App.Current.MainWindow = mainWindow;
            this.Close();
            mainWindow.Show();
        }

        private void AddNoteClick(object sender, RoutedEventArgs e)
        {
            using (SqlConnection conn = new SqlConnection("Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open(); //insert log, the creation_date is added by default
                string sql = @"
                INSERT INTO NOTES(NOTE_TITLE,NOTE_CONTENT,PROJECT_ID) VALUES('Note Title...','Note Content goes here...',@projectId);--creates an empty note that refers to this project
                ";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@projectId", this.ProjectID);
                    cmd.ExecuteNonQuery(); //Will create the database Project_Notes if it does not already exist.
                }
            }
            //Now reassembly this window
            Projects projectWindow = new Projects(this.ProjectID,this.ArchivedMode);//PASS AROUND THE PROJECT ID
            App.Current.MainWindow = projectWindow;
            this.Close();
            projectWindow.Show();
        }

        private void ArchiveButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.ArchivedMode == 0)
            {
                using (
                    SqlConnection conn =
                        new SqlConnection(
                            "Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
                {
                    conn.Open(); //insert log, the creation_date is added by default
                    string sql = @"
                UPDATE PROJECT SET ARCHIVED = 1 WHERE ID = @projectId
                ";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.Add("@projectId", this.ProjectID);
                        cmd.ExecuteNonQuery();
                    }
                }
                //now go to the main window
                MainWindow mainWindow = new MainWindow(0);
                App.Current.MainWindow = mainWindow;
                this.Close();
                mainWindow.Show();
            }
            else
            {
                using (
                    SqlConnection conn =
                        new SqlConnection(
                            "Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
                {
                    conn.Open(); //insert log, the creation_date is added by default
                    string sql = @"
                UPDATE PROJECT SET ARCHIVED = 0 WHERE ID = @projectId
                ";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.Add("@projectId", this.ProjectID);
                        cmd.ExecuteNonQuery();
                    }
                }
                //now go to the main window
                MainWindow mainWindow = new MainWindow(1);
                App.Current.MainWindow = mainWindow;
                this.Close();
                mainWindow.Show();
            }

        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Projects_OnClosed(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection("Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open(); //insert log, the creation_date is added by default
                string sql = @"
                UPDATE [Project_Notes].[dbo].[DIMENSIONS] SET HEIGHT = @height,WIDTH = @width,TOPDIM = @top,LEFTDIM = @left WHERE ID = 
                (SELECT PROJECT_MANAGER_DIM_FK from [Project_Notes].[dbo].[PROJECT] WHERE ID = @projectId)
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
