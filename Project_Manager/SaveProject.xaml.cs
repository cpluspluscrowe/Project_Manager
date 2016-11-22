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
    /// Interaction logic for SaveProject.xaml
    /// </summary>
    public partial class SaveProject : Window
    {
        public int ArchiveMode;
        public SaveProject(int archiveMode)
        {
            ArchiveMode = archiveMode;
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            if (this.ArchiveMode == 1)
            {
                IEnumerable<Button> collection = MainSaveProjectGrid.Children.OfType<Button>();
                foreach (var button in collection)
                {
                    button.Background = new SolidColorBrush(Colors.GhostWhite);
                }
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow(this.ArchiveMode);
            App.Current.MainWindow = mainWindow;
            this.Close();
            mainWindow.Show();
        }



        private void PTitleMd(object sender, RoutedEventArgs e)
        {
            RichTextBox rtb = (RichTextBox)sender;//this is the button I clicked
            TextRange taskTitle = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            if (taskTitle.Text.Contains("Project Title"))
            {
                rtb.Document.Blocks.Clear();
                rtb.Focus();
            }
        }


        private void PNumberMd(object sender, RoutedEventArgs e)
        {
            RichTextBox rtb = (RichTextBox)sender;//this is the button I clicked
            TextRange taskTitle = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            if (taskTitle.Text.Contains("Project Number"))
            {
                rtb.Document.Blocks.Clear();
                rtb.Focus();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            TextRange projectTitle = new TextRange(ProjectTitleSave.Document.ContentStart, ProjectTitleSave.Document.ContentEnd);
            TextRange projectContent = new TextRange(ProjectNumberSave.Document.ContentStart, ProjectNumberSave.Document.ContentEnd);

            if (IsNumber(projectContent.Text.Replace("\r\n","")))
            {
                using (
                    SqlConnection conn =
                        new SqlConnection(
                            "Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
                {
                    conn.Open(); //insert log, the creation_date is added by default
                    string sql = @"
                if NOT '{0}' = '" + "" + "\r\n" + "" + @"'
                INSERT INTO PROJECT(PROJECT_TITLE, PROJECT_NUMBER, Archived) VALUES(@projectTitleText,@projectContentText,@archiveMode);
                ";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.Add("@projectTitleText", projectTitle.Text);
                        cmd.Parameters.Add("@projectContentText", projectContent.Text);
                        cmd.Parameters.Add("@archiveMode", this.ArchiveMode);
                        try
                        {
                            cmd.ExecuteNonQuery();
                            //Will create the database Project_Notes if it does not already exist.
                            this.Button_Click(sender,e);//return to main screen
                        }
                        catch (Exception excp)
                        {
                            MessageBox.Show("Project Name cannot be empty");
                        }

                    }

                    sql = @"
                    DECLARE @projectId int;
                    SET @projectId = (SELECT TOP 1 id FROM PROJECT ORDER BY CREATION_DATE DESC);
                    INSERT INTO DIMENSIONS(HEIGHT,WIDTH,TOPDIM,LEFTDIM) VALUES(370,625,250,360);
					DECLARE @lastid int;
                    SET @lastid = (SELECT SCOPE_IDENTITY());
                    UPDATE PROJECT SET PROJECT_MANAGER_DIM_FK = @lastid WHERE ID = @projectId;

                    INSERT INTO DIMENSIONS(HEIGHT,WIDTH,TOPDIM,LEFTDIM) VALUES(370,625,250,360);
                    SET @lastid = (SELECT SCOPE_IDENTITY());
                    UPDATE PROJECT SET PROJECT_DETAILS_DIM_FK = @lastid WHERE ID = @projectId;

                    INSERT INTO DIMENSIONS(HEIGHT,WIDTH,TOPDIM,LEFTDIM) VALUES(370,625,250,360);
                    SET @lastid = (SELECT SCOPE_IDENTITY());
                    UPDATE PROJECT SET ADD_TASK_DIM_FK = @lastid WHERE ID = @projectId;

                    INSERT INTO DIMENSIONS(HEIGHT,WIDTH,TOPDIM,LEFTDIM) VALUES(370,625,250,360);
                    SET @lastid = (SELECT SCOPE_IDENTITY());
                    UPDATE PROJECT SET PROJECT_DIM_FK = @lastid WHERE ID = @projectId;

                    INSERT INTO DIMENSIONS(HEIGHT,WIDTH,TOPDIM,LEFTDIM) VALUES(370,625,250,360);
                    SET @lastid = (SELECT SCOPE_IDENTITY());
                    UPDATE PROJECT SET PROJECT_LOG_DIM_FK = @lastid WHERE ID = @projectId;

                    INSERT INTO DIMENSIONS(HEIGHT,WIDTH,TOPDIM,LEFTDIM) VALUES(370,625,250,360);
                    SET @lastid = (SELECT SCOPE_IDENTITY());
                    UPDATE PROJECT SET PROJECT_FILE_DIM_FK = @lastid WHERE ID = @projectId;

                    INSERT INTO DIMENSIONS(HEIGHT,WIDTH,TOPDIM,LEFTDIM) VALUES(370,625,250,360);
                    SET @lastid = (SELECT SCOPE_IDENTITY());
                    UPDATE PROJECT SET PROJECT_TASK_MANAGER_DIM_FK = @lastid WHERE ID = @projectId;
                ";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                            cmd.ExecuteNonQuery();
                            //Will create the database Project_Notes if it does not already exist.
                            this.Button_Click(sender, e);//return to main screen
                    }



                }
            }
            else
            {
                MessageBox.Show("Project not corrected.  Project number must be a number or left empty");
            }
        }
        public Boolean IsNumber(String value)
        {
            return value.All(Char.IsDigit);
        }

        private void SaveProject_OnClosed(object sender, EventArgs e)
        {
            
        }
    }
}
