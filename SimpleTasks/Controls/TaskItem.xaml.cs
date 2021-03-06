﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SimpleTasks.Core.Models;
using SimpleTasks.Helpers;

namespace SimpleTasks.Controls
{
    public class TaskEventArgs : RoutedEventArgs
    {
        public TaskModel Task { get; set; }

        public TaskItem Item { get; set; }

        public bool Delete { get; set; }

        public TaskEventArgs(TaskModel task, TaskItem item)
        {
            Task = task;
            Item = item;
            Delete = false;
        }
    }

    public class TaskSubtaskEventArgs : RoutedEventArgs
    {
        public TaskModel Task { get; set; }

        public Subtask Subtask { get; set; }

        public bool Delete { get; set; }

        public TaskSubtaskEventArgs(TaskModel task, Subtask subtask)
        {
            Task = task;
            Subtask = subtask;
            Delete = false;
        }
    }

    [TemplateVisualState(GroupName = CompleteStatesGroup, Name = CompletedState)]
    [TemplateVisualState(GroupName = CompleteStatesGroup, Name = UncompletedState)]
    [TemplateVisualState(GroupName = GestureStatesGroup, Name = GestureStartDragState)]
    [TemplateVisualState(GroupName = GestureStatesGroup, Name = GestureDragOkState)]
    [TemplateVisualState(GroupName = GestureStatesGroup, Name = GestureDragState)]
    [TemplateVisualState(GroupName = GestureStatesGroup, Name = GestureEndDragState)]
    [TemplateVisualState(GroupName = DeleteStatesGroup, Name = DeletedState)]
    [TemplateVisualState(GroupName = DeleteStatesGroup, Name = NotDeletedState)]
    [TemplateVisualState(GroupName = SubtasksStatesGroup, Name = ShowSubtasksState)]
    [TemplateVisualState(GroupName = SubtasksStatesGroup, Name = HideSubtasksState)]
    public partial class TaskItem : UserControl, INotifyPropertyChanged
    {
        #region Events
        private void OnTaskEvent(EventHandler<TaskEventArgs> handler)
        {
            if (handler != null)
            {
                TaskEventArgs args = new TaskEventArgs(Task, this);
                handler(this, args);
                if (args.Delete)
                {
                    args.Delete = false;
                    RootBorder.Height = RootBorder.ActualHeight;
                    UpdateVisualState(DeletedState);
                }
            }
        }

        private bool OnSubtaskEvent(EventHandler<TaskSubtaskEventArgs> handler, Subtask subtask)
        {
            if (handler != null)
            {
                TaskSubtaskEventArgs args = new TaskSubtaskEventArgs(Task, subtask);
                handler(this, args);
                return args.Delete;
            }

            return false;
        }

        public event EventHandler<TaskEventArgs> SwipeLeft;
        private void OnSwipeLeft()
        {
            OnTaskEvent(SwipeLeft);
        }

        public event EventHandler<TaskEventArgs> SwipeRight;
        private void OnSwipeRight()
        {
            OnTaskEvent(SwipeRight);
        }

        public event EventHandler<TaskEventArgs> Check;
        private void OnCheck()
        {
            OnTaskEvent(Check);
        }

        public event EventHandler<TaskEventArgs> Click;
        private void OnClick()
        {
            OnTaskEvent(Click);
        }

        public event EventHandler<TaskSubtaskEventArgs> SubtaskSwipeLeft;
        private bool OnSwipeLeft(Subtask subtask)
        {
            return OnSubtaskEvent(SubtaskSwipeLeft, subtask);
        }

        public event EventHandler<TaskSubtaskEventArgs> SubtaskSwipeRight;
        private bool OnSwipeRight(Subtask subtask)
        {
            return OnSubtaskEvent(SubtaskSwipeRight, subtask);
        }

        public event EventHandler<TaskSubtaskEventArgs> SubtaskCheck;
        private void OnCheck(Subtask subtask)
        {
            OnSubtaskEvent(SubtaskCheck, subtask);
        }

        public event EventHandler<TaskSubtaskEventArgs> SubtaskClick;
        private void OnClick(Subtask subtask)
        {
            OnSubtaskEvent(SubtaskClick, subtask);
        }
        #endregion // end of Events

        #region (Dependency) Properties
        public TaskModel Task
        {
            get { return DataContext as TaskModel; }
        }

        public bool IsCompleted
        {
            get { return (bool)GetValue(IsCompletedProperty); }
            set { SetValue(IsCompletedProperty, value); }
        }
        public static readonly DependencyProperty IsCompletedProperty =
            DependencyProperty.Register("IsCompleted", typeof(bool), typeof(TaskItem), new PropertyMetadata(false, IsCompletedChanged));
        private static void IsCompletedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TaskItem item = d as TaskItem;
            if (item != null)
            {
                item.UpdateVisualState((bool)e.NewValue ? CompletedState : UncompletedState);
            }
        }

        public double SwipeGestureTreshold
        {
            get { return (double)GetValue(SwipeGestureTresholdProperty); }
            set { SetValue(SwipeGestureTresholdProperty, value); }
        }
        public static readonly DependencyProperty SwipeGestureTresholdProperty =
            DependencyProperty.Register("SwipeGestureTreshold", typeof(double), typeof(TaskItem), new PropertyMetadata(105d));
        #endregion // end of Dependency Properties

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }
            storage = value;

            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion // end of INotifyPropertyChanged

        #region Visual States
        private const string CompleteStatesGroup = "CompleteStates";
        private const string CompletedState = "Completed";
        private const string UncompletedState = "Uncompleted";

        private const string GestureStatesGroup = "GestureStates";
        private const string GestureStartDragState = "GestureStartDrag";
        private const string GestureDragOkState = "GestureDragOk";
        private const string GestureDragState = "GestureDrag";
        private const string GestureEndDragState = "GestureEndDrag";

        private const string DeleteStatesGroup = "DeleteStates";
        private const string DeletedState = "Deleted";
        private const string NotDeletedState = "NotDeleted";

        private const string SubtasksStatesGroup = "SubtasksStates";
        private const string ShowSubtasksState = "ShowSubtasks";
        private const string HideSubtasksState = "HideSubtasks";

        private void UpdateVisualState(string state, bool useTransitions = true)
        {
            VisualStateManager.GoToState(this, state, _loaded && useTransitions);
        }

        private void UpdateVisualStates(bool useTransitions = true)
        {
            UpdateVisualState((Task != null && Task.IsCompleted) ? CompletedState : UncompletedState, useTransitions);
            UpdateVisualState((Task != null && Task.ShowSubtasks) ? ShowSubtasksState : HideSubtasksState, useTransitions);
            UpdateVisualState(GestureEndDragState, useTransitions);
        }
        #endregion // end of Visual States

        #region Constructor
        public TaskItem()
        {
            InitializeComponent();
            ResetDelete();
            ShowSubtasksBorder.Visibility = Visibility.Collapsed;
        }

        private bool _loaded = false;

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_loaded)
            {
                ShowSubtasksBorder.Visibility = Visibility.Visible;
                UpdateVisualStates(false);
            }
            _loaded = true;
        }
        #endregion // end of Constructor

        #region Event Handlers
        private void InfoGrid_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            UpdateVisualState(GestureStartDragState);

            SwipeLeftGestureIcon.Style = GestureActionHelper.IconStyle(Settings.Current.SwipeLeftAction);
            SwipeRightGestureIcon.Style = GestureActionHelper.IconStyle(Settings.Current.SwipeRightAction);
            SwipeLeftGestureIcon.Visibility = Visibility.Visible;
            SwipeRightGestureIcon.Visibility = Visibility.Visible;
            SwipeLeftGestureIcon.Opacity = 0;
            SwipeRightGestureIcon.Opacity = 0;
            BackgroundBorder.Opacity = 0;
            RootTransform.X = 0;
        }

        private void InfoGrid_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            double value = RootTransform.X;
            if (Math.Abs(value) > SwipeGestureTreshold)
            {
                if (value < 0)
                {
                    // Swipe Left
                    OnSwipeLeft();
                }
                else
                {
                    // Swipe Right
                    OnSwipeRight();
                }
            }
            UpdateVisualState(GestureEndDragState);
        }

        private void GestureEndStoryboard_Completed(object sender, EventArgs e)
        {
            SwipeLeftGestureIcon.Visibility = Visibility.Collapsed;
            SwipeRightGestureIcon.Visibility = Visibility.Collapsed;
        }

        private void InfoGrid_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            RootTransform.X += e.DeltaManipulation.Translation.X;
            if (Settings.Current.SwipeLeftAction == GestureAction.None && RootTransform.X < 0)
            {
                RootTransform.X = 0;
            }
            else if (Settings.Current.SwipeRightAction == GestureAction.None && RootTransform.X > 0)
            {
                RootTransform.X = 0;
            }

            double value = Math.Abs(RootTransform.X);
            if (value > SwipeGestureTreshold)
            {
                UpdateVisualState(GestureDragOkState);
            }
            else
            {
                UpdateVisualState(GestureDragState);
            }

            double opacity = Math.Min(value / SwipeGestureTreshold, 1.0);
            SwipeLeftGestureIcon.Opacity = RootTransform.X < 0 ? opacity : 0;
            SwipeRightGestureIcon.Opacity = RootTransform.X > 0 ? opacity : 0;
        }

        private void Checkbox_Tap(object sender, GestureEventArgs e)
        {
            OnCheck();
        }

        private void Task_Tap(object sender, GestureEventArgs e)
        {
            OnClick();
        }

        private void SubtaskAdd_Click(object sender, RoutedEventArgs e)
        {
            OnClick(new Subtask());
        }

        private void SubtaskItem_Click(object sender, SubtaskEventArgs e)
        {
            OnClick(e.Subtask);
        }

        private void SubtaskItem_Check(object sender, SubtaskEventArgs e)
        {
            OnCheck(e.Subtask);
        }

        private void SubtaskItem_SwipeLeft(object sender, SubtaskEventArgs e)
        {
            if (OnSwipeLeft(e.Subtask))
            {
                e.DeleteFrom = Task;
            }
        }

        private void SubtaskItem_SwipeRight(object sender, SubtaskEventArgs e)
        {
            if (OnSwipeRight(e.Subtask))
            {
                e.DeleteFrom = Task;
            }
        }

        private void ShowSubtasksButton_Checked(object sender, RoutedEventArgs e)
        {
            UpdateVisualState(ShowSubtasksState);
        }

        private void ShowSubtasksButton_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateVisualState(HideSubtasksState);
        }
        #endregion // end of Event Handlers

        #region Delete
        public void ResetDelete()
        {
            RootBorder.Height = double.NaN;
            UpdateVisualState(NotDeletedState, false);
        }
        #endregion // end of Delete

        private void TaskPriorityGrid_Loaded(object sender, RoutedEventArgs e)
        {
            Grid grid = (Grid)sender;
            if (Settings.Current.ShowTaskCheckBox)
            {
                grid.Margin = new Thickness(-62, 0, 0, 0);
            }
            else
            {
                grid.Margin = new Thickness(0);
            }
        }
    }
}
