﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using SimpleTasks.Core.Models;

namespace SimpleTasks.Core.Tiles
{
    public partial class WideTaskTile : TaskTileControl
    {
        public WideTaskTile()
        {
            InitializeComponent();
        }

        public WideTaskTile(TaskModel task)
            : base(task)
        {
            InitializeComponent();
        }

        public override void Refresh()
        {
            TaskModel task = Task;
            if (task == null)
                return;

            DataContext = task;
            TaskTileSettings settings = task.TileSettings ?? Settings.Current.DefaultTaskTileSettings;

            // Název
            if (settings.ShowTitle)
            {
                TitleWrapper.Visibility = Visibility.Visible;
                Title.FontSize = settings.LineHeight * 0.72;
                Title.LineHeight = settings.LineHeight;
                Title.Text = task.Title;
                Title.TextWrapping = settings.TitleOnOneLine ? TextWrapping.NoWrap : TextWrapping.Wrap;
            }
            else
            {
                TitleWrapper.Visibility = Visibility.Collapsed;
            }

            // Date
            bool showDate = settings.ShowDate && task.HasDueDate;
            InfoWrapper.Visibility = showDate ? Visibility.Visible : Visibility.Collapsed;
            if (showDate)
            {
                Info.Height = settings.LineHeight;
                Date.Text = task.ActualDueDate.Value.ToShortDateString();
            }

            // Podúkoly + Detail
            Subtasks1.Children.Clear();
            Subtasks2.Children.Clear();
            ShowSecondColumn();
            List<Subtask> subtasks = new List<Subtask>(task.Subtasks.Where(s => settings.ShowCompletedSubtasks || !s.IsCompleted));
            int maxColumnItems = (int)Math.Round(336 / settings.LineHeight);
            if (settings.ShowTitle)
                maxColumnItems--;
            if (showDate)
                maxColumnItems--;
            bool subtaksOnTwoColumns = subtasks.Count > maxColumnItems;
            bool showSubtasks = subtasks.Count > 0;

            if (showSubtasks)
            {
                if (!subtaksOnTwoColumns && string.IsNullOrWhiteSpace(task.Detail))
                {
                    HideSecondColumn();
                }

                Subtasks1.Visibility = Visibility.Visible;
                foreach (Subtask subtask in subtasks.Take(maxColumnItems))
                {
                    SubtaskControl sc = new SubtaskControl(subtask);
                    sc.Refresh(settings.LineHeight);
                    Subtasks1.Children.Add(sc);
                }

                Subtasks2.Visibility = subtaksOnTwoColumns ? Visibility.Visible : Visibility.Collapsed;
                if (subtaksOnTwoColumns)
                {
                    foreach (Subtask subtask in subtasks.Skip(maxColumnItems).Take(maxColumnItems))
                    {
                        SubtaskControl sc = new SubtaskControl(subtask);
                        sc.Refresh(settings.LineHeight);
                        Subtasks2.Children.Add(sc);
                    }
                }

                Detail2.FontSize = settings.LineHeight * 0.65;
                Detail2.Text = task.Detail;
                Detail1.Visibility = Visibility.Collapsed;
            }
            else
            {
                Subtasks1.Visibility = Visibility.Collapsed;
                Subtasks2.Visibility = Visibility.Collapsed;

                Detail1.Visibility = Visibility.Visible;
                Detail1.FontSize = settings.LineHeight * 0.65;
                Detail1.Text = task.Detail;
                HideSecondColumn();
            }

            // Pozadí
            LayoutRoot.Background = new SolidColorBrush(Colors.Transparent);
            //LayoutRoot.Background = new SolidColorBrush(task.Color) { Opacity = settings.BackgroundOpacity };
        }

        private void ShowSecondColumn()
        {
            FirstColumn.Width = new GridLength(336);
            SecondColumn.Visibility = Visibility.Visible;
        }

        private void HideSecondColumn()
        {
            FirstColumn.Width = new GridLength(691);
            SecondColumn.Visibility = Visibility.Collapsed;
        }
    }
}
