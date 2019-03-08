using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NiceHashMiner.Forms
{
    public static class FormHelpers
    {

        public static void TranslateFormControls(Control c)
        {
            try
            {
                c.Text = Translations.Tr(c.Text);
            }
            catch(Exception e)
            {
            }
            
            // call on all controls
            foreach (Control childC in c.Controls)
            {
                TranslateFormControls(childC);
            }
        }

    }
}
