﻿using System;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SimpleTasks.ViewModels;
using SimpleTasks.Resources;
using System.Windows;
using SimpleTasks.Models;
using SimpleTasks.Core.Helpers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Diagnostics;
using SimpleTasks.Core.Models;
using Microsoft.Phone.Scheduler;
using SimpleTasks.Helpers;

namespace SimpleTasks.Views
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainViewModel ViewModel { get; private set; }

        public MainPage()
        {
            InitializeComponent();
            DataContext = ViewModel = App.ViewModel;

            //LiveTile.UpdateOrReset(App.Settings.EnableLiveTileSetting, ViewModel.Tasks);

            BuildLocalizedApplicationBar();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            NavigationService.RemoveBackEntry();
        }

        private ApplicationBarIconButton appBarNewTaskButton;

        private ApplicationBarIconButton appBarSaveQuickButton;

        private ApplicationBarIconButton appBarCancelQuickButton;

        #region AppBar

        private void BuildLocalizedApplicationBar()
        {
            ApplicationBar = new ApplicationBar();

            #region Ikony

            // Přidat úkol
            appBarNewTaskButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.png", UriKind.Relative));
            appBarNewTaskButton.Text = AppResources.AppBarNew;
            appBarNewTaskButton.Click += (s, e) => { NavigationService.Navigate(new Uri("/Views/EditTaskPage.xaml", UriKind.Relative)); };
            ApplicationBar.Buttons.Add(appBarNewTaskButton);

            // Uložit rychlý úkol
            appBarSaveQuickButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.save.png", UriKind.Relative));
            appBarSaveQuickButton.Text = AppResources.AppBarSave;
            appBarSaveQuickButton.Click += (s, e) =>
            {
                string title = QuickAddTextBox.Text;
                if (!string.IsNullOrWhiteSpace(title))
                {
                    ViewModel.AddTask(new TaskModel() { Title = title });
                    QuickAddTextBox.Text = "";
                    this.Focus();
                }
            };

            // Zrušit rychlý úkol
            appBarCancelQuickButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.close.png", UriKind.Relative));
            appBarCancelQuickButton.Text = AppResources.AppBarCancel;
            appBarCancelQuickButton.Click += (s, e) => { QuickAddTextBox.Text = ""; this.Focus(); };

            #endregion

            #region Menu

            // Smazat dokončené úkoly
            ApplicationBarMenuItem appBarDeleteCompletedItem = new ApplicationBarMenuItem(AppResources.AppBarDeleteCompleted);
            appBarDeleteCompletedItem.Click += (s, e) => { OverlayAction(ViewModel.DeleteCompletedTasks); };
            ApplicationBar.MenuItems.Add(appBarDeleteCompletedItem);

            // Smazat všechny úkoly
            ApplicationBarMenuItem appBarDeleteAllItem = new ApplicationBarMenuItem(AppResources.AppBarDeleteAll);
            appBarDeleteAllItem.Click += appBarDeleteAllItem_Click;
            ApplicationBar.MenuItems.Add(appBarDeleteAllItem);

            // Nastavení
            ApplicationBarMenuItem appBarSettingsMenuItem = new ApplicationBarMenuItem(AppResources.AppBarSettings);
            appBarSettingsMenuItem.Click += (s, e) => { NavigationService.Navigate(new Uri("/Views/SettingsPage.xaml", UriKind.Relative)); };
            ApplicationBar.MenuItems.Add(appBarSettingsMenuItem);

            // O aplikaci
            ApplicationBarMenuItem appBarAboutMenuItem = new ApplicationBarMenuItem(AppResources.AppBarAbout);
            appBarAboutMenuItem.Click += (s, e) => { NavigationService.Navigate(new Uri("/Views/AboutPage.xaml", UriKind.Relative)); };
            ApplicationBar.MenuItems.Add(appBarAboutMenuItem);

#if DEBUG
            // Reset
            ApplicationBarMenuItem appBarResetMenuItem = new ApplicationBarMenuItem("resetovat data");
            appBarResetMenuItem.Click += appBarResetMenuItem_Click;
            ApplicationBar.MenuItems.Add(appBarResetMenuItem);

            // Clear
            ApplicationBarMenuItem appBarClearMenuItem = new ApplicationBarMenuItem("smazat data");
            appBarClearMenuItem.Click += (s, e) => { ViewModel.Tasks.Clear(); LiveTile.UpdateUI(ViewModel.Tasks); };
            ApplicationBar.MenuItems.Add(appBarClearMenuItem);
#endif
            #endregion
        }

        void appBarDeleteAllItem_Click(object sender, EventArgs e)
        {
            CustomMessageBox messageBox = new CustomMessageBox()
            {
                Caption = AppResources.DeleteAllTasksCaption,
                Message = AppResources.DeleteAllTasks,
                LeftButtonContent = AppResources.DeleteTaskYes,
                RightButtonContent = AppResources.DeleteTaskNo
            };

            messageBox.Dismissed += (s1, e1) =>
            {
                switch (e1.Result)
                {
                case CustomMessageBoxResult.LeftButton:
                    OverlayAction(ViewModel.DeleteAllTasks);
                    break;
                case CustomMessageBoxResult.RightButton:
                case CustomMessageBoxResult.None:
                default:
                    break;
                }
            };


            messageBox.Show();
        }

        private void SetAppBarLiveTileItem(ApplicationBarMenuItem appBarItem)
        {

        }

        void appBarResetMenuItem_Click(object sender, EventArgs e)
        {
            ViewModel.Tasks.Clear();

            if (App.ForceDebugCulture == "en-US")
            {
                ViewModel.Tasks.Add(new TaskModel()
                {
                    Title = "Go to the dentist",
                    DueDate = DateTimeExtensions.Today.AddDays(2)
                });
                ViewModel.Tasks.Add(new TaskModel()
                {
                    Title = "Call Chuck",
                    DueDate = DateTimeExtensions.Today.AddDays(0),
                    ReminderDate = DateTimeExtensions.Today.AddHours(21).AddMinutes(13),
                    Priority = TaskPriority.Low
                });
                ViewModel.Tasks.Add(new TaskModel()
                {
                    Title = "Go to cinema" + Environment.NewLine + "Amazing Spider-Man 2 or X-Men: Days of Future Past",
                    ReminderDate = DateTimeExtensions.Today.AddHours(65).AddMinutes(27),
                });
                ViewModel.Tasks.Add(new TaskModel()
                {
                    Title = "Math project",
                    DueDate = DateTimeExtensions.Today.AddDays(9),
                    ReminderDate = DateTimeExtensions.Today.AddDays(4).AddHours(13).AddMinutes(42),
                    Priority = TaskPriority.High
                });
            }
            else if (App.ForceDebugCulture == "cs-CZ")
            {
                ViewModel.Tasks.Add(new TaskModel()
                {
                    Title = "Jít k zubaři",
                    DueDate = DateTimeExtensions.Today.AddDays(2)
                });
                ViewModel.Tasks.Add(new TaskModel()
                {
                    Title = "Zavolat Honzovi",
                    DueDate = DateTimeExtensions.Today.AddDays(0),
                    ReminderDate = DateTimeExtensions.Today.AddHours(21).AddMinutes(13),
                    Priority = TaskPriority.Low
                });
                ViewModel.Tasks.Add(new TaskModel()
                {
                    Title = "Jít do kina" + Environment.NewLine + "Amazing Spider-Man 2 nebo X-Men: Days of Future Past",
                    ReminderDate = DateTimeExtensions.Today.AddHours(65).AddMinutes(27),
                });
                ViewModel.Tasks.Add(new TaskModel()
                {
                    Title = "Projekt do matematiky",
                    DueDate = DateTimeExtensions.Today.AddDays(9),
                    ReminderDate = DateTimeExtensions.Today.AddDays(4).AddHours(13).AddMinutes(42),
                    Priority = TaskPriority.High
                });
            }
            else if (App.ForceDebugCulture == "sk-SK")
            {
                ViewModel.Tasks.Add(new TaskModel()
                {
                    Title = "Ísť k zubárovi",
                    DueDate = DateTimeExtensions.Today.AddDays(2)
                });
                ViewModel.Tasks.Add(new TaskModel()
                {
                    Title = "Kúpiť mlieko",
                    DueDate = DateTimeExtensions.Today.AddDays(0),
                    ReminderDate = DateTimeExtensions.Today.AddHours(21).AddMinutes(13),
                    Priority = TaskPriority.Low
                });
                ViewModel.Tasks.Add(new TaskModel()
                {
                    Title = "Ísť do kina" + Environment.NewLine + "Amazing Spider-Man 2 alebo X-Men: Days of Future Past",
                    ReminderDate = DateTimeExtensions.Today.AddHours(65).AddMinutes(27),
                });
                ViewModel.Tasks.Add(new TaskModel()
                {
                    Title = "Projekt z matematiky",
                    DueDate = DateTimeExtensions.Today.AddDays(9),
                    ReminderDate = DateTimeExtensions.Today.AddDays(4).AddHours(13).AddMinutes(42),
                    Priority = TaskPriority.High
                });
            }

            LiveTile.UpdateUI(ViewModel.Tasks);
        }

        private void OverlayAction(Action action)
        {
            PageOverlay.Visibility = Visibility.Visible;
            PageOverlayTransitionShow.Completed += (s2, e2) =>
            {
                action();

                PageOverlayTransitionHide.Completed += (s3, e3) =>
                {
                    PageOverlay.Visibility = Visibility.Collapsed;
                };
                PageOverlayTransitionHide.Begin();
            };
            PageOverlayTransitionShow.Begin();
        }

        #endregion

        private void TasksLongListSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LongListSelector selector = sender as LongListSelector;
            if (selector == null)
                return;

            TaskModel task = selector.SelectedItem as TaskModel;
            if (task == null)
                return;

            selector.SelectedItem = null;

            NavigationService.Navigate(new Uri(string.Format("/Views/EditTaskPage.xaml?Task={0}", task.Uid), UriKind.Relative));
        }

        private void TasksLongListSelector_Loaded(object sender, RoutedEventArgs e)
        {
            // Změnění margin scrollbaru. 
            // Při větším počtu úkolů se automaticky měnila šířka celého LLS.
            ScrollBar scrollBar = ((FrameworkElement)VisualTreeHelper.GetChild(TasksLongListSelector, 0)).FindName("VerticalScrollBar") as ScrollBar;
            if (scrollBar != null)
                scrollBar.Margin = new Thickness(-10, 0, 0, 0);
        }

        #region QuickAddTextBox

        private void QuickAddTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ApplicationBar.Buttons.Remove(appBarSaveQuickButton);
            ApplicationBar.Buttons.Remove(appBarCancelQuickButton);
            ApplicationBar.Buttons.Add(appBarNewTaskButton);
        }

        private void QuickAddTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ApplicationBar.Buttons.Remove(appBarNewTaskButton);
            ApplicationBar.Buttons.Add(appBarSaveQuickButton);
            ApplicationBar.Buttons.Add(appBarCancelQuickButton);
        }

        #endregion
    }
}