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
    /// Interaction logic for EditTask.xaml
    /// </summary>
    public partial class EditTask : Window
    {
        public int ArchiveMode;
        public int TaskId;
        public int ProjectId;
        public EditTask(int projectId, int archiveMode, int taskId)
        {
            this.ArchiveMode = archiveMode;
            this.ProjectId = projectId;
            this.TaskId = taskId;
            InitializeComponent();

            using (SqlConnection conn = new SqlConnection("Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open();
                string sql = @"
                SELECT TASK_TITLE,TASK_DESCRIPTION FROM TASKS WHERE ID = @taskId;
                ";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@taskId", this.TaskId);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        object[] colVals = new object[2];
                        reader.GetValues(colVals);
                        string projTitle = reader.GetString(0);
                        string projNumber = reader.GetString(1);
                        var rtbTitle = (RichTextBox)this.FindName("TaskEditTitle");
                        var rtbNumber = (RichTextBox)this.FindName("TaskEditDescription");
                        if (projTitle != "")
                        {
                            rtbTitle.Document.Blocks.Clear();
                            rtbTitle.AppendText(projTitle);
                        }
                        if (projNumber != "")
                        {
                            rtbNumber.Document.Blocks.Clear();
                            rtbNumber.AppendText(projNumber);
                        }
                    }
                }
            }
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Task_Manager mainWindow = new Task_Manager(this.ArchiveMode,this.ProjectId);
            App.Current.MainWindow = mainWindow;
            this.Close();
            mainWindow.Show();
        }



        private void PTitleMd(object sender, RoutedEventArgs e)
        {
            RichTextBox rtb = (RichTextBox)sender;//this is the button I clicked
            TextRange taskTitle = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            if (taskTitle.Text.Contains("Task Title"))
            {
                rtb.Document.Blocks.Clear();
                rtb.Focus();
            }
        }


        private void PNumberMd(object sender, RoutedEventArgs e)
        {
            RichTextBox rtb = (RichTextBox)sender;//this is the button I clicked
            TextRange taskTitle = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            if (taskTitle.Text.Contains("Task Description"))
            {
                rtb.Document.Blocks.Clear();
                rtb.Focus();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {

            TextRange projectTitle = new TextRange(TaskEditTitle.Document.ContentStart, TaskEditTitle.Document.ContentEnd);
            TextRange projectContent = new TextRange(TaskEditDescription.Document.ContentStart, TaskEditDescription.Document.ContentEnd);

            using (SqlConnection conn = new SqlConnection("Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open();
                string sql = @"
                UPDATE TASKS SET TASK_TITLE = @taskTitle,TASK_DESCRIPTION = @taskDescription, CREATION_DATE = GetDate() WHERE ID = @taskId;
                ";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@taskTitle", projectTitle.Text);
                    cmd.Parameters.Add("@taskDescription", projectContent.Text);
                    cmd.Parameters.Add("@taskId", this.TaskId);
                    cmd.ExecuteNonQuery();
                }
            }


        }
        private void BackButton(object sender, RoutedEventArgs e)
        {
            Task_Manager projectWindow = new Task_Manager(this.ProjectId, this.ArchiveMode);//PASS AROUND THE PROJECT ID
            App.Current.MainWindow = projectWindow;
            this.Close();
            projectWindow.Show();
        }







    }
}
