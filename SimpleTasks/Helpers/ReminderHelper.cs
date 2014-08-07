﻿using Microsoft.Phone.Shell;
using Microsoft.Phone.Scheduler;
using SimpleTasks.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTasks.Helpers
{
    public class ReminderHelper
    {
        public static void Add(string name, string title, string content, DateTime beginTime, Uri navigationUri)
        {
            Remove(name);
            if (beginTime <= DateTime.Now)
            {
                Debug.WriteLine("> Reminder Add: overdue: {0} <= {1}", beginTime, DateTime.Now);
                return;
            }

            try
            {
                ScheduledActionService.Add(new Reminder(name)
                {
                    BeginTime = beginTime,
                    Title = title,
                    Content = content,
                    NavigationUri = navigationUri,
                });
            }
            catch (InvalidOperationException e)
            {
                Debug.WriteLine("Chyba při přidání připomínky: {0}", e.Message);
            }
            catch (SchedulerServiceException e)
            {
                Debug.WriteLine("Chyba při přidání připomínky: {0}", e.Message);
            }
        }

        public static Reminder Get(string name)
        {
            return (Reminder)ScheduledActionService.Find(name);
        }

        public static bool Exists(string name)
        {
            return ScheduledActionService.Find(name) != null;
        }

        public static void Remove(string name)
        {
            if (Exists(name))
            {
                ScheduledActionService.Remove(name);
            }
        }
    }
}
