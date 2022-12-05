using NHM.DeviceMonitoring.PID;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.DeviceMonitoring
{
    public class PidController
    {
        public double GetOutput(double temp, double setpoint)
        {
            var fan_speed = 0.0;
            fan_speed = PID_CONTROLLER.nhm_pid_get_output(temp, setpoint);
            return fan_speed;
        }
    }
}
