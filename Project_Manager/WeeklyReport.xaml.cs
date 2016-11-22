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
    /// Interaction logic for WeeklyReport.xaml
    /// </summary>
    public partial class WeeklyReport : Window
    {
        public int ArchivedMode;
        public int ProjectID;
        public WeeklyReport(int projectID, int archivedMode)
        {
            this.ArchivedMode = archivedMode;
            this.ProjectID = projectID;
            InitializeComponent();
            if (this.ArchivedMode == 1)
            {
                IEnumerable<Button> collection = MainWRGrid.Children.OfType<Button>();
                foreach (var button in collection)
                {
                    button.Background = new SolidColorBrush(Colors.GhostWhite);
                }
            }
            //Now gather the project data into the weekly report

            string weeklyReportText = "";
            string currentType = "";
            using (
                SqlConnection conn =
                    new SqlConnection(
                        "Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open(); //insert log, the creation_date is added by default
                string sql = String.Format(@"
                DECLARE @project_title NVARCHAR(50);
                DECLARE @PROJECT_NUMBER NVARCHAR(50);
                DECLARE @NOTE_TITLE NVARCHAR(50);
                DECLARE @NOTE_CONTENT NVARCHAR(400);
                DECLARE @TASK_TITLE NVARCHAR(50);
                DECLARE @TASK_DESCRIPTION NVARCHAR(400);
                DECLARE @CREATION_DATE DATE;
                DECLARE @LOG_NOTE NVARCHAR(400);
                DECLARE @project_id int;
                DECLARE @task_id int;

                DECLARE @sqlCommand varchar(1000)
                DECLARE @FetchStatus int
                DECLARE project_cursor CURSOR  
	                FOR SELECT ID,PROJECT_TITLE,PROJECT_NUMBER FROM PROJECT WHERE ID = @projectId;

                OPEN project_cursor 
                FETCH NEXT FROM project_cursor INTO @project_id, @project_title, @project_number
                WHILE @@FETCH_STATUS = 0
	                BEGIN
		                SELECT 'PROJECT',@PROJECT_TITLE AS 'PROJECT_TITLE',@PROJECT_NUMBER AS 'PROJECT_NUMBER'
		                SELECT 'NOTE',NOTE_TITLE,NOTE_CONTENT FROM NOTES WHERE PROJECT_ID = @PROJECT_ID


		                DECLARE TASK_cursor CURSOR  
		                FOR SELECT ID,TASK_TITLE,TASK_DESCRIPTION,CREATION_DATE FROM TASKS WHERE PROJECT_ID = @PROJECT_ID;
		                OPEN TASK_CURSOR
		                FETCH NEXT FROM TASK_cursor INTO @TASK_ID,@TASK_TITLE,@TASK_DESCRIPTION,@CREATION_DATE
		                WHILE @@FETCH_STATUS = 0
			                BEGIN
				                SELECT 'TASK',@TASK_TITLE AS 'TASK_TITLE',@TASK_DESCRIPTION AS 'TASK_DESCRIPTION' WHERE @CREATION_DATE > (SELECT DATEADD(wk, DATEDIFF(wk, 6, GETDATE()), 6));
				                SELECT 'LOG',LOG_NOTE FROM [Project_Notes].[dbo].[LOGS] WHERE Task_Id = @TASK_ID AND
                                     CREATION_DATE > (SELECT DATEADD(wk, DATEDIFF(wk, 6, GETDATE()), 6)) ORDER BY CREATION_DATE DESC;
				                FETCH NEXT FROM TASK_cursor INTO  @TASK_ID,@TASK_TITLE,@TASK_DESCRIPTION,@CREATION_DATE
			                END
		                CLOSE TASK_CURSOR
		                DEALLOCATE TASK_CURSOR

		                FETCH NEXT FROM project_cursor INTO  @project_id, @project_title, @project_number
	                END
                CLOSE project_cursor;
                DEALLOCATE project_cursor;
                ");
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@projectId",this.ProjectID);
                    SqlDataReader reader = cmd.ExecuteReader();
                    do
                    {


                        while (reader.Read())
                        {
                            object[] colVals = new object[4];
                            reader.GetValues(colVals);
                            for (int i = 0; i < 4; i++)
                            {
                                if (colVals[i] != null)
                                {
                                    colVals[i] = colVals[i].ToString().Replace("\r\n", "");
                                    if (i == 0)
                                    {
                                        if (colVals[i].ToString() == "PROJECT")
                                        {
                                            weeklyReportText += "\nProject: ";
                                            currentType = "PROJECT";
                                        }
                                        else if (colVals[i].ToString() == "NOTE")
                                        {
                                            weeklyReportText += "Note: ";
                                            currentType = "NOTE";
                                        }
                                        else if (colVals[i].ToString() == "TASK")
                                        {
                                            weeklyReportText += "\n\tTask: ";
                                            currentType = "TASK";
                                        }
                                        else if (colVals[i].ToString() == "LOG")
                                        {
                                            weeklyReportText += "\n\t\t\tLog: ";
                                            currentType = "LOG";
                                        }
                                    }
                                    else
                                    {
                                        if (i == 2)
                                        {
                                            weeklyReportText += "\n\t";
                                            if (currentType == "NOTE" || currentType == "TASK")
                                            {
                                                weeklyReportText += "\t";
                                            }
                                        }
                                        if (i == 1)
                                        {
                                            weeklyReportText += "";
                                        }
                                        if (currentType == "PROJECT")
                                        {

                                        }
                                        else if (currentType == "NOTE")
                                        {
                                            if (i == 1)
                                            {
                                                weeklyReportText += "Title: ";
                                            }
                                            else if (i == 2)
                                            {
                                                weeklyReportText += "Description: ";
                                            }
                                        }
                                        else if (currentType == "TASK")
                                        {
                                            if (i == 2)
                                            {
                                                weeklyReportText += "Description: ";
                                            }
                                        }
                                        else if(currentType == "LOG")
                                        {
                                            
                                        }
                                        if (!(currentType == "PROJECT" && i == 2))
                                        {
                                            weeklyReportText += colVals[i].ToString().Replace("\r\n\r\n", "");
                                        }
                                        else
                                        {
                                            int p = 5;
                                        }
                                        
                                    }
                                    
                                }
                            }
                        }
                    }while (reader.NextResult());


                }
            }

            WeeklyReportTextbox.AppendText(weeklyReportText);


            this.Width = SystemParameters.WorkArea.Width;
            this.Height = SystemParameters.WorkArea.Height;


        }

        private void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Projects projectWindow = new Projects(this.ProjectID,this.ArchivedMode);
            App.Current.MainWindow = projectWindow;
            this.Close();
            projectWindow.Show();
        }

        private void Calendar_OnSelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            WeeklyReportTextbox.Document.Blocks.Clear();
            var calendar = sender as Calendar;
            DateTime startDate = calendar.SelectedDate.Value;
            int sn = calendar.SelectedDates.Count;
            DateTime endDate = startDate.AddDays(sn-1);
            string weeklyReportText = "";
            string currentType = "";
            using (
                SqlConnection conn =
                    new SqlConnection(
                        "Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open(); //insert log, the creation_date is added by default
                string sql = String.Format(@"
                DECLARE @project_title NVARCHAR(50);
                DECLARE @PROJECT_NUMBER NVARCHAR(50);
                DECLARE @NOTE_TITLE NVARCHAR(50);
                DECLARE @NOTE_CONTENT NVARCHAR(400);
                DECLARE @TASK_TITLE NVARCHAR(50);
                DECLARE @TASK_DESCRIPTION NVARCHAR(400);
                DECLARE @CREATION_DATE DATE;
                DECLARE @LOG_NOTE NVARCHAR(400);
                DECLARE @project_id int;
                DECLARE @task_id int;

                DECLARE @sqlCommand varchar(1000)
                DECLARE @FetchStatus int
                DECLARE project_cursor CURSOR  
	                FOR SELECT ID,PROJECT_TITLE,PROJECT_NUMBER FROM PROJECT WHERE ID = @projectId;

                OPEN project_cursor 
                FETCH NEXT FROM project_cursor INTO @project_id, @project_title, @project_number
                WHILE @@FETCH_STATUS = 0
	                BEGIN
		                SELECT 'PROJECT',@PROJECT_TITLE AS 'PROJECT_TITLE',@PROJECT_NUMBER AS 'PROJECT_NUMBER'
		                SELECT 'NOTE',NOTE_TITLE,NOTE_CONTENT FROM NOTES WHERE PROJECT_ID = @PROJECT_ID


		                DECLARE TASK_cursor CURSOR  
		                FOR SELECT ID,TASK_TITLE,TASK_DESCRIPTION,CREATION_DATE FROM TASKS WHERE PROJECT_ID = @PROJECT_ID;
		                OPEN TASK_CURSOR
		                FETCH NEXT FROM TASK_cursor INTO @TASK_ID,@TASK_TITLE,@TASK_DESCRIPTION,@CREATION_DATE
		                WHILE @@FETCH_STATUS = 0
			                BEGIN
				                SELECT 'TASK',@TASK_TITLE AS 'TASK_TITLE',@TASK_DESCRIPTION AS 'TASK_DESCRIPTION' WHERE @CREATION_DATE > (SELECT DATEADD(wk, DATEDIFF(wk, 6, GETDATE()), 6));
				                SELECT 'LOG',LOG_NOTE FROM [Project_Notes].[dbo].[LOGS] WHERE Task_Id = @TASK_ID AND
                                     CREATION_DATE > @startDate and CREATION_DATE < @endDate ORDER BY CREATION_DATE DESC;
				                FETCH NEXT FROM TASK_cursor INTO  @TASK_ID,@TASK_TITLE,@TASK_DESCRIPTION,@CREATION_DATE
			                END
		                CLOSE TASK_CURSOR
		                DEALLOCATE TASK_CURSOR

		                FETCH NEXT FROM project_cursor INTO  @project_id, @project_title, @project_number
	                END
                CLOSE project_cursor;
                DEALLOCATE project_cursor;
                ");
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@projectId", this.ProjectID);
                    cmd.Parameters.Add("@startDate", startDate.ToString("yyyy-MM-dd hh:mm:ss"));
                    cmd.Parameters.Add("@endDate", endDate.ToString("yyyy-MM-dd hh:mm:ss"));
                    SqlDataReader reader = cmd.ExecuteReader();
                    do
                    {


                        while (reader.Read())
                        {
                            object[] colVals = new object[4];
                            reader.GetValues(colVals);
                            for (int i = 0; i < 4; i++)
                            {
                                if (colVals[i] != null)
                                {
                                    colVals[i] = colVals[i].ToString().Replace("\r\n", "");
                                    if (i == 0)
                                    {
                                        if (colVals[i].ToString() == "PROJECT")
                                        {
                                            weeklyReportText += "\nProject: ";
                                            currentType = "PROJECT";
                                        }
                                        else if (colVals[i].ToString() == "NOTE")
                                        {
                                            weeklyReportText += "Note: ";
                                            currentType = "NOTE";
                                        }
                                        else if (colVals[i].ToString() == "TASK")
                                        {
                                            weeklyReportText += "\n\tTask: ";
                                            currentType = "TASK";
                                        }
                                        else if (colVals[i].ToString() == "LOG")
                                        {
                                            weeklyReportText += "\n\t\t\tLog: ";
                                            currentType = "LOG";
                                        }
                                    }
                                    else
                                    {
                                        if (i == 2)
                                        {
                                            weeklyReportText += "\n\t";
                                            if (currentType == "NOTE" || currentType == "TASK")
                                            {
                                                weeklyReportText += "\t";
                                            }
                                        }
                                        if (i == 1)
                                        {
                                            weeklyReportText += "";
                                        }
                                        if (currentType == "PROJECT")
                                        {

                                        }
                                        else if (currentType == "NOTE")
                                        {
                                            if (i == 1)
                                            {
                                                weeklyReportText += "Title: ";
                                            }
                                            else if (i == 2)
                                            {
                                                weeklyReportText += "Description: ";
                                            }
                                        }
                                        else if (currentType == "TASK")
                                        {
                                            if (i == 2)
                                            {
                                                weeklyReportText += "Description: ";
                                            }
                                        }
                                        else if (currentType == "LOG")
                                        {

                                        }
                                        if (!(currentType == "PROJECT" && i == 2))
                                        {
                                            weeklyReportText += colVals[i].ToString().Replace("\r\n\r\n", "");
                                        }
                                        else
                                        {
                                            int p = 5;
                                        }

                                    }

                                }
                            }
                        }
                    } while (reader.NextResult());


                }
            }
            WeeklyReportTextbox.AppendText(weeklyReportText);
        }
    }
}
