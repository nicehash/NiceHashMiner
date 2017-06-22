using System;
using System.Collections.Generic;
using System.Text;
using MyDownloader.Core.Extensions;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using MyDownloader.Core;
using MyDownloader.Core.UI;
using System.Security.Permissions;
using System.ComponentModel;

//http://radsoftware.com.au/articles/clipboardmonitor.aspx 
namespace MyDownloader.Extension.WindowsIntegration.ClipboardMonitor
{
    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
    public class ClipboardMonitor : IDisposable
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern bool ChangeClipboardChain(
            IntPtr hWndRemove,  // handle to window to remove
            IntPtr hWndNewNext  // handle to next window
            );

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        private const int WM_DRAWCLIPBOARD = 0x0308;
        private const int WM_CHANGECBCHAIN = 0x030D;

        private IntPtr handle;
        private IntPtr clipboardViewerNext;
        private IClipboardDataHandler dataHandler;
        private bool enabled;

        #region Methods 

        public bool FilterMessage(ref Message m)
        {
            if (!enabled)
            {
                return false;
            }

            switch (m.Msg)
            {
                //
                // The WM_DRAWCLIPBOARD message is sent to the first window 
                // in the clipboard viewer chain when the content of the 
                // clipboard changes. This enables a clipboard viewer 
                // window to display the new content of the clipboard. 
                //
                case WM_DRAWCLIPBOARD:

                    Debug.WriteLine("WindowProc DRAWCLIPBOARD: " + m.Msg, "WndProc");

                    if (dataHandler != null)
                    {
                        try
                        {
                            IDataObject iData = Clipboard.GetDataObject();

                            dataHandler.HandleClipboardData(iData);
                        }
                        catch
                        {
                        }
                    }

                    //
                    // Each window that receives the WM_DRAWCLIPBOARD message 
                    // must call the SendMessage function to pass the message 
                    // on to the next window in the clipboard viewer chain.
                    //
                    SendMessage(clipboardViewerNext, m.Msg, m.WParam, m.LParam);
                    return true;


                //
                // The WM_CHANGECBCHAIN message is sent to the first window 
                // in the clipboard viewer chain when a window is being 
                // removed from the chain. 
                //
                case WM_CHANGECBCHAIN:
                    Debug.WriteLine("WM_CHANGECBCHAIN: lParam: " + m.LParam, "WndProc");

                    // When a clipboard viewer window receives the WM_CHANGECBCHAIN message, 
                    // it should call the SendMessage function to pass the message to the 
                    // next window in the chain, unless the next window is the window 
                    // being removed. In this case, the clipboard viewer should save 
                    // the handle specified by the lParam parameter as the next window in the chain. 

                    //
                    // wParam is the Handle to the window being removed from 
                    // the clipboard viewer chain 
                    // lParam is the Handle to the next window in the chain 
                    // following the window being removed. 
                    if (m.WParam == clipboardViewerNext)
                    {
                        //
                        // If wParam is the next clipboard viewer then it
                        // is being removed so update pointer to the next
                        // window in the clipboard chain
                        //
                        clipboardViewerNext = m.LParam;
                    }
                    else
                    {
                        SendMessage(clipboardViewerNext, m.Msg, m.WParam, m.LParam);
                    }
                    return true;

                default:
                    return false;
            }
        }
        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            ChangeClipboardChain(handle, clipboardViewerNext);
            handle = IntPtr.Zero;
            clipboardViewerNext = IntPtr.Zero;
            dataHandler = null;
        }

        #endregion

        #region Properties

        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                Settings.Default.MonitorClipboard = value;
                Settings.Default.Save();
            }
        } 

        #endregion

        public ClipboardMonitor(IClipboardDataHandler handler, IntPtr winHandle)
        {
            enabled = Settings.Default.MonitorClipboard;

            Settings.Default.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                enabled = Settings.Default.MonitorClipboard;
            };

            handle = winHandle;
            clipboardViewerNext = SetClipboardViewer(handle);

            this.dataHandler = handler;
        } 
    }
}
