using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Project_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            MainWindow mw = new MainWindow(0);
            this.Close();
            mw.Show(); 
        }
        public int ArchivedMode;
        public Dictionary<string, FrameworkElement> Controls; 
        public MainWindow(int archived = 0)
        {
            this.ArchivedMode = archived;
            InitializeComponent();


            this.Controls = new Dictionary<string, FrameworkElement>();
            if (this.ArchivedMode == 1)
            {
                IEnumerable<Button> collection = MainWindowGrid.Children.OfType<Button>();
                foreach (var button in collection)
                {
                    button.Background = new SolidColorBrush(Colors.GhostWhite);
                }
            }
            
            if (this.ArchivedMode == 0)
            {
                ArchiveSwitch.Content = "Archives";
                this.Title = "Project Manager";
                PM_ProjectLabel.Content = "Available Projects";
            }
            else
            {
                ArchiveSwitch.Content = "Current Projects";
                this.Title = "Archived Project Manager";
                PM_ProjectLabel.Content = "Archived Projects";
            }

            //This next part creates the directory and creates the database
            if (!Directory.Exists("C:\\Project_Notes"))
            {
                Directory.CreateDirectory("C:\\Project_Notes");
            }
            using (SqlConnection conn =
                new SqlConnection(
                    "Server=(LocalDB)\\MSSQLLocalDB;Integrated Security = true"))
            {
                conn.Open(); //this next query checks to see if a database exists called Project_Notes
                string sql = @"DECLARE @dbname nvarchar(150)
                SET @dbname = N'Project_Notes'

                IF NOT(EXISTS (SELECT name 
	                FROM master.dbo.sysdatabases 
	                WHERE ('[' + name + ']' = @dbname 
	                OR name = @dbname)))
	                BEGIN
	                CREATE DATABASE Project_Notes ON PRIMARY 
			                (NAME = Project_Notes, 
			                FILENAME = 'C:\\Project_Notes\\Project_Notes.mdf', 
			                SIZE = 5MB, MAXSIZE = 100MB, FILEGROWTH = 10%) 
			                LOG ON (NAME = Project_Notes_log, 
			                FILENAME = 'C:\\Project_Notes\\Project_Notes_log.ldf', 
			                SIZE = 1MB, 
			                MAXSIZE = 10MB, 
			                FILEGROWTH = 10%);
	                END";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery(); //Will create the database Project_Notes if it does not already exist.  
                }
                //This next portion creates the default tables within the database
                //Checks if logs does not exist.  If it does not, then create the tables
                sql = @"
                IF NOT(EXISTS (SELECT TABLE_NAME FROM [Project_Notes].[INFORMATION_SCHEMA].[TABLES] WHERE TABLE_NAME = 'LOGS'))
	            BEGIN
                    CREATE TABLE [Project_Notes].[dbo].[PROJECT]
                        (ID int NOT NULL IDENTITY(1,1), PROJECT_TITLE NVARCHAR(300) NOT NULL,PROJECT_NUMBER NVARCHAR(50),CREATION_DATE DATETIME CONSTRAINT DF_PROJECT DEFAULT GETDATE(),

                        PROJECT_MANAGER_DIM_FK int FOREIGN KEY REFERENCES DIMENSIONS(ID),
                        PROJECT_DETAILS_DIM_FK int FOREIGN KEY REFERENCES DIMENSIONS(ID),
                        ADD_TASK_DIM_FK int FOREIGN KEY REFERENCES DIMENSIONS(ID),
                        
                        PROJECT_DIM_FK int FOREIGN KEY REFERENCES DIMENSIONS(ID),
                        PROJECT_LOG_DIM_FK int FOREIGN KEY REFERENCES DIMENSIONS(ID),
                        PROJECT_FILE_DIM_FK int FOREIGN KEY REFERENCES DIMENSIONS(ID),
                        PROJECT_TASK_MANAGER_DIM_FK int FOREIGN KEY REFERENCES DIMENSIONS(ID),

                        CONSTRAINT pk_project PRIMARY KEY (ID),Archived bit NOT NULL DEFAULT 0); 
                    CREATE TABLE [Project_Notes].[dbo].[NOTES](ID int NOT NULL IDENTITY(1,1), NOTE_TITLE NVARCHAR(300),NOTE_CONTENT NVARCHAR(4000),CREATION_DATE DATETIME CONSTRAINT DF_NOTES DEFAULT GETDATE(),CONSTRAINT pk_note PRIMARY KEY (ID),Project_ID int Foreign Key References Project(ID),Archived bit NOT NULL DEFAULT 0);--notes reference the project
                    CREATE TABLE [Project_Notes].[dbo].[TASKS](ID int NOT NULL IDENTITY(1,1),TASK_TITLE NVARCHAR(300),TASK_DESCRIPTION NVARCHAR(4000),CREATION_DATE DATETIME CONSTRAINT DF_TASKS DEFAULT GETDATE(),CONSTRAINT PK_TASK PRIMARY KEY(ID),Project_ID int Foreign Key References Project(ID),Archived bit NOT NULL DEFAULT 0);--tasks reference the project;
                    CREATE TABLE [Project_Notes].[dbo].[LOGS](ID int NOT NULL IDENTITY(1,1),LOG_NOTE NVARCHAR(4000),CREATION_DATE DATETIME CONSTRAINT DF_LOGS DEFAULT GETDATE(), Task_Id int FOREIGN KEY REFERENCES Tasks(ID),CONSTRAINT pk_log PRIMARY KEY (ID));--Logs reference tasks;
                    CREATE TABLE SIZES(id int primary key,
	                    heightSize FLOAT not null constraint height_default default 450,
	                    widthSize FLOAT not null constraint width_default default 400,
	                    topSize FLOAT not null constraint top_default default 0,
	                    leftSize FLOAT not null constraint left_default default 0)
                    CREATE TABLE FILES(ID int NOT NULL IDENTITY(1,1),filepath nvarchar(300),CREATION_DATE DATETIME CONSTRAINT DF_FILES DEFAULT GETDATE(),CONSTRAINT pk_file PRIMARY KEY (ID),
                            Project_ID int Foreign Key References Project(ID),
                            Task_ID int Foreign Key References Tasks(ID))
                    CREATE TABLE DIMENSIONS(ID int PRIMARY KEY NOT NULL IDENTITY(1,1),HEIGHT FLOAT NOT NULL, WIDTH FLOAT NOT NULL, TOPDIM FLOAT NOT NULL,LEFTDIM FLOAT NOT NULL)
                END
                ";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery(); //Will create the database Project_Notes if it does not already exist.  
                }
                conn.Close();
            } //missing database, I create it below


            //Get the number of projects to be displayed and their Title...
            int i = 0;
            using (SqlConnection conn = new SqlConnection("Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open(); //insert log, the creation_date is added by default
                string sql = @"
                    SELECT ID,PROJECT_TITLE FROM PROJECT WHERE ARCHIVED = @archiveMode;
                ";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@archiveMode", this.ArchivedMode);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        object[] colVals = new object[2];
                        reader.GetValues(colVals);

                        RowDefinition gridRow1 = new RowDefinition();
                        Projects_Grid.RowDefinitions.Add(gridRow1);

                        System.Windows.Controls.Button projectBtn = new Button();
                        projectBtn.Content = colVals[1].ToString();
                        projectBtn.Name = "ProjectMainButton" + colVals[0].ToString();
                        this.Controls["ProjectMainButton" + colVals[0].ToString()] = projectBtn;
                        projectBtn.Tag = colVals[0].ToString();
                        projectBtn.MinHeight = 2;
                        projectBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(NavigateToProject));
                        if (this.ArchivedMode == 1)
                        {
                            projectBtn.Background = new SolidColorBrush(Colors.AliceBlue);
                        }
                        else
                        {
                            projectBtn.Background = new SolidColorBrush(Colors.Cornsilk);
                        }

                        Grid.SetRow(projectBtn, i+1);
                        Projects_Grid.Children.Add(projectBtn);
                        i++;
                    }
                }
            }
            if (!Directory.Exists("C:\\Project_Notes\\Stored_Content"))
            {
                Directory.CreateDirectory("C:\\Project_Notes\\Stored_Content");
            }

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // save the property settings
            Properties.Settings.Default.Save();
        }



        public string GetTitle()
        {
            if (this.ArchivedMode == 0)
            {
                return "Projects";
            }
            else
            {
                return "Archives";
            }
        }

        private void NavigateToProject(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;//this is the button I clicked
            Projects projectWindow = new Projects(Int32.Parse(button.Tag.ToString()),this.ArchivedMode);//PASS AROUND THE PROJECT ID
            App.Current.MainWindow = projectWindow;
            this.Close();
            projectWindow.Show();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SaveProject projectWindow = new SaveProject(this.ArchivedMode);
            App.Current.MainWindow = projectWindow;
            this.Close();
            projectWindow.Show();
        }

        private void NavigateToArchivedProjects(object sender, RoutedEventArgs e)
        {
            if (this.ArchivedMode == 0)
            {
                MainWindow mainWindow = new MainWindow(1); //ask for archived
                App.Current.MainWindow = mainWindow;
                this.Close();
                mainWindow.Show();
            }
            else
            {
                MainWindow mainWindow = new MainWindow(0); //ask for archived
                App.Current.MainWindow = mainWindow;
                this.Close();
                mainWindow.Show();
            }

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchTextBox.Text != "Project Search..." && SearchTextBox.Text != "")
            {
                //I still need to add this search functionality
                using (
                    SqlConnection conn =
                        new SqlConnection(
                            "Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
                {
                    conn.Open(); //insert log, the creation_date is added by default
                    string sql = String.Format(@"
                    SELECT ID,PROJECT_TITLE FROM [Project_Notes].[dbo].[PROJECT] WHERE ARCHIVED = @archiveMode AND PROJECT_TITLE NOT LIKE '%{0}%';
                    ",SearchTextBox.Text);
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.Add("@archiveMode", this.ArchivedMode);
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            object[] colVals = new object[2];
                            reader.GetValues(colVals);

                            string projId = colVals[0].ToString();
                            string projTitle = colVals[1].ToString();

                            var button2Hide = this.Controls["ProjectMainButton" + projId];
                            button2Hide.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
            if (SearchTextBox.Text == "")
            {
                foreach (var key in this.Controls.Keys)
                {
                    var button2Hide = this.Controls[key];
                    button2Hide.Visibility = Visibility.Visible;
                }
            }
        }

        private void UcTextBox_PreviewMouseDown(Object sender, RoutedEventArgs args)
        {
            if (SearchTextBox.Text == "Project Search...")
            {
                SearchTextBox.Text = "";
            }
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
            //MessageBox.Show(this.Height.ToString() + " " + this.Width.ToString() + " " + this.Top.ToString() + " " +  this.Left.ToString());
        }
    }
}
