using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MinerPlugin.Interfaces
{
    public interface IInstalablePlugin
    {
        Task<bool> Install();
        Task<bool> Uninstall();
    }
}
