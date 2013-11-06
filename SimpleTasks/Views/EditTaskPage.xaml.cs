﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SimpleTasks.ViewModels;
using SimpleTasks.Resources;
using SimpleTasks.Models;
using System.Windows.Input;
using SimpleTasks.Core.Helpers;
using SimpleTasks.Core.Models;

namespace SimpleTasks.Views
{
    public partial class EditTaskPage : PhoneApplicationPage
    {
        public EditTaskViewModel ViewModel { get; private set; }

        public EditTaskPage()
        {
            InitializeComponent();

            // Zjistíme, jaký úkol se bude upravovat (pokud se nepřidává nový úkol)
            TaskModel taskToEdit = null;
            if (PhoneApplicationService.Current.State.ContainsKey("TaskToEdit"))
            {
                taskToEdit = (TaskModel)PhoneApplicationService.Current.State["TaskToEdit"];
                PhoneApplicationService.Current.State["TaskToEdit"] = null;
            }

            ViewModel = new EditTaskViewModel(taskToEdit);
            DataContext = ViewModel;

            BuildLocalizedApplicationBar();

            // Při přidání nového úkolu se zobrází klávesnice a nastaví defaultní termín
            if (!ViewModel.IsOldTask)
            {
                RoutedEventHandler firstTimeLoadHandler = null;
                firstTimeLoadHandler = (s, e) =>
                {
                    // Zobrazení klávesnice
                    TitleTextBox.Focus();

                    // Nastavení defaultního termínu
                    if (App.Settings.DefaultDueDateSettingToDateTime != null)
                    {
                        DueDateToggleButton.IsChecked = true;
                    }

                    this.Loaded -= firstTimeLoadHandler;
                };
                this.Loaded += firstTimeLoadHandler;
            }
        }

        private bool CanSave()
        {
            if (TitleTextBox.Text == "")
            {
                MessageBox.Show(AppResources.MissingTitleText);
                return false;
            }
            else if (ReminderToggleButton.IsChecked.Value && ViewModel.CurrentTask.ReminderDate <= DateTime.Now)
            {
                MessageBox.Show("Datum a čas připomenutí musí být v budoucnosti.");
                return false;
            }
            else
            {
                return true;
            }
        }

        private void PrepareSave()
        {
            if (!DueDateToggleButton.IsChecked.Value)
            {
                DueDatePicker.Value = null;
            }
            if (!ReminderToggleButton.IsChecked.Value)
            {
                ReminderDatePicker.Value = null;
            }
        }

        #region AppBar

        private void BuildLocalizedApplicationBar()
        {
            ApplicationBar = new ApplicationBar();

            // Ikony
            if (ViewModel.IsOldTask)
            {
                if (ViewModel.CurrentTask.IsComplete)
                {
                    ApplicationBarIconButton appBarActivateButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.undo.curve.png", UriKind.Relative));
                    appBarActivateButton.Text = AppResources.AppBarActivateText;
                    appBarActivateButton.Click += appBarActivateButton_Click;
                    ApplicationBar.Buttons.Add(appBarActivateButton);
                }
                else
                {
                    ApplicationBarIconButton appBarCompleteButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.checkmark.pencil.top.png", UriKind.Relative));
                    appBarCompleteButton.Text = AppResources.AppBarCompleteText;
                    appBarCompleteButton.Click += appBarCompleteButton_Click;
                    ApplicationBar.Buttons.Add(appBarCompleteButton);
                }
            }

            ApplicationBarIconButton appBarSaveButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.save.png", UriKind.Relative));
            appBarSaveButton.Text = AppResources.AppBarSaveText;
            appBarSaveButton.Click += appBarSaveButton_Click;
            ApplicationBar.Buttons.Add(appBarSaveButton);


            // Menu
            if (ViewModel.IsOldTask)
            {
                ApplicationBarMenuItem appBarDeleteItem = new ApplicationBarMenuItem();
                appBarDeleteItem.Text = AppResources.AppBarDeleteText;
                appBarDeleteItem.Click += appBarDeleteItem_Click;
                ApplicationBar.MenuItems.Add(appBarDeleteItem);
            }
        }

        private void appBarActivateButton_Click(object sender, EventArgs e)
        {
            if (CanSave())
            {
                PrepareSave();
                ViewModel.ActivateTask();
                NavigationService.GoBack();
            }
        }

        private void appBarCompleteButton_Click(object sender, EventArgs e)
        {
            if (CanSave())
            {
                PrepareSave();
                ViewModel.CompleteTask();
                NavigationService.GoBack();
            }
        }

        private void appBarSaveButton_Click(object sender, EventArgs e)
        {
            if (CanSave())
            {
                PrepareSave();
                ViewModel.SaveTask();
                NavigationService.GoBack();
            }
        }

        private void appBarDeleteItem_Click(object sender, EventArgs e)
        {
            CustomMessageBox messageBox = new CustomMessageBox()
            {
                Caption = AppResources.DeleteTaskCaptionText,
                Message = AppResources.DeleteTaskText
                            + Environment.NewLine + Environment.NewLine
                            + ViewModel.CurrentTask.Title,
                LeftButtonContent = AppResources.DeleteTaskYesText,
                RightButtonContent = AppResources.DeleteTaskNoText
            };

            messageBox.Dismissed += (s1, e1) =>
            {
                switch (e1.Result)
                {
                case CustomMessageBoxResult.LeftButton:
                    ViewModel.DeleteTask();
                    NavigationService.GoBack();
                    break;
                case CustomMessageBoxResult.RightButton:
                case CustomMessageBoxResult.None:
                default:
                    break;
                }
            };
            messageBox.Show();
        }

        #endregion

        #region Task Title

        private void PhoneTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Focus();
            }
        }

        #endregion

        #region Due Date

        private void DueToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentTask.DueDate == null)
            {
                if (App.Settings.DefaultDueDateSettingToDateTime != null)
                {
                    DueDatePicker.Value = App.Settings.DefaultDueDateSettingToDateTime;
                }
                else
                {
                    DueDatePicker.Value = DateTimeExtensions.Today;
                }
            }
            else
            {
                DueDatePicker.Value = ViewModel.CurrentTask.DueDate;
            }

            // Animace zobrazení
            DueDatePicker.Visibility = Visibility.Visible;
            DueDatePickerShow.Begin();
            DueDatePickerShow.Completed += (s2, e2) =>
            {
                DueDatePicker.Visibility = Visibility.Visible;
                DueDatePicker.IsEnabled = true;
            };
        }

        private void DueToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            // Animace skrytí
            DueDatePicker.IsEnabled = false;
            DueDatePickerHide.Begin();
            DueDatePickerHide.Completed += (s2, e2) =>
            {
                DueDatePicker.Visibility = Visibility.Collapsed;
            };
        }

        #endregion

        #region Reminder

        private void ReminderToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentTask.ReminderDate == null)
            {
                if (ViewModel.CurrentTask.DueDate == null)
                {
                    ReminderDatePicker.Value = DateTime.Now;
                    ReminderTimePicker.Value = DateTime.Now.AddHours(1);
                }
                else
                {
                    ReminderDatePicker.Value = ViewModel.CurrentTask.DueDate;
                    ReminderTimePicker.Value = ViewModel.CurrentTask.DueDate;
                }
            }
            else
            {
                ReminderDatePicker.Value = ViewModel.CurrentTask.ReminderDate;
                ReminderTimePicker.Value = ViewModel.CurrentTask.ReminderDate;
            }

            // Animace zobrazení
            ReminderGrid.Visibility = Visibility.Visible;
            ReminderGridShow.Begin();
            ReminderGridShow.Completed += (s2, e2) =>
            {
                ReminderDatePicker.IsEnabled = true;
                ReminderTimePicker.IsEnabled = true;
            };
        }

        private void ReminderToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            // Animace skrytí
            ReminderDatePicker.IsEnabled = false;
            ReminderTimePicker.IsEnabled = false;
            ReminderGridHide.Begin();
            ReminderGridHide.Completed += (s2, e2) =>
            {
                ReminderGrid.Visibility = Visibility.Collapsed;
            };
        }

        private DateTime? GetReminderDate()
        {
            return DateTime.Now;
            //if (ReminderToggleButton.IsChecked.Value)
            //{
            //    return 
            //}
        }

        #endregion

    }
}