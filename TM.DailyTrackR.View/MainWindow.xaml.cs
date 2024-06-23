using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using TM.DailyTrackR.DataType;

namespace TM.DailyTrackR.View
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<Activity> Activities { get; set; }
        private string currentUser;

        public MainWindow()
        {
            InitializeComponent();
            Activities = new ObservableCollection<Activity>();
            DataContext = this;

            currentUser = GenerateNewUserId();

            calendar.SelectedDatesChanged += Calendar_SelectedDatesChanged;

            if (calendar.SelectedDate.HasValue)
            {
                LoadActivities(calendar.SelectedDate.Value);
            }
        }

        private void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (calendar.SelectedDate.HasValue)
            {
                DateTime selectedDate = calendar.SelectedDate.Value;
                LoadActivities(selectedDate);
            }
        }

        private void LoadActivities(DateTime date)
        {
            Activities.Clear();

            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT No, ProjectType, TaskType, Description, Status, User FROM Activities WHERE ActivityDate = @ActivityDate";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@ActivityDate", date.ToString("yyyy-MM-dd"));

                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Activities.Add(new Activity
                        {
                            No = reader.GetInt32("No"),
                            ProjectType = reader.GetString("ProjectType"),
                            TaskType = reader.GetString("TaskType"),
                            Description = reader.GetString("Description"),
                            Status = reader.GetString("Status"),
                            User = reader.GetString("User")
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while loading activities: " + ex.Message);
                }
            }

            dailyDataGrid.ItemsSource = null;
            dailyDataGrid.ItemsSource = Activities;

            overviewDataGrid.ItemsSource = null;
            overviewDataGrid.ItemsSource = Activities;
        }

        private string GenerateNewUserId()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT MAX(User) FROM Activities";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    var result = cmd.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        string lastUser = result.ToString();
                        int lastUserNumber = int.Parse(lastUser.Replace("testUser", ""));
                        return "testUser" + (lastUserNumber + 1);
                    }
                    else
                    {
                        return "testUser1";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while generating new user ID: " + ex.Message);
                    return "testUser1";
                }
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            NewActivityWindow newActivityWindow = new NewActivityWindow(currentUser);
            if (newActivityWindow.ShowDialog() == true)
            {
                if (calendar.SelectedDate.HasValue)
                {
                    LoadActivities(calendar.SelectedDate.Value);
                }
            }
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (dailyDataGrid.SelectedItem is Activity selectedActivity)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this activity?", "Delete Activity", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    DeleteActivityFromDatabase(selectedActivity.No);
                    Activities.Remove(selectedActivity);
                }
            }
        }

        private void DeleteActivityFromDatabase(int activityNo)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "DELETE FROM Activities WHERE No = @No";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@No", activityNo);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while deleting the activity: " + ex.Message);
                }
            }
        }

        private void DailyDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                Activity editedActivity = e.Row.Item as Activity;

                if (ValidateActivity(editedActivity))
                {
                    UpdateActivityInDatabase(editedActivity);
                }
                else
                {
                    MessageBox.Show("Invalid input. Please ensure all fields contain valid values.");
                }
            }
        }

        private void DailyDataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            if (dailyDataGrid.SelectedItem is Activity selectedActivity)
            {
                if (ValidateActivity(selectedActivity))
                {
                    UpdateActivityInDatabase(selectedActivity);
                }
                else
                {
                    MessageBox.Show("Invalid input. Please ensure all fields contain valid values.");
                }
            }
        }

        private bool ValidateActivity(Activity activity)
        {
            if (string.IsNullOrWhiteSpace(activity.ProjectType) ||
                string.IsNullOrWhiteSpace(activity.TaskType) ||
                string.IsNullOrWhiteSpace(activity.Status) ||
                string.IsNullOrWhiteSpace(activity.Description))
            {
                return false;
            }

            return (activity.ProjectType == "Administrative" || activity.ProjectType == "Marketing" || activity.ProjectType == "Meeting") &&
                   (activity.TaskType == "New" || activity.TaskType == "Fix") &&
                   (activity.Status == "In progress" || activity.Status == "On hold" || activity.Status == "Done");
        }

        private void UpdateActivityInDatabase(Activity activity)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE Activities SET ProjectType = @ProjectType, TaskType = @TaskType, Description = @Description, Status = @Status WHERE No = @No";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@No", activity.No);
                    cmd.Parameters.AddWithValue("@ProjectType", activity.ProjectType);
                    cmd.Parameters.AddWithValue("@TaskType", activity.TaskType);
                    cmd.Parameters.AddWithValue("@Description", activity.Description);
                    cmd.Parameters.AddWithValue("@Status", activity.Status);

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while updating the activity: " + ex.Message);
                }
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime? startDate = startDatePicker.SelectedDate;
            DateTime? endDate = endDatePicker.SelectedDate;

            if (startDate.HasValue && endDate.HasValue)
            {
                ExportActivities(startDate.Value, endDate.Value);
            }
            else
            {
                MessageBox.Show("Please select a valid date range.");
            }
        }

        private void ExportActivities(DateTime startDate, DateTime endDate)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            ObservableCollection<Activity> activitiesToExport = new ObservableCollection<Activity>();

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT No, ProjectType, TaskType, Description, Status, User FROM Activities WHERE ActivityDate BETWEEN @StartDate AND @EndDate";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@StartDate", startDate.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@EndDate", endDate.ToString("yyyy-MM-dd"));

                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        activitiesToExport.Add(new Activity
                        {
                            No = reader.GetInt32("No"),
                            ProjectType = reader.GetString("ProjectType"),
                            TaskType = reader.GetString("TaskType"),
                            Description = reader.GetString("Description"),
                            Status = reader.GetString("Status"),
                            User = reader.GetString("User")
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while loading activities: " + ex.Message);
                    return;
                }
            }

            StringBuilder exportText = new StringBuilder();
            exportText.AppendLine($"Team Activity in the period {startDate:dd.MM.yyyy} – {endDate:dd.MM.yyyy}\n");

            var groupedActivities = activitiesToExport.GroupBy(a => a.ProjectType);

            foreach (var group in groupedActivities)
            {
                exportText.AppendLine($"{group.Key}:");

                foreach (var activity in group)
                {
                    exportText.AppendLine($"-{activity.Description} – {activity.Status}");
                }

                exportText.AppendLine();
            }

            string fileName = $"TeamWeekActivity_{startDate:dd.MM.yyyy}_{endDate:dd.MM.yyyy}.txt";
            File.WriteAllText(fileName, exportText.ToString());

            MessageBox.Show($"Activities exported successfully to {fileName}");
        }


    }
}
