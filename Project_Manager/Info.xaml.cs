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
    /// Interaction logic for Info.xaml
    /// </summary>
    public partial class Info : Window
    {
        public int ArchivedMode;
        public int ProjectID;
        public Info(int projectId, int archivedMode)
        {
            this.ProjectID = projectId;
            this.ArchivedMode = archivedMode;

            InitializeComponent();

            using (SqlConnection conn = new SqlConnection("Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open(); //insert log, the creation_date is added by default
                string sql = @"
                SELECT HEIGHT,WIDTH,TOPDIM,LEFTDIM FROM [Project_Notes].[dbo].[DIMENSIONS] WHERE ID =
                (SELECT PROJECT_DETAILS_DIM_FK from [Project_Notes].[dbo].[PROJECT] WHERE ID = @projectId);
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


            using (SqlConnection conn = new SqlConnection("Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open();
                string sql = @"
                SELECT PROJECT_TITLE,PROJECT_NUMBER FROM PROJECT WHERE ID = @projectId;
                ";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@projectId", this.ProjectID);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        object[] colVals = new object[2];
                        reader.GetValues(colVals);
                        string projTitle = reader.GetString(0);
                        string projNumber = reader.GetString(1);
                        var rtbTitle = (RichTextBox)this.FindName("ProjectEditTitle");
                        var rtbNumber = (RichTextBox)this.FindName("ProjectEditNumber");
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
            






            if (ArchivedMode == 1)
            {
                IEnumerable<Button> collection = InfoMainGrid.Children.OfType<Button>();
                foreach (var button in collection)
                {
                    button.Background = new SolidColorBrush(Colors.GhostWhite);
                }
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Projects projectWindow = new Projects(this.ProjectID, this.ArchivedMode);
            App.Current.MainWindow = projectWindow;
            this.Close();
            projectWindow.Show();
        }

        private void ProjectTitleMd(object sender, RoutedEventArgs e)
        {
            RichTextBox rtb = (RichTextBox)sender;//this is the button I clicked
            TextRange taskTitle = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            if (taskTitle.Text.Contains("Project Title..."))
            {
                rtb.Document.Blocks.Clear();
                rtb.Focus();
            }
        }
        private void ProjectNumberMd(object sender, RoutedEventArgs e)
        {
            RichTextBox rtb = (RichTextBox)sender;//this is the button I clicked
            TextRange taskTitle = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            if (taskTitle.Text.Contains("Project Number..."))
            {
                rtb.Document.Blocks.Clear();
                rtb.Focus();
            }
        }


        private void SaveProjectDetails(object sender, RoutedEventArgs e)
        {
            var rtbTitle = (RichTextBox)this.FindName("ProjectEditTitle");
            var rtbNumber = (RichTextBox)this.FindName("ProjectEditNumber");

            TextRange projectTitle = new TextRange(rtbTitle.Document.ContentStart, rtbTitle.Document.ContentEnd);
            TextRange projectNumber = new TextRange(rtbNumber.Document.ContentStart, rtbNumber.Document.ContentEnd);


            using (SqlConnection conn = new SqlConnection("Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open(); 
                string sql = @"
                UPDATE PROJECT SET PROJECT_TITLE = @projectTitleText,PROJECT_NUMBER = @projectNumberText WHERE ID = @projectId;
                ";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@projectTitleText", projectTitle.Text);
                    cmd.Parameters.Add("@projectNumberText",projectNumber.Text);
                    cmd.Parameters.Add("@projectId",this.ProjectID);
                    cmd.ExecuteNonQuery();
                }
            }

        }

        private void Info_OnClosed(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection("Server=(LocalDB)\\MSSQLLocalDB;Database=Project_Notes;Integrated Security = true"))
            {
                conn.Open(); //insert log, the creation_date is added by default
                string sql = @"
                UPDATE [Project_Notes].[dbo].[DIMENSIONS] SET HEIGHT = @height,WIDTH = @width,TOPDIM = @top,LEFTDIM = @left WHERE ID = 
                (SELECT PROJECT_DETAILS_DIM_FK from [Project_Notes].[dbo].[PROJECT] WHERE ID = @projectId)
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
