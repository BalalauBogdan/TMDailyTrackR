using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;

namespace TM.DailyTrackR.View
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<Activity> Activities { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Activities = new ObservableCollection<Activity>();
            DataContext = this;

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
                    string query = "SELECT No, ProjectType, TaskType, Description, Status FROM Activities WHERE ActivityDate = @ActivityDate";
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
                            Status = reader.GetString("Status")
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while loading activities: " + ex.Message);
                }
            }

            dataGrid.ItemsSource = null;
            dataGrid.ItemsSource = Activities;
        }
    }

    public class Activity
    {
        public int No { get; set; }
        public string ProjectType { get; set; }
        public string TaskType { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }
}
