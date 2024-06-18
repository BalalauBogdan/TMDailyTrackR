namespace TM.DailyTrackR
{
  using System.Windows;
  using TM.DailyTrackR.View;
  using TM.DailyTrackR.ViewModel;

  public partial class App : Application
  {
    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);

      //Register View

      MainWindow window = new();
      window.DataContext = new MainWindowViewModel();
      window.ShowDialog();
    }
  }
}
