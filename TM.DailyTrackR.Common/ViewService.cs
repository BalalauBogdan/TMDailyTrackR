namespace TM.DailyTrackR.Common
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Windows;
  using Prism.Mvvm;

  public class ViewService
  {
    private static readonly Lazy<ViewService> lazy = new(() => new(), isThreadSafe: true);
    private Dictionary<BindableBase, Window> openedWindows = new();
    private Dictionary<Type, Type> registeredViews = new();

    private ViewService()
    {
    }

    public static ViewService Instance => lazy.Value;

    public void RegisterView(Type viewModel, Type window)
    {
      Instance.registeredViews[viewModel] = window;
    }

    public void ShowDialog(BindableBase viewModel, BindableBase? parent = null, Type? parentViewModelType = null)
    {
      if (Instance.registeredViews.TryGetValue(viewModel.GetType(), out Type? windowType))
      {
        Window? window = Activator.CreateInstance(windowType) as Window;

        if (window != null)
        {
          window.DataContext = viewModel;

          Instance.openedWindows[viewModel] = window;

          if (parent != null)
          {
            if (Instance.openedWindows.TryGetValue(parent, out Window? parentWindow))
            {
              window.Owner = parentWindow;
            }
          }

          if (parent == null && parentViewModelType != null)
          {
            var parentWindow = Instance.openedWindows.FirstOrDefault(x => x.Key.GetType() == parentViewModelType).Value;
            window.Owner = parentWindow;
          }

          window.ShowDialog();
        }
      }
    }

    public void ShowWindow(BindableBase viewModel)
    {
      if (Instance.registeredViews.TryGetValue(viewModel.GetType(), out Type? windowType))
      {
        Window? window = Activator.CreateInstance(windowType) as Window;

        if (window != null)
        {
          window.DataContext = viewModel;

          Instance.openedWindows[viewModel] = window;

          window.Show();
        }
      }
    }
  }
}
