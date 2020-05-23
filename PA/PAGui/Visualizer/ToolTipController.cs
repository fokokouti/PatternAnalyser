﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace PAGui.Visualizer
{
    /// <summary>
    /// Helps displaying tooltips over different areas of the same FrameworkElement
    /// </summary>
    static class ToolTipController
    {
        public static void Move(ToolTipContentProviderDelegate provider, object tag)
        {
            if (provider != current_provider || tag != current_tag)
            {
                timer.Stop();
                current_provider = provider;
                current_tag = tag;
                if (tip.IsOpen) Hide();
                timer.Start();
            }
            else if (tip.IsOpen)
            {
                Vector delta = Mouse.GetPosition(null) - initial_position;
                tip.VerticalOffset = delta.Y;
                tip.HorizontalOffset = delta.X;
            }
        }

        public static void Hide()
        {
            timer.Stop();
            tip.IsOpen = false;
        }

        static ToolTipController()
        {
            timer.Interval = new TimeSpan(ToolTipService.GetInitialShowDelay(tip) * 10000);
            timer.Tick += new EventHandler(timer_Tick);
        }

        static void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            OnShowToolTip();
        }

        static void OnShowToolTip()
        {
            if (current_provider != null)
            {
                tip.VerticalOffset = 0;
                tip.HorizontalOffset = 0;
                tip.Content = current_provider(current_tag);
                tip.IsOpen = tip.Content != null;

                initial_position = Mouse.GetPosition(null);
            }
        }

        private static ToolTipContentProviderDelegate current_provider;
        private static object current_tag;
        private static ToolTip tip = new ToolTip();
        private static Point initial_position;
        private static DispatcherTimer timer = new DispatcherTimer();
    }

    /// <summary>
    /// returns the content of a ToolTip displayed for object tag
    /// </summary>
    public delegate object ToolTipContentProviderDelegate(object tag);
}
