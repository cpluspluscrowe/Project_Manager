using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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


namespace Project_Manager
{
    /// <summary>
    /// Interaction logic for Task_Manager.xaml
    /// </summary>
    public partial class Task_Manager : Window
    {
        public int ProjectID;
        public int ArchivedMode;
        public string ProjectTitle;
        Dictionary<string, FrameworkElement> Controls;
        public Task_Manager(int projectID,int archivedMode)
        {
            this.ProjectID = projectID;
            this.ArchivedMode = archivedMode;
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
                (SELECT PROJECT_TASK_MANAGER_DIM_FK from [Project_Notes].[dbo].[PROJECT] WHERE ID = @projectId);
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



            this.Controls = new Dictionary<string, FrameworkElement>();


            ColumnDefinition gridCol0 = new ColumnDefinition();
            gridCol0.Width = new GridLength(4, GridUnitType.Star);
            gridCol0.AllowDrop = true;
            MainTaskManagerGrid.ColumnDefinitions.Add(gridCol0);
            //add the two columns, not in for loop
            for (int j = 0; j < 3; j++)
            {
                ColumnDefinition gridCol1 = new ColumnDefinition();
                gridCol1.Width = new GridLength(.7, GridUnitType.Star);
                gridCol1.AllowDrop = true;
                MainTaskManagerGrid.ColumnDefinitions.Add(gridCol1);
            }


            int i = 0;

            RowDefinition gridRow2 = new RowDefinition();
            gridRow2.Height = new GridLength(1, GridUnitType.Star);
            gridRow2.AllowDrop = true;
            MainTaskManagerGrid.RowDefinitions.Add(gridRow2); //add row 0

            System.Windows.Controls.Button addBtn = new Button();
            addBtn.Content = "Add Task";
            Grid.SetRow(addBtn, i);
            Grid.SetColumn(addBtn, 0);
            Grid.SetColumnSpan(addBtn, 4);
            addBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddTaskButton));
            addBtn.AllowDrop = true;
            MainTaskManagerGrid.Children.Add(addBtn);
            this.Controls[addBtn.Name] = addBtn;

            i = 1;

            RowDefinition gridRow3 = new RowDefinition();
            gridRow3.Height = new GridLength(1, GridUnitType.Star);
            gridRow3.AllowDrop = true;
            MainTaskManagerGrid.RowDefinitions.Add(gridRow3); //add row 0

            System.Windows.Controls.Button backBtn = new Button();
            backBtn.Content = "Back";
            Grid.SetRow(backBtn, i);
            Grid.SetColumn(backBtn, 0);
            Grid.SetColumnSpan(backBtn, 4);
            backBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(BackButton));
            backBtn.AllowDrop = true;
            MainTaskManagerGrid.Children.Add(backBtn);
            this.Controls[backBtn.Name] = backBtn;

            i++;

            using (SqlConnection conn = new SqlConnection("Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open(); //insert log, the creation_date is added by default
                string sql = @"
                DECLARE @ID INT;
                SET @ID = (SELECT ID from [Project_Notes].[dbo].[PROJECT] WHERE ID = @projectId);--THIS IS THE CURRENT PROJECT'S ID
                SELECT TASK_TITLE,TASK_DESCRIPTION,ID,ARCHIVED FROM TASKS WHERE PROJECT_ID = @ID;--GRAB NOTES BELONGING TO THIS PROJECT
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
                            gridRow0.Height = new GridLength(1, GridUnitType.Star);
                            gridRow0.AllowDrop = true;
                            MainTaskManagerGrid.RowDefinitions.Add(gridRow0); //add row 0

                            Border myBorder = new Border();
                            myBorder.Background = Brushes.AntiqueWhite;
                            Grid.SetRow(myBorder, i);
                            Grid.SetColumn(myBorder, 0);
                            Grid.SetColumnSpan(myBorder, 1);
                            myBorder.AllowDrop = true;
                            MainTaskManagerGrid.Children.Add(myBorder);

                            StackPanel lv = new StackPanel();
                            lv.MinHeight = 30;
                            Grid.SetRow(lv, i);
                            Grid.SetColumn(lv, 0);
                            Grid.SetColumnSpan(lv, 1);
                            lv.AllowDrop = true;
                            MainTaskManagerGrid.Children.Add(lv);


                            System.Windows.Controls.TextBox titletb = new TextBox();
                            titletb.MinHeight = 15;
                            titletb.Height = double.NaN;
                            titletb.Name = "titletb" + colVals[2].ToString();
                            titletb.TextWrapping = TextWrapping.Wrap;
                            if (colVals[0].ToString().Length > 4 && colVals[0].ToString() != "Note Title...")
                            {
                                titletb.Text = colVals[0].ToString().Substring(0, colVals[0].ToString().Length - 2);
                            }
                            else
                            {
                                titletb.Text = colVals[0].ToString();
                            }
                            titletb.IsReadOnly = true;
                            titletb.Tag = colVals[2].ToString();
                            lv.Children.Add(titletb);
                            
                            /*
                            Grid.SetRow(titletb, 2 * i);
                            Grid.SetColumn(titletb, 0);
                            Grid.SetColumnSpan(titletb, 5);
                            titletb.PreviewMouseDown += TitleClick;
                            //titletb.PreviewMouseUp += TitleUpClick;
                            titletb.AllowDrop = true;
                            MainTaskManagerGrid.Children.Add(titletb);
                             * */
                            this.Controls[titletb.Name] = titletb;



                            //PreviewMouseDown="UcTextBox_PreviewMouseDown" 

                            System.Windows.Controls.TextBox contenttb = new TextBox();
                            contenttb.Name = "contenttb" + colVals[2].ToString();
                            contenttb.Height = double.NaN;
                            contenttb.MinHeight = 15;
                            contenttb.TextWrapping = TextWrapping.Wrap;
                            if (colVals[1].ToString().Length > 4 && colVals[1].ToString() != "Note Content goes here...")
                            {
                                contenttb.Text = colVals[1].ToString().Substring(0, colVals[1].ToString().Length - 2);
                            }
                            else
                            {
                                contenttb.Text = colVals[1].ToString();
                            }
                            contenttb.IsReadOnly = true;
                            contenttb.Tag = colVals[2].ToString();
                            lv.Children.Add(contenttb);
                            /*
                            Grid.SetRow(contenttb, 2 * i + 1);
                            Grid.SetColumn(contenttb, 0);
                            Grid.SetColumnSpan(contenttb, 8);
                            Grid.SetRowSpan(contenttb, 1);
                            contenttb.PreviewMouseDown += ContentClick;
                            //contenttb.PreviewMouseUp += ContentUpClick;
                            contenttb.AllowDrop = true;
                            MainTaskManagerGrid.Children.Add(contenttb);
                             * */
                            this.Controls[contenttb.Name] = contenttb;


                            System.Windows.Controls.Button removeBtn = new Button();
                            removeBtn.Content = "Remove";
                            removeBtn.Name = "RemoveButton" + colVals[2].ToString();
                            removeBtn.Tag = colVals[2].ToString();
                            Grid.SetRow(removeBtn, i);
                            Grid.SetColumn(removeBtn, 1);
                            removeBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(RemoveButton));
                            MainTaskManagerGrid.Children.Add(removeBtn);
                            this.Controls[removeBtn.Name] = removeBtn;


                            System.Windows.Controls.Button archiveBtn = new Button();
                            archiveBtn.Content = "Archive";
                            archiveBtn.Name = "ArchiveButton" + colVals[2].ToString();
                            archiveBtn.Tag = colVals[2].ToString();
                            Grid.SetRow(archiveBtn, i);
                            Grid.SetColumn(archiveBtn, 2);
                            archiveBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(ArchiveButton));
                            archiveBtn.AllowDrop = true;
                            MainTaskManagerGrid.Children.Add(archiveBtn);
                            this.Controls[archiveBtn.Name] = archiveBtn;

                            System.Windows.Controls.Button saveBtn = new Button();
                            saveBtn.Content = "Edit";
                            saveBtn.Name = "EditButton" + colVals[2].ToString();
                            saveBtn.Tag = colVals[2].ToString();
                            Grid.SetRow(saveBtn, i);
                            Grid.SetColumn(saveBtn, 3);
                            saveBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(EditButton));
                            saveBtn.AllowDrop = true;
                            MainTaskManagerGrid.Children.Add(saveBtn);
                            this.Controls[saveBtn.Name] = saveBtn;



                            i++;
                        }
                    }
                }
            }
            if (ArchivedMode == 1)
            {
                /*//add extra row for deleting the archived project
                RowDefinition gridRowLast = new RowDefinition();
                gridRowLast.Height = new GridLength(20, GridUnitType.Star);
                MainTaskManagerGrid.RowDefinitions.Add(gridRowLast); //add row 0


                System.Windows.Controls.Button deleteBtn = new Button();
                deleteBtn.Content = "Delete Project";
                deleteBtn.Tag = "Delete" + this.ProjectID;
                Grid.SetRow(deleteBtn, 5);
                Grid.SetColumn(deleteBtn, 0);
                Grid.SetColumnSpan(deleteBtn, 8);

                deleteBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(DeleteButton));
                deleteBtn.Background = new SolidColorBrush(Colors.GhostWhite);
                MainTaskManagerGrid.Children.Add(deleteBtn);


                this.Title = this.Title + "- Archived";
                IEnumerable<Button> collection = MainTaskManagerGrid.Children.OfType<Button>();
                foreach (var button in collection)
                {
                    button.Background = new SolidColorBrush(Colors.GhostWhite);
                }*/
            }

        }



        private void AddTaskButton(object sender, RoutedEventArgs args)
        {
            Tasks addTaskWindow = new Tasks(this.ProjectID, this.ArchivedMode);
            App.Current.MainWindow = addTaskWindow;
            this.Close();
            addTaskWindow.Show();
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

        private void RemoveButton(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;//this is the button I clicked
            string titleString = "titletb" + button.Tag.ToString();//extra
            var title = (TextBox)this.Controls["titletb" + button.Tag.ToString()];
            var content = (TextBox)this.Controls["contenttb" + button.Tag.ToString()];

            string noteTitle = title.Text;
            string noteContent = content.Text;

            int taskId = Int32.Parse(button.Name.Replace("RemoveButton", ""));

            using (
                SqlConnection conn =
                    new SqlConnection(
                        "Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open(); //insert log, the creation_date is added by default
                string sql = @"
                DELETE TASKS WHERE ID = @taskId";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@taskId",taskId);
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
            var title = (TextBox)this.Controls["titletb" + button.Tag.ToString()];
            var content = (TextBox)this.Controls["contenttb" + button.Tag.ToString()];

            string noteTitle = title.Text;
            string noteContent = content.Text;

            int taskId = Int32.Parse(button.Name.Replace("ArchiveButton", ""));

            using (
                SqlConnection conn =
                    new SqlConnection(
                        "Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open(); //insert log, the creation_date is added by default
                string sql = @"
                UPDATE TASKS SET ARCHIVED = 1 WHERE ID = @taskId
                ";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@taskId", taskId);
                    cmd.ExecuteNonQuery();

                }
            }
            //Now reassembly this window
            Projects projectWindow = new Projects(this.ProjectID, this.ArchivedMode);//PASS AROUND THE PROJECT ID
            App.Current.MainWindow = projectWindow;
            this.Close();
            projectWindow.Show();
        }

        private void EditButton(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int taskId = Int32.Parse(button.Name.Replace("EditButton", ""));
            EditTask editTaskWindow = new EditTask(this.ProjectID, this.ArchivedMode,taskId);
            App.Current.MainWindow = editTaskWindow;
            this.Close();
            editTaskWindow.Show();
        }


        private void BackButton(object sender, RoutedEventArgs e)
        {
            Projects projectWindow = new Projects(this.ProjectID, this.ArchivedMode);//PASS AROUND THE PROJECT ID
            App.Current.MainWindow = projectWindow;
            this.Close();
            projectWindow.Show();
        }


        private void Task_Manager_OnClosed(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection("Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open(); //insert log, the creation_date is added by default
                string sql = @"
                UPDATE [Project_Notes].[dbo].[DIMENSIONS] SET HEIGHT = @height,WIDTH = @width,TOPDIM = @top,LEFTDIM = @left WHERE ID = 
                (SELECT PROJECT_TASK_MANAGER_DIM_FK from [Project_Notes].[dbo].[PROJECT] WHERE ID = @projectId)
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
