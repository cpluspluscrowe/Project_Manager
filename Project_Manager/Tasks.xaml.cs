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
    /// Interaction logic for Tasks.xaml
    /// </summary>
    public partial class Tasks : Window
    {
        public int ArchivedMode;
        public int ProjectID;
        public Tasks(int projectID, int archivedMode)
        {
            this.ArchivedMode = archivedMode;
            this.ProjectID = projectID;
            InitializeComponent();


            using (SqlConnection conn = new SqlConnection("Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open(); //insert log, the creation_date is added by default
                string sql = @"
                SELECT HEIGHT,WIDTH,TOPDIM,LEFTDIM FROM [Project_Notes].[dbo].[DIMENSIONS] WHERE ID =
                (SELECT ADD_TASK_DIM_FK from [Project_Notes].[dbo].[PROJECT] WHERE ID = @projectId);
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


            var taskTb = (TextBlock)this.FindName("ExistingTasks");
            taskTb.Text = "";

            using (
            SqlConnection conn =
                new SqlConnection(
                    "Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
                    {
                        conn.Open(); //insert log, the creation_date is added by default
                        string sql = @"
                            SELECT TASK_TITLE FROM [Project_Notes].[dbo].[TASKS] WHERE PROJECT_ID = @projectId ORDER BY CREATION_DATE DESC
                        ";
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.Add("@projectId", this.ProjectID);
                            SqlDataReader reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                string val = reader.GetString(0);
                                taskTb.Text += "> " + val;
                            }
                        }
                    }




            if (this.ArchivedMode == 1)
            {
                IEnumerable<Button> collection = MainGridTask.Children.OfType<Button>();
                foreach (var button in collection)
                {
                    button.Background = new SolidColorBrush(Colors.GhostWhite);
                }
            }
        }

        private void Button_Click_Back(object sender, RoutedEventArgs e)
        {
            Task_Manager taskManagementWindow = new Task_Manager(this.ProjectID, this.ArchivedMode);
            App.Current.MainWindow = taskManagementWindow;
            this.Close();
            taskManagementWindow.Show();
        }

        private void taskTitleMD(object sender, RoutedEventArgs e)
        {
            RichTextBox rtb = (RichTextBox)sender;//this is the button I clicked
            TextRange taskTitle = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            if (taskTitle.Text.Contains("Task Title"))
            {
                rtb.Document.Blocks.Clear();
                rtb.Focus();
            }
        }

        private void taskDescrMD(object sender, RoutedEventArgs e)
        {
            RichTextBox rtb = (RichTextBox)sender;//this is the button I clicked
            TextRange taskTitle = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            if (taskTitle.Text.Contains("Task Description"))
            {
                rtb.Document.Blocks.Clear();
                rtb.Focus();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TextRange taskTitle = new TextRange(TaskTitle.Document.ContentStart, TaskTitle.Document.ContentEnd);
            TextRange taskContent = new TextRange(TaskDescription.Document.ContentStart, TaskDescription.Document.ContentEnd);

            if (taskTitle.Text != "")
            {
                using (
                    SqlConnection conn =
                        new SqlConnection(
                            "Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
                {
                    conn.Open(); //insert log, the creation_date is added by default
                    string sql = String.Format(@"
                INSERT INTO TASKS(TASK_TITLE, TASK_DESCRIPTION, PROJECT_ID) VALUES(@taskTitleText,@taskContentText,@projectId);
                ");
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.Add("@taskTitleText", taskTitle.Text);
                        cmd.Parameters.Add("@taskContentText", taskContent.Text);
                        cmd.Parameters.Add("@projectId", this.ProjectID);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                MessageBox.Show("Task Title cannot be empty");
            }
            TaskTitle.Document.Blocks.Clear();
            TaskDescription.Document.Blocks.Clear();
        }

        private void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Tasks_OnClosed(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection("Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open(); //insert log, the creation_date is added by default
                string sql = @"
                UPDATE [Project_Notes].[dbo].[DIMENSIONS] SET HEIGHT = @height,WIDTH = @width,TOPDIM = @top,LEFTDIM = @left WHERE ID = 
                (SELECT ADD_TASK_DIM_FK from [Project_Notes].[dbo].[PROJECT] WHERE ID = @projectId)
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
