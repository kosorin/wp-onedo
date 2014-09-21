﻿using GoogleAnalytics;
using GoogleAnalytics.Core;
using SimpleTasks.Controls;
using SimpleTasks.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTasks.Helpers
{
    public enum CustomDimension
    {
        TaskRelativeDate = 1,
        TaskTime = 2,
        TaskReminder = 3,
        TaskTileLineHeight = 4,
        Feedback = 5,
    }

    public static class GoogleAnalyticsHelper
    {
        private static bool _showDebugMessages = true;
        public static bool ShowDebugMessages
        {
            get { return _showDebugMessages; }
            set { _showDebugMessages = value; }
        }

        private static Tracker _tracker = null;
        private static Tracker Tracker
        {
            get
            {
                if (_tracker == null)
                {
                    _tracker = EasyTracker.GetTracker();
                }
                return _tracker;
            }
        }

        public static void SendPage(BasePage page)
        {
            if (!Settings.Current.General.Feedback)
                return;

            try
            {
                if (page != null)
                {
                    string pageName = page.GetType().Name;

                    if (ShowDebugMessages)
                        Debug.WriteLine("> GA SendPage: {0}", pageName);

                    Tracker.SendView(pageName);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("> GoogleAnalyticsHelper SendPage exception: {0}", e.Message);
            }
        }

        public static void SendEvent(string category, string action, string label = "<Label>", long value = 0)
        {
            if (!Settings.Current.General.Feedback)
                return;

            try
            {
                if (ShowDebugMessages)
                    Debug.WriteLine("> GA SendEvent: {0} > {1} > '{2}' ({3})", category, action, label, value);

                Tracker.SendEvent(category, action, label, value);
            }
            catch (Exception e)
            {
                Debug.WriteLine("> GoogleAnalyticsHelper SendEvent exception: {0}", e.Message);
            }
        }

        public static void SetDimension(CustomDimension type, string value)
        {
            if (!Settings.Current.General.Feedback)
                return;

            try
            {
                int index = (int)type;
                if (index >= 1)
                {
                    if (ShowDebugMessages)
                        Debug.WriteLine("> GA SetDimension: {0} ({1})", type, value);

                    Tracker.SetCustomDimension(index, value);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("> GoogleAnalyticsHelper SendPage exception: {0}", e.Message);
            }
        }
    }
}