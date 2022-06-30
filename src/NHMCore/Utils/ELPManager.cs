using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Utils
{
    public delegate void NotifyELPChangeEventHandler(object sender, EventArgs e);
    public class ELPManager
    {
        private ELPManager() { }
        public static ELPManager Instance { get; } = new ELPManager();
        public event NotifyELPChangeEventHandler ELPReiteration;
        protected virtual void OnChanged(EventArgs e)
        {
            if (ELPReiteration != null) ELPReiteration(this, e);
        }
        public void NotifyELPReiteration()
        {
            OnChanged(EventArgs.Empty);
        }
    }
}
