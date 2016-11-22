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
using Project_Manager;

namespace Project_Manager
{
    /// <summary>
    /// Interaction logic for Logs.xaml
    /// </summary>
    public partial class Logs : Window
    {
        public int SelectedIndex;
        public int ArchivedMode;
        public Dictionary<string, int> Controls;
        public int ProjectID;
        public Logs(int projectID, int archivedMode, int selectedIndex)
        {
            this.SelectedIndex = selectedIndex;
            this.ArchivedMode = archivedMode;
            this.ProjectID = projectID;
            InitializeComponent();

            using (SqlConnection conn = new SqlConnection("Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open(); //insert log, the creation_date is added by default
                string sql = @"
                SELECT HEIGHT,WIDTH,TOPDIM,LEFTDIM FROM [Project_Notes].[dbo].[DIMENSIONS] WHERE ID =
                (SELECT PROJECT_LOG_DIM_FK from [Project_Notes].[dbo].[PROJECT] WHERE ID = @projectId);
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




            Controls = new Dictionary<string, int>();


            using (
                SqlConnection conn =
                    new SqlConnection(
                        "Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open(); //insert log, the creation_date is added by default
                string sql = @"
                    SELECT TASK_TITLE,ID FROM [Project_Notes].[dbo].[TASKS] WHERE PROJECT_ID = @projectId ORDER BY TASK_TITLE ASC
                ";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@projectId", this.ProjectID);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        string val = reader.GetString(0);
                        LogComboBox.Items.Add(val);
                        Controls[val] = reader.GetInt32(1);
                    }
                }
            }
            string lastLog = null;
            using (
                SqlConnection conn =
                    new SqlConnection(
                        "Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open(); //insert log, the creation_date is added by default
                string sql = @"
                DECLARE @lastUpdatedTask int;
                SET @lastUpdatedTask = (
                SELECT TOP 1 Task_Id FROM [Project_Notes].[dbo].[LOGS] ORDER BY CREATION_DATE DESC)
                SELECT TASK_TITLE FROM [Project_Notes].[dbo].[TASKS] WHERE ID = @lastUpdatedTask;
                ";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        string val = reader.GetString(0);
                        lastLog = val.ToString();
                    }
                }
            }
            bool foundItem = false;
            int indexcnt = 0;
            foreach (var item in LogComboBox.Items)
            {
                string ex = item.ToString();
                if (item.ToString() == lastLog)
                {
                    LogComboBox.SelectedIndex = indexcnt;
                    foundItem = true;
                }
                indexcnt += 1;
            }
            if (foundItem == false)
            {
                LogComboBox.SelectedIndex = this.SelectedIndex;
            }

            
        }

        private void ReWriteLogs(object sender)
        {
            Past_Logs.Text = "";
            using (
                SqlConnection conn =
                    new SqlConnection(
                        "Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open(); //insert log, the creation_date is added by default
                string sql = @"
                    SELECT LOG_NOTE FROM [Project_Notes].[dbo].[LOGS] WHERE TASK_ID = (SELECT ID FROM TASKS WHERE TASK_TITLE = @selectedItem) ORDER BY CREATION_DATE DESC
                "; 
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@selectedItem", (sender as ComboBox).SelectedItem as string);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        string val = reader.GetString(0);
                        Past_Logs.Text += "> " + reader.GetString(0);
                    }
                }
            }
        }


        private void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private void logMd(object sender, RoutedEventArgs e)
        {
            RichTextBox rtb = (RichTextBox)sender;//this is the button I clicked
            TextRange taskTitle = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            if (taskTitle.Text.Contains("Log"))
            {
                rtb.Document.Blocks.Clear();
                rtb.Focus();
            }
        }
        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TextRange textRange = new TextRange(RichtbLog.Document.ContentStart, RichtbLog.Document.ContentEnd);
            if (textRange.Text != "")
            {
                if (LogComboBox.Text != "")
                {
                    using (
                        SqlConnection conn =
                            new SqlConnection(
                                "Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
                    {
                        conn.Open(); //insert log, the creation_date is added by default
                        string sql = @"
                    INSERT INTO [Project_Notes].[dbo].[LOGS](LOG_NOTE,TASK_ID) VALUES (@textRangeText,@ControlsLogComboBoxText);
                        ";
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.Add("@textRangeText",textRange.Text);
                            cmd.Parameters.Add("@ControlsLogComboBoxText",Controls[LogComboBox.Text]);
                            cmd.ExecuteNonQuery();
                            //Will create the database Project_Notes if it does not already exist.  
                        }
                    }
                    RichtbLog.Document.Blocks.Clear();
                    Logs logsWindow = new Logs(this.ProjectID, this.ArchivedMode, LogComboBox.SelectedIndex);
                    App.Current.MainWindow = logsWindow;
                    this.Close();
                    logsWindow.Show();
                }
                else
                {
                    MessageBox.Show("You need to first create a Task!  Click Back and then click 'Add Task'");
                }
            }
        }
        private void Button_Click_Back(object sender, RoutedEventArgs e)
        {
            Projects projectWindow = new Projects(this.ProjectID,this.ArchivedMode);
            App.Current.MainWindow = projectWindow;
            this.Close();
            projectWindow.Show();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ReWriteLogs(sender);
        }

        private void Logs_OnClosed(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection("Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open(); //insert log, the creation_date is added by default
                string sql = @"
                UPDATE [Project_Notes].[dbo].[DIMENSIONS] SET HEIGHT = @height,WIDTH = @width,TOPDIM = @top,LEFTDIM = @left WHERE ID = 
                (SELECT PROJECT_LOG_DIM_FK from [Project_Notes].[dbo].[PROJECT] WHERE ID = @projectId)
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
