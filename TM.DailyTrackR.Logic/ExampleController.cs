namespace TM.DailyTrackR.Logic
{
  using System.Data.SqlClient;

  public sealed class ExampleController
  {
    string connectionString = @"Server=.\TM_DAILY_TRACKR;Database=TRACKER_DATA;Integrated Security=true;";

    public int GetDataExample()
    {
      string query = "SELECT @@VERSION";

      using SqlConnection connection = new SqlConnection(connectionString);

      try
      {
        connection.Open();

        using SqlCommand command = new SqlCommand(query, connection);
        string version = (string)command.ExecuteScalar();
        System.Diagnostics.Debug.WriteLine("SQL Server Version: " + version);
      }
      catch (Exception ex)
      {
        Console.WriteLine("An error occurred: " + ex.Message);
      }

      return 0;
    }
  }
}
