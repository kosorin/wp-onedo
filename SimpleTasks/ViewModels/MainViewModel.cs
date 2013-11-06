﻿using Microsoft.Phone.Scheduler;
using SimpleTasks.Core.Helpers;
using SimpleTasks.Core.Models;
using SimpleTasks.Models;
using System;
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
        public TaskKeyGroupCollection GroupedTasks
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
            Tasks = TaskCollection.LoadFromXmlFile(TaskCollection.DefaultDataFileName);
            if (Tasks == null)
            {
                Tasks = new TaskCollection();
            }
            IsDataLoaded = true;

            if (App.Settings.DeleteCompletedTasksSetting)
            {
                DeleteOldCompletedTasks(App.Settings.DeleteCompletedTasksDaysSetting);
            }

#if DEBUG
            Debug.WriteLine("> Úkoly ({0}):", Tasks.Count);
            foreach (TaskModel task in Tasks)
            {
                Debug.WriteLine("  {0}", task.Title);
            }

            Debug.WriteLine("> Připomenutí:");
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
            TaskReminder.Add(task);
            LiveTile.Update(Tasks);
        }

        public void UpdateTask(TaskModel oldTask, TaskModel newTask)
        {
            if (oldTask == null || newTask == null)
                throw new ArgumentNullException();

            Tasks.Remove(oldTask);
            Tasks.Add(newTask);
            TaskReminder.Add(newTask);
            LiveTile.Update(Tasks);
        }

        public void RemoveTask(TaskModel task)
        {
            if (task == null)
                throw new ArgumentNullException();

            Tasks.Remove(task);
            TaskReminder.Remove(task);
            LiveTile.Update(Tasks);
        }

        public void DeleteOldCompletedTasks(int days)
        {
            if (IsDataLoaded)
            {
                // Odstranění dokončených úkolů starších více jak days dnů
                var updatedTasks = Tasks.Where((t) =>
                {
                    if (t.IsComplete)
                    {
                        if (t.CompletedDate != null)
                        {
                            return (DateTime.Today.Date - t.CompletedDate.Value.Date < TimeSpan.FromDays(days));
                        }
                    }
                    return true;
                });
                Tasks = new TaskCollection(updatedTasks);
            }
        }

        public void DeleteCompletedTasks()
        {
            if (IsDataLoaded)
            {
                // Odstranění dokončených úkolů
                Tasks = new TaskCollection(Tasks.Where((t) => { return t.IsActive; }));
            }
        }
    }
}
