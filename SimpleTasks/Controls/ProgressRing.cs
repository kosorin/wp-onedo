﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SimpleTasks.Controls
{
    public class ProgressRing : Control
    {
        bool hasAppliedTemplate = false;

        public ProgressRing()
        {
            this.DefaultStyleKey = typeof(ProgressRing);
            TemplateSettings = new TemplateSettingValues(60);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            hasAppliedTemplate = true;
            UpdateState(this.IsActive);
        }

        void UpdateState(bool isActive)
        {
            if (hasAppliedTemplate)
            {
                string state = isActive ? "Active" : "Inactive";
                System.Windows.VisualStateManager.GoToState(this, state, true);
            }
        }

        protected override System.Windows.Size MeasureOverride(System.Windows.Size availableSize)
        {
            var width = 100D;
            if (!System.ComponentModel.DesignerProperties.IsInDesignTool)
                width = this.Width != double.NaN ? this.Width : availableSize.Width;
            TemplateSettings = new TemplateSettingValues(width);
            return base.MeasureOverride(availableSize);
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(ProgressRing), new PropertyMetadata(""));

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(ProgressRing), new PropertyMetadata(false, new PropertyChangedCallback(IsActiveChanged)));
        private static void IsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var pr = (ProgressRing)d;
            var isActive = (bool)args.NewValue;
            pr.UpdateState(isActive);
        }

        public TemplateSettingValues TemplateSettings
        {
            get { return (TemplateSettingValues)GetValue(TemplateSettingsProperty); }
            set { SetValue(TemplateSettingsProperty, value); }
        }
        public static readonly DependencyProperty TemplateSettingsProperty =
            DependencyProperty.Register("TemplateSettings", typeof(TemplateSettingValues), typeof(ProgressRing), new PropertyMetadata(new TemplateSettingValues(100)));


        public class TemplateSettingValues : System.Windows.DependencyObject
        {
            public TemplateSettingValues(double width)
            {
                MaxSideLength = 400;
                EllipseDiameter = width / 10D;
                EllipseOffset = new System.Windows.Thickness(EllipseDiameter);
            }

            public double MaxSideLength
            {
                get { return (double)GetValue(MaxSideLengthProperty); }
                set { SetValue(MaxSideLengthProperty, value); }
            }
            public static readonly DependencyProperty MaxSideLengthProperty =
                DependencyProperty.Register("MaxSideLength", typeof(double), typeof(TemplateSettingValues), new PropertyMetadata(0D));

            public double EllipseDiameter
            {
                get { return (double)GetValue(EllipseDiameterProperty); }
                set { SetValue(EllipseDiameterProperty, value); }
            }
            public static readonly DependencyProperty EllipseDiameterProperty =
                DependencyProperty.Register("EllipseDiameter", typeof(double), typeof(TemplateSettingValues), new PropertyMetadata(0D));

            public Thickness EllipseOffset
            {
                get { return (Thickness)GetValue(EllipseOffsetProperty); }
                set { SetValue(EllipseOffsetProperty, value); }
            }
            public static readonly DependencyProperty EllipseOffsetProperty =
                DependencyProperty.Register("EllipseOffset", typeof(Thickness), typeof(TemplateSettingValues), new PropertyMetadata(new Thickness()));
        }
    }
}
