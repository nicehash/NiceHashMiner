using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NiceHashMiner.Forms
{
    public partial class LoginForm : Form
    {
        public string Btc { get; private set; } = "";

        public LoginForm()
        {
            InitializeComponent();
            CenterToScreen();
            Icon = Properties.Resources.logo;
            webBrowser1.Navigate(Links.Login);
            webBrowser1.DocumentCompleted += WebBrowser1_DocumentCompleted;
            //webBrowser1.SizeChanged += WebBrowser1_SizeChanged;
            FormHelpers.TranslateFormControls(this);
        }

        private void WebBrowser1_SizeChanged(object sender, EventArgs e)
        {
            if (webBrowser1.Document == null) return;
            this.Size = webBrowser1.Document.Body.ScrollRectangle.Size; // new Size(600, 400); // webBrowser1.Size;
        }

        private void WebBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            Size = new Size(600, 400);
            webBrowser1.Size = new Size(600, 400); // webBrowser1.Document.Body.ScrollRectangle.Size;
            var center = new Point(webBrowser1.Document.Body.ScrollRectangle.Size.Width / 2, webBrowser1.Document.Body.ScrollRectangle.Size.Height / 2);
            webBrowser1.Document.Body.ScrollTop = center.Y;

            if (webBrowser1.DocumentText.Contains("<PRE>back</PRE>"))
            {
                this.Close();
                return;
            }

            string btc = BtcParser.FindBtcInText(webBrowser1.DocumentText);
            Console.WriteLine($"btc is {btc}");
            if (btc != "")
            {
                Btc = btc;
                this.Close();
            }
        }

        internal static class BtcParser
        {
            const string btcAlphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

            public static string FindBtcInText(string text)
            {
                if (string.IsNullOrEmpty(text) || text.Length < 26) return ""; // nope

                // try to find "btc:" prefix
                const string btcPrefix = "btc:";
                var startIndex = text.IndexOf(btcPrefix);
                if (startIndex > -1)
                {
                    var subText = text.Substring(startIndex);
                    return FindBtcInText2(subText);
                }
                return "";
            }

            // this one is bulletproof, takes 2s for 1MB string
            public static string FindBtcInText2(string text)
            {
                // 2 is for testnet btc others are 1, 3 and bc1
                const string btcStartAlphabet = "123bc";
                const int min = 26;
                const int max = 35;
                if (string.IsNullOrEmpty(text) || text.Length < min) return ""; // nope


                for (int i = 0; i < text.Length - min; i++)
                {
                    char startC = text[i];
                    if (!btcStartAlphabet.Contains(startC)) continue;
                    var left = text.Length - i;
                    var subText = text.Substring(i, left > max ? max : left);
                    var btc = "";
                    foreach (char c in subText)
                    {
                        if (!btcAlphabet.Contains(c) || btc.Length > max) break;
                        btc += c;
                        if (btc.Length > min && CredentialValidators.ValidateBitcoinAddress(btc))
                        {
                            return btc;
                        }
                    }
                }

                return "";
            }
        }
    }
}
