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

        public void SetOutputLimit(double max_fan_speed)
        {
            PID_CONTROLLER.nhm_set_output_limits(max_fan_speed);
        }

        public void SetOutputLimits(double min_speed, double max_speed)
        {
            PID_CONTROLLER.nhm_set_output_limits(min_speed, max_speed);
        }

        public void SetPid(double p, double i, double d)
        {
            PID_CONTROLLER.nhm_set_pid(p, i, d);
        }
    }
}
