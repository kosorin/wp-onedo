﻿using SimpleTasks.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows.Media;

namespace SimpleTasks.Core.Models
{
    [DataContract(Name = "Task", Namespace = "")]
    public class TaskModel : BindableBase
    {
        public TaskModel()
        {
            Uid = Guid.NewGuid().ToString();
            Created = DateTime.Now;
        }

        #region Wrapper
        private object _wrapper = null;
        public object Wrapper
        {
            get { return _wrapper; }
            set { SetProperty(ref _wrapper, value); }
        }
        #endregion


        #region Uid 0
        private string _uid = "";
        [DataMember(Order = 0)]
        public string Uid
        {
            get { return _uid; }
            private set
            {
                SetProperty(ref _uid, value);
            }
        }
        #endregion


        #region Title 1
        private string _title = "";
        [DataMember(Order = 1)]
        public string Title
        {
            get { return _title; }
            set
            {
                if (SetProperty(ref _title, value))
                {
                    Modified = DateTime.Now;
                }
            }
        }
        #endregion

        #region Detail 2
        private string _detail = "";
        [DataMember(Order = 2)]
        public string Detail
        {
            get
            {
                return _detail;
            }
            set
            {
                if (SetProperty(ref _detail, value))
                {
                    Modified = DateTime.Now;
                }
            }
        }
        #endregion

        #region Subtasks 3
        private ObservableCollection<Subtask> _subtasks = new ObservableCollection<Subtask>();
        [DataMember(Order = 3)]
        public ObservableCollection<Subtask> Subtasks
        {
            get { return _subtasks; }
            set
            {
                if (SetProperty(ref _subtasks, value))
                {
                    OnPropertyChanged("HasSubtasks");
                    value.CollectionChanged -= Subtasks_CollectionChanged;
                    value.CollectionChanged += Subtasks_CollectionChanged;
                    Modified = DateTime.Now;
                }
            }
        }

        public bool HasSubtasks
        {
            get { return Subtasks.Count > 0; }
        }

        private void Subtasks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Modified = DateTime.Now;
            OnPropertyChanged("HasSubtasks");
        }
        #endregion


        #region DueDate 10
        private DateTime? _dueDate = null;
        [DataMember(Order = 10)]
        public DateTime? DueDate
        {
            get
            {
                return _dueDate;
            }
            set
            {
                if (SetProperty(ref _dueDate, value))
                {
                    OnPropertyChanged("CurrentDueDate");
                    OnPropertyChanged("HasDueDate");
                    OnPropertyChanged("IsOverdue");
                    OnPropertyChanged("Reminder");
                    OnPropertyChanged("ReminderDate");
                    OnPropertyChanged("ReminderDates");
                    OnPropertyChanged("HasReminder");
                    OnPropertyChanged("Repeats");
                    OnPropertyChanged("HasRepeats");
                    Modified = DateTime.Now;
                }
            }
        }

        public DateTime? CurrentDueDate
        {
            get
            {
                if (Repeats != Models.Repeats.None)
                {
                    if (HasDueDate)
                    {
                        const int dayCount = 7;
                        List<DayOfWeek> list = new List<DayOfWeek>();
                        if ((Repeats & Repeats.Monday) != 0) list.Add(DayOfWeek.Monday);
                        if ((Repeats & Repeats.Tuesday) != 0) list.Add(DayOfWeek.Tuesday);
                        if ((Repeats & Repeats.Wednesday) != 0) list.Add(DayOfWeek.Wednesday);
                        if ((Repeats & Repeats.Thursday) != 0) list.Add(DayOfWeek.Thursday);
                        if ((Repeats & Repeats.Friday) != 0) list.Add(DayOfWeek.Friday);
                        if ((Repeats & Repeats.Saturday) != 0) list.Add(DayOfWeek.Saturday);
                        if ((Repeats & Repeats.Sunday) != 0) list.Add(DayOfWeek.Sunday);

                        DateTime startDate = (DateTime.Today < DueDate.Value.Date) ? DueDate.Value.Date : DateTime.Today;
                        for (int i = 0; i < dayCount; i++)
                        {
                            DateTime date = startDate.AddDays(i);
                            if (list.Contains(date.DayOfWeek))
                            {
                                return date.SetTime(DueDate.Value);
                            }
                        }
                    }
                    return null;
                }
                else
                {
                    return DueDate;
                }
            }
        }

        public bool HasDueDate
        {
            get { return DueDate != null; }
        }

        public bool IsOverdue
        {
            get { return DueDate != null && DueDate.Value.Date < DateTime.Today; }
        }
        #endregion

        #region Reminder 11
        private TimeSpan? _reminder = null;
        [DataMember(Order = 11)]
        public TimeSpan? Reminder
        {
            get { return _reminder; }
            set
            {
                if (SetProperty(ref _reminder, value))
                {
                    OnPropertyChanged("HasReminder");
                    Modified = DateTime.Now;
                }
            }
        }

        public DateTime ReminderDate
        {
            get
            {
                if (!HasReminder)
                {
                    throw new InvalidOperationException("Pro získání datumu připomenutí je nutné zadat termín splnění.");
                }
                return CurrentDueDate.Value - Reminder.Value;
            }
        }

        public List<DateTime> ReminderDates
        {
            get
            {
                if (!HasReminder)
                {
                    throw new InvalidOperationException("Pro získání datumu připomenutí je nutné zadat termín splnění.");
                }

                List<DateTime> dates = new List<DateTime>();
                if (Repeats != Models.Repeats.None)
                {
                    const int dayCount = 7;
                    List<DayOfWeek> list = new List<DayOfWeek>();
                    if ((Repeats & Repeats.Monday) != 0) list.Add(DayOfWeek.Monday);
                    if ((Repeats & Repeats.Tuesday) != 0) list.Add(DayOfWeek.Tuesday);
                    if ((Repeats & Repeats.Wednesday) != 0) list.Add(DayOfWeek.Wednesday);
                    if ((Repeats & Repeats.Thursday) != 0) list.Add(DayOfWeek.Thursday);
                    if ((Repeats & Repeats.Friday) != 0) list.Add(DayOfWeek.Friday);
                    if ((Repeats & Repeats.Saturday) != 0) list.Add(DayOfWeek.Saturday);
                    if ((Repeats & Repeats.Sunday) != 0) list.Add(DayOfWeek.Sunday);

                    for (int i = 0; i < dayCount; i++)
                    {
                        DateTime date = DueDate.Value.AddDays(i);
                        if (list.Contains(date.DayOfWeek))
                        {
                            dates.Add(date - Reminder.Value);
                        }
                    }
                }
                else
                {
                    dates.Add(ReminderDate);
                }

                return dates;
            }
        }

        public bool HasReminder
        {
            get { return DueDate != null && Reminder != null; }
        }
        #endregion

        #region Repeats 12
        private Repeats _repeats = Repeats.None;
        [DataMember(Order = 12)]
        public Repeats Repeats
        {
            get { return _repeats; }
            set
            {
                if (SetProperty(ref _repeats, value))
                {
                    OnPropertyChanged("CurrentDueDate");
                    OnPropertyChanged("HasRepeats");
                    Modified = DateTime.Now;
                }
            }
        }

        public bool HasRepeats
        {
            get { return Repeats != Models.Repeats.None; }
        }

        #endregion // end of Repeats 12

        #region Priority 20
        private TaskPriority _priority = TaskPriority.Normal;
        [DataMember(Order = 20)]
        public TaskPriority Priority
        {
            get
            {
                return _priority;
            }
            set
            {
                if (SetProperty(ref _priority, value))
                {
                    OnPropertyChanged("IsLowPriority");
                    OnPropertyChanged("IsNormalPriority");
                    OnPropertyChanged("IsHighPriority");
                    Modified = DateTime.Now;
                }
            }
        }

        public bool IsLowPriority
        {
            get { return Priority == TaskPriority.Low; }
        }

        public bool IsNormalPriority
        {
            get { return Priority == TaskPriority.Normal; }
        }

        public bool IsHighPriority
        {
            get { return Priority == TaskPriority.High; }
        }
        #endregion


        #region TileSettings 40
        private TaskTileSettings _tileSettings = null;
        [DataMember(Order = 40)]
        public TaskTileSettings TileSettings
        {
            get { return _tileSettings; }
            set { SetProperty(ref _tileSettings, value); }
        }
        #endregion


        //#region Color 50
        //public static readonly Color DefaultColor = Colors.Transparent;

        //private Color _color = DefaultColor;
        //[DataMember(Order = 50)]
        //public Color Color
        //{
        //    get { return _color; }
        //    set { SetProperty(ref _color, value); }
        //}

        //public bool HasColor
        //{
        //    get { return Color != DefaultColor; }
        //}
        //#endregion


        #region Created 100
        private DateTime? _created = null;
        [DataMember(Order = 100)]
        public DateTime? Created
        {
            get { return _created; }
            private set { SetProperty(ref _created, value); }
        }
        #endregion

        #region Modified 101
        private DateTime? _modified = null;
        [DataMember(Order = 101)]
        public DateTime? Modified
        {
            get { return _modified; }
            private set { SetProperty(ref _modified, value); }
        }

        private bool _modifiedSinceStart = false;
        public bool ModifiedSinceStart
        {
            get { return _modifiedSinceStart; }
            set { _modifiedSinceStart = value; }
        }
        #endregion

        #region Completed 102
        private DateTime? _completed = null;
        [DataMember(Order = 102)]
        public DateTime? Completed
        {
            get { return _completed; }
            set
            {
                if (SetProperty(ref _completed, value))
                {
                    OnPropertyChanged("IsCompleted");
                    OnPropertyChanged("IsActive");
                    Modified = DateTime.Now;
                }
            }
        }

        public bool IsCompleted { get { return Completed.HasValue; } }

        public bool IsActive { get { return !Completed.HasValue; } }
        #endregion
    }
}
