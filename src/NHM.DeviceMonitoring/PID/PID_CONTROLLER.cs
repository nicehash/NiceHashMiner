using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NHM.DeviceMonitoring.PID
{
    internal class PID_CONTROLLER
    {
        const string dll = "pid_controller.dll";

        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_pid_init();
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int nhm_pid_deinit();
        [DllImport(dll, CallingConvention = CallingConvention.StdCall)]
        public static extern double nhm_pid_get_output(double actual_temp, double setpoint);

    }
}
