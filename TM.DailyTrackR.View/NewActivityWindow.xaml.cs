using System;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace TM.DailyTrackR.View
{
    public partial class NewActivityWindow : Window
    {
        private string currentUser;

        public NewActivityWindow(string user)
        {
            InitializeComponent();
            currentUser = user;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string projectType = ((ComboBoxItem)projectTypeComboBox.SelectedItem)?.Content.ToString();
            string taskType = ((ComboBoxItem)taskTypeComboBox.SelectedItem)?.Content.ToString();
            string description = descriptionTextBox.Text;
            string status = ((ComboBoxItem)statusComboBox.SelectedItem)?.Content.ToString();
            DateTime? date = datePicker.SelectedDate;

            if (projectType != null && taskType != null && status != null && date.HasValue)
            {
                string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        string query = "INSERT INTO Activities (ProjectType, TaskType, Description, Status, ActivityDate, User) VALUES (@ProjectType, @TaskType, @Description, @Status, @ActivityDate, @User)";
                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@ProjectType", projectType);
                        cmd.Parameters.AddWithValue("@TaskType", taskType);
                        cmd.Parameters.AddWithValue("@Description", description);
                        cmd.Parameters.AddWithValue("@Status", status);
                        cmd.Parameters.AddWithValue("@ActivityDate", date.Value.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@User", currentUser);

                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred while saving the activity: " + ex.Message);
                    }
                }

                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Please fill in all fields.");
            }
        }
    }
}
