﻿using Microsoft.Phone.Scheduler;
using SimpleTasks.Core.Helpers;
using SimpleTasks.Helpers;
using SimpleTasks.Core.Models;
using SimpleTasks.Models;
using SimpleTasks.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SimpleTasks.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private TaskCollection _tasks = new TaskCollection();
        public TaskCollection Tasks
        {
            get
            {
                return _tasks;
            }
            private set
            {
                SetProperty(ref _tasks, value);
                OnPropertyChanged(GroupedTasksPropertyString);

                if (_tasks != null)
                {
                    _tasks.CollectionChanged += (s, e) => { OnPropertyChanged(GroupedTasksPropertyString); };
                }
            }
        }

        private const string GroupedTasksPropertyString = "GroupedTasks";
        public List<TaskKeyGroup> GroupedTasks
        {
            get
            {
                return TaskKeyGroup.CreateGroups(Tasks);
            }
        }

        public MainViewModel()
        {
            IsDataLoaded = false;
        }

        public bool IsDataLoaded { get; private set; }

        public void LoadTasks()
        {
            if (App.Settings.CurrentDataFileVersion != App.Settings.DataFileVersionSetting)
            {
                Tasks = TaskCollection.ConvertOldDataFile(TaskCollection.DefaultDataFileName);
                App.Settings.DataFileVersionSetting = App.Settings.CurrentDataFileVersion;
            }
            else
            {
                Tasks = TaskCollection.LoadFromXmlFile(TaskCollection.DefaultDataFileName);
            }

            if (Tasks == null)
            {
                Tasks = new TaskCollection();
            }
            IsDataLoaded = true;

            if (App.Settings.DeleteCompletedTasksSetting)
            {
                DeleteCompletedTasks(App.Settings.DeleteCompletedTasksDaysSetting);
            }

#if DEBUG
            Debug.WriteLine("> Nahrané úkoly ({0}):", Tasks.Count);
            foreach (TaskModel task in Tasks)
            {
                Debug.WriteLine(": {0}", task.TitleFirstLine);
            }

            Debug.WriteLine("> Nahrané připomenutí ({0}):", Tasks.Count((t) => {
                ScheduledAction reminder = ScheduledActionService.Find(t.Uid);
                if (reminder != null)
                {
                    return true;

                } return false;
            }));
            foreach (TaskModel task in Tasks)
            {
                ScheduledAction reminder = ScheduledActionService.Find(task.Uid);
                if (reminder != null)
                {
                    Debug.WriteLine("  [{0}] {1}", reminder.Name, task.Title);
                }
            }
#endif
        }

        public void SaveTasks()
        {
            Tasks.SaveToXmlFile(TaskCollection.DefaultDataFileName);
        }

        public void AddTask(TaskModel task)
        {
            if (task == null)
                throw new ArgumentNullException();

            Tasks.Add(task);
            if (task.ReminderDate != null)
            {
                ReminderHelper.Add(task.Uid, 
                                   AppResources.ReminderTitle, 
                                   task.Title, 
                                   task.ReminderDate.Value,
                                   new Uri(string.Format("/Views/EditTaskPage.xaml?Task={0}", task.Uid), UriKind.Relative));
            }
            //LiveTile.UpdateOrReset(App.Settings.EnableLiveTileSetting, Tasks);
        }

        public void UpdateTask(TaskModel oldTask, TaskModel newTask)
        {
            if (oldTask == null || newTask == null)
                throw new ArgumentNullException();

            ReminderHelper.Remove(oldTask.Uid);
            Tasks.Remove(oldTask);
            Tasks.Add(newTask);
            if (newTask.ReminderDate != null)
            {
                ReminderHelper.Add(newTask.Uid, 
                                   AppResources.ReminderTitle,
                                   newTask.Title, 
                                   newTask.ReminderDate.Value, 
                                   new Uri(string.Format("/Views/EditTaskPage.xaml?Task={0}", newTask.Uid), UriKind.Relative));
            }
            //LiveTile.UpdateOrReset(App.Settings.EnableLiveTileSetting, Tasks);
        }

        public void RemoveTask(TaskModel task, bool updateLiveTile = true)
        {
            if (task == null)
                throw new ArgumentNullException();

            Tasks.Remove(task);
            ReminderHelper.Remove(task.Uid);

            if (updateLiveTile)
            {
                //LiveTile.UpdateOrReset(App.Settings.EnableLiveTileSetting, Tasks);
            }
        }

        public void DeleteCompletedTasks(int days)
        {
            if (IsDataLoaded)
            {
                // Odstranění úkolů, které byly odstarněny před více jak 'days' dny.
                var completedTasks = Tasks.Where((t) =>
                {
                    if (t.IsComplete)
                    {
                        return (DateTime.Now - t.CompletedDate.Value) >= TimeSpan.FromDays(days);
                    }
                    else
                    {
                        return false; 
                    }
                }).ToList();
                foreach (TaskModel task in completedTasks)
                {
                    RemoveTask(task, false);
                }
                //LiveTile.UpdateOrReset(App.Settings.EnableLiveTileSetting, Tasks);
            }
        }

        public void DeleteCompletedTasks()
        {
            if (IsDataLoaded)
            {
                // Odstranění dokončených úkolů
                var completedTasks = Tasks.Where((t) => { return t.IsComplete; }).ToList();
                foreach (TaskModel task in completedTasks)
                {
                    RemoveTask(task, false);
                }
                //LiveTile.UpdateOrReset(App.Settings.EnableLiveTileSetting, Tasks);
                SaveTasks();
            }
        }

        public void DeleteAllTasks()
        {
            if (IsDataLoaded)
            {
                foreach (TaskModel task in Tasks)
                {
                    ReminderHelper.Remove(task.Uid);
                }
                Tasks.Clear();
            }
        }
    }
}
