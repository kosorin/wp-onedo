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
using System.Collections.Generic;
using System.Windows.Media.Animation;

namespace SimpleTasks.Views
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();
            DataContext = App.Tasks;

            CreateAppBarItems();
            BuildTasksdAppBar();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            NavigationService.RemoveBackEntry();

            App.Tasks.OnPropertyChanged(App.Tasks.GroupedTasksPropertyString);

            // Animace zesvětlení
            TasksPageOverlay.Opacity = 1;
            TasksPageOverlay.Visibility = Visibility.Visible;
            TasksPageOverlayTransitionHide.Completed += (s2, e2) =>
            {
                TasksPageOverlay.Visibility = Visibility.Collapsed;
            };
            TasksPageOverlayTransitionHide.Begin();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (!e.IsNavigationInitiator)
            {
                App.UpdateAllLiveTiles();
            }
        }

        T FindFirstChild<T>(FrameworkElement element, string name = null) where T : FrameworkElement
        {
            int childrenCount = VisualTreeHelper.GetChildrenCount(element);
            var children = new FrameworkElement[childrenCount];

            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(element, i) as FrameworkElement;
                children[i] = child;
                if (child is T)
                {
                    if (name == null || child.Name == name)
                        return (T)child;
                }
            }

            for (int i = 0; i < childrenCount; i++)
            {
                if (children[i] != null)
                {
                    var subChild = FindFirstChild<T>(children[i]);
                    if (subChild != null)
                        return subChild;
                }
            }

            return null;
        }

        #region AppBar

        private ApplicationBarIconButton appBarNewTaskButton;

        private ApplicationBarIconButton appBarSaveQuickButton;

        private ApplicationBarIconButton appBarCancelQuickButton;

        private List<ApplicationBarMenuItem> appBarMenuItems;

        private void CreateAppBarItems()
        {
            #region Ikony
            appBarNewTaskButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.png", UriKind.Relative));
            appBarNewTaskButton.Text = AppResources.AppBarNew;
            appBarNewTaskButton.Click += (s, e) => { NavigationService.Navigate(new Uri("/Views/EditTaskPage.xaml", UriKind.Relative)); };

            appBarSaveQuickButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.save.png", UriKind.Relative));
            appBarSaveQuickButton.Text = AppResources.AppBarSave;
            appBarSaveQuickButton.Click += (s, e) =>
            {
                string title = QuickAddTextBox.Text;
                if (!string.IsNullOrWhiteSpace(title))
                {
                    TaskModel task = new TaskModel()
                    {
                        Title = title,
                        DueDate = App.Settings.DefaultDueDateSettingToDateTime
                    };
                    App.Tasks.Add(task);
                    QuickAddTextBox.Text = "";
                    this.Focus();
                }
            };

            appBarCancelQuickButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.close.png", UriKind.Relative));
            appBarCancelQuickButton.Text = AppResources.AppBarCancel;
            appBarCancelQuickButton.Click += (s, e) => { QuickAddTextBox.Text = ""; this.Focus(); };
            #endregion

            #region Menu

            appBarMenuItems = new List<ApplicationBarMenuItem>();
            // Smazat dokončené úkoly
            ApplicationBarMenuItem appBarDeleteCompletedItem = new ApplicationBarMenuItem(AppResources.AppBarDeleteCompleted);
            appBarDeleteCompletedItem.Click += (s, e) => { OverlayAction(App.Tasks.DeleteCompleted); };
            appBarMenuItems.Add(appBarDeleteCompletedItem);

            // Smazat všechny úkoly
            ApplicationBarMenuItem appBarDeleteAllItem = new ApplicationBarMenuItem(AppResources.AppBarDeleteAll);
            appBarDeleteAllItem.Click += appBarDeleteAllItem_Click;
            appBarMenuItems.Add(appBarDeleteAllItem);

            // Nastavení
            ApplicationBarMenuItem appBarSettingsMenuItem = new ApplicationBarMenuItem(AppResources.AppBarSettings);
            appBarSettingsMenuItem.Click += (s, e) => { NavigationService.Navigate(new Uri("/Views/SettingsPage.xaml", UriKind.Relative)); };
            appBarMenuItems.Add(appBarSettingsMenuItem);

            // O aplikaci
            ApplicationBarMenuItem appBarAboutMenuItem = new ApplicationBarMenuItem(AppResources.AppBarAbout);
            appBarAboutMenuItem.Click += (s, e) => { NavigationService.Navigate(new Uri("/Views/AboutPage.xaml", UriKind.Relative)); };
            appBarMenuItems.Add(appBarAboutMenuItem);

#if DEBUG
            // Reset
            ApplicationBarMenuItem appBarResetMenuItem = new ApplicationBarMenuItem("resetovat data");
            appBarResetMenuItem.Click += appBarResetMenuItem_Click;
            appBarMenuItems.Add(appBarResetMenuItem);

            // Clear
            ApplicationBarMenuItem appBarClearMenuItem = new ApplicationBarMenuItem("smazat data");
            appBarClearMenuItem.Click += (s, e) => { App.Tasks.DeleteAll(); };
            appBarMenuItems.Add(appBarClearMenuItem);
#endif
            #endregion
        }

        private void BuildTasksdAppBar()
        {
            ApplicationBar = new ApplicationBar();

            ApplicationBar.Buttons.Add(appBarNewTaskButton);

            foreach (var item in appBarMenuItems)
            {
                ApplicationBar.MenuItems.Add(item);
            }
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
                    OverlayAction(App.Tasks.DeleteAll);
                    break;
                case CustomMessageBoxResult.RightButton:
                case CustomMessageBoxResult.None:
                default:
                    break;
                }
            };


            messageBox.Show();
        }

        void appBarResetMenuItem_Click(object sender, EventArgs e)
        {
            App.Tasks.DeleteAll();

            App.Tasks.Add(new TaskModel()
            {
                Title = "Go to the dentist",
                DueDate = DateTimeExtensions.Today.AddDays(2),
                CompletedDate = DateTime.Now
            });
            App.Tasks.Add(new TaskModel()
            {
                Title = "Call Chuck",
                DueDate = DateTimeExtensions.Today.AddDays(0),
                ReminderDate = DateTimeExtensions.Today.AddHours(21).AddMinutes(13),
                Priority = TaskPriority.High
            });
            App.Tasks.Add(new TaskModel()
            {
                Title = "Simple list",
                Detail = " \u2022 just simple\n \u2022 bulleted list\n \u2022 I don't know\n \u2022 what to say",
                DueDate = DateTimeExtensions.Today.AddDays(5),
                ReminderDate = DateTimeExtensions.Today.AddHours(65).AddMinutes(27),
                Priority = TaskPriority.Low
            });
            App.Tasks.Add(new TaskModel()
            {
                Title = "Math project",
                DueDate = DateTimeExtensions.Today.AddDays(3),
                ReminderDate = DateTimeExtensions.Today.AddDays(4).AddHours(13).AddMinutes(42)
            });

            if (App.ForceDebugCulture == "cs-CZ")
            {
                App.Tasks.Tasks[0].Title = "Jít k zubaři";
                App.Tasks.Tasks[1].Title = "Zavolat Honzovi";
                App.Tasks.Tasks[2].Title = "Jednoduchý seznam";
                App.Tasks.Tasks[2].Detail = " \u2022 prostě jednoduché\n \u2022 odrážky\n \u2022 nevím, co sem\n \u2022 mám napsat";
                App.Tasks.Tasks[3].Title = "Projekt do matematiky";
            }
            else if (App.ForceDebugCulture == "sk-SK")
            {
                App.Tasks.Tasks[0].Title = "Ísť k zubárovi";
                App.Tasks.Tasks[1].Title = "Kúpiť mlieko";
                App.Tasks.Tasks[2].Title = "Jednoduchý zoznam";
                App.Tasks.Tasks[2].Detail = " \u2022 len jednoduché\n \u2022 odrážky\n \u2022 neviem, čo by som\n \u2022 tu mal napísať";
                App.Tasks.Tasks[3].Title ="Projekt z matematiky";
            }
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

        #region TasksList

        private void Border_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.Focus();

            Border border = ((sender as Border).Parent as Grid).Parent as Border;
            Storyboard showStoryboard = ((Storyboard)border.FindName("TaskInfoShow"));
            Storyboard hideStoryboard = ((Storyboard)border.FindName("TaskInfoHide"));
            TaskWrapper wrapper = border.DataContext as TaskWrapper;
            TaskModel task = wrapper.Task;
            if (task == null)
                return;

            if (task.IsActive)
            {   // DOKONČENÍ
                task.CompletedDate = DateTime.Now;
                task.ReminderDate = null;
                if (App.Settings.UnpinCompletedSetting)
                {
                    LiveTile.Unpin(task);
                }

                wrapper.Animation = true;
                hideStoryboard.Completed += (s2, e2) => { wrapper.Animation = false; };
                hideStoryboard.Begin();
            }
            else
            {   // AKTIVOVÁNÍ
                task.CompletedDate = null;

                wrapper.Animation = true;
                showStoryboard.Completed += (s2, e2) => { wrapper.Animation = false; };
                showStoryboard.Begin();
            }
            App.Tasks.Update(task);
        }

        private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Grid grid = (Grid)sender;
            TaskWrapper wrapper = (TaskWrapper)grid.DataContext;
            TaskModel task = wrapper.Task;
            if (task == null)
                return;

            NavigationService.Navigate(new Uri(string.Format("/Views/EditTaskPage.xaml?Task={0}", task.Uid), UriKind.Relative));
        }

        private void TasksLongListSelector_ItemRealized(object sender, ItemRealizationEventArgs e)
        {
            if (e.ItemKind == LongListSelectorItemKind.Item)
            {
                //TaskWrapper wrapper = e.Container.DataContext as TaskWrapper;
                //TaskModel task = wrapper.Task;

                //Debug.WriteLine(" REALIZED: " + task.Title);

                //StackPanel panel = FindFirstChild<StackPanel>(e.Container, "TaskInfoStackPanel");
                //wrapper.Height = panel.ActualHeight;
                ////panel.Height = panel.ActualHeight;
                ////var r = new Random(task.Title.GetHashCode());
                ////panel.Background = new SolidColorBrush(Color.FromArgb(255, (byte)r.Next(255), (byte)r.Next(255), (byte)r.Next(255)));
                //Border rootBorder = FindFirstChild<Border>(e.Container, "RootBorder");
                //Storyboard showStoryboard = ((Storyboard)rootBorder.FindName("TaskInfoShow"));
                //Storyboard hideStoryboard = ((Storyboard)rootBorder.FindName("TaskInfoHide"));

                //if (task == null)
                //    return;
                //if (task.IsActive)
                //{
                //    showStoryboard.Begin();
                //    hideStoryboard.Stop();
                //}
                //else
                //{
                //    showStoryboard.Stop();
                //    hideStoryboard.Begin();
                //}
            }
        }

        private void TasksLongListSelector_Loaded(object sender, RoutedEventArgs e)
        {
            // Změnění margin scrollbaru. 
            // Při větším počtu úkolů se automaticky měnila šířka celého LLS.
            ScrollBar scrollBar = ((FrameworkElement)VisualTreeHelper.GetChild(TasksLongListSelector, 0)).FindName("VerticalScrollBar") as ScrollBar;
            if (scrollBar != null)
                scrollBar.Margin = new Thickness(-10, 0, 0, 0);
        }

        private void TaskInfoStackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            StackPanel panel = (StackPanel)sender;
            TaskWrapper wrapper = panel.DataContext as TaskWrapper;
            TaskModel task = wrapper.Task;

            //Grid moreInfo = (Grid)panel.FindName("TaskInfoMoreInfo");
            //Grid textBlock = (Grid)panel.FindName("DetailGridWrapper");

            //double moreInfoHeight = moreInfo.ActualHeight + 10;
            //double detailHeight = (textBlock.Visibility == Visibility.Visible) ? textBlock.ActualHeight + 10 : 0;
            //double oldHeight = moreInfoHeight + detailHeight;
            //wrapper.Height = oldHeight;
            //panel.Height = oldHeight;

            Border rootBorder = (Border)((Grid)((Grid)panel.Parent).Parent).Parent;
            Storyboard showStoryboard = ((Storyboard)rootBorder.FindName("TaskInfoShow"));
            Storyboard hideStoryboard = ((Storyboard)rootBorder.FindName("TaskInfoHide"));
            if (task.IsActive)
            {
                showStoryboard.Begin();
                hideStoryboard.Stop();
            }
            else
            {
                showStoryboard.Stop();
                hideStoryboard.Begin();
            }
        }

        #endregion

        #region QuickAddTextBox

        private void QuickAddTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            PageOverlayTransitionQuickHide.Completed += (s2, e2) =>
            {
                PageOverlay.Visibility = Visibility.Collapsed;
            };
            PageOverlayTransitionQuickHide.Begin();

            ApplicationBar.Buttons.Remove(appBarSaveQuickButton);
            ApplicationBar.Buttons.Remove(appBarCancelQuickButton);
            ApplicationBar.Buttons.Add(appBarNewTaskButton);
        }

        private void QuickAddTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PageOverlay.Visibility = Visibility.Visible;
            PageOverlayTransitionQuickShow.Begin();

            ApplicationBar.Buttons.Remove(appBarNewTaskButton);
            ApplicationBar.Buttons.Add(appBarSaveQuickButton);
            ApplicationBar.Buttons.Add(appBarCancelQuickButton);
        }

        #endregion

    }
}