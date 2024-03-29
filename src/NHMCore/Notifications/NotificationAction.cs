﻿using System;

namespace NHMCore.Notifications
{
    public class NotificationAction
    {
        public ActionID ActionID { get; set; }
        public string Info { get; internal set; }
        public Action Action { get; internal set; }
        public bool IsSingleShotAction { get; internal set; } = false;
        public bool BindProgress { get; internal set; } = false;
        public IProgress<int> Progress { get; set; } = new Progress<int>();
        public string Extra { get; set; } = string.Empty;
    }
}
