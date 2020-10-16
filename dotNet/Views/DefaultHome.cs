using System;
using System.Web;
using System.Windows.Forms;

namespace Atomus.Control.Home
{
    public partial class DefaultHome : UserControl, IAction
    {
        private AtomusControlEventHandler beforeActionEventHandler;
        private AtomusControlEventHandler afterActionEventHandler;
        
        #region Init
        public DefaultHome()
        {
            InitializeComponent();
        }
        #endregion

        #region Dictionary
        #endregion

        #region Spread
        #endregion

        #region IO
        object IAction.ControlAction(ICore sender, AtomusControlArgs e)
        {
            try
            {
                this.beforeActionEventHandler?.Invoke(this, e);

                if (e.Action.StartsWith("Button"))
                    this.ExecuteSSO(e.Action);

                switch (e.Action)
                {
                    default:
                        return null;// throw new AtomusException("'{0}'은 처리할 수 없는 Action 입니다.".Translate(e.Action));
                }
            }
            finally
            {
                this.afterActionEventHandler?.Invoke(this, e);
            }
        }
        #endregion

        #region Event
        event AtomusControlEventHandler IAction.BeforeActionEventHandler
        {
            add
            {
                this.beforeActionEventHandler += value;
            }
            remove
            {
                this.beforeActionEventHandler -= value;
            }
        }
        event AtomusControlEventHandler IAction.AfterActionEventHandler
        {
            add
            {
                this.afterActionEventHandler += value;
            }
            remove
            {
                this.afterActionEventHandler -= value;
            }
        }

        private void DefaultHome_Load(object sender, EventArgs e)
        {
            string namespaceName;
            string[] controls;

            try
            {
                controls = this.GetAttribute("Controls").Split(',');

                foreach (string control in controls)
                {
                    namespaceName = this.GetAttribute(string.Format("Controls.{0}.Namespace", control));

                    if (namespaceName.Equals("System.Windows.Forms.SplitContainer"))
                        this.AddSplitContainer(control);
                    else
                        this.AddAtomusUserControl(control);
                }

                //AtomusControlEventArgs e1;
                //AtomusControlEventArgs e2;

                //e2 = new AtomusControlEventArgs("Search", null);

                ////Action, new object[] { _MENU_ID, _ASSEMBLY_ID, AtomusControlEventArgs, addTabControl }
                //e1 = new AtomusControlEventArgs("Menu.GetControl", new object[] { 8M, 2M, e2, false });

                //this.afterActionEventHandler?.Invoke(this, e1);

                //if (e1.Value is System.Windows.Forms.Control)
                //{
                //    ((System.Windows.Forms.Control)e1.Value).Dock = DockStyle.Bottom;

                //    this.Controls.Add(((System.Windows.Forms.Control)e1.Value));
                //}
            }
            catch (Exception exception)
            {
                this.MessageBoxShow(this, exception);
            }
        }

        private void Menu_BeforeActionEventHandler(ICore sender, AtomusControlEventArgs e) { }
        private void Menu_AfterActionEventHandler(ICore sender, AtomusControlEventArgs e)
        {
            this.afterActionEventHandler?.Invoke(this, e);
        }

        private void Home_BeforeActionEventHandler(ICore sender, AtomusControlEventArgs e) { }
        private void Home_AfterActionEventHandler(ICore sender, AtomusControlEventArgs e) { }
        #endregion

        #region "ETC"
        private void AddSplitContainer(string controlName)
        {
            SplitContainer splitContainer;
            System.Windows.Forms.Control control;
            IAction action;

            splitContainer = new SplitContainer();
            splitContainer.DoubleBuffered(true);
            splitContainer.Name = controlName;

            try
            {
                this.SetDock(splitContainer, this.GetAttribute(string.Format("Controls.{0}.Dock", controlName)));
            }
            catch (Exception exception)
            {
                this.MessageBoxShow(this, exception);
            }

            try
            {
                if (this.GetAttribute(string.Format("Controls.{0}.Orientation", controlName)).Equals("Vertical"))
                    splitContainer.Orientation = Orientation.Vertical;
                else
                    splitContainer.Orientation = Orientation.Horizontal;

                this.Controls.Add(splitContainer);
            }
            catch (Exception exception)
            {
                this.MessageBoxShow(this, exception);
            }

            try
            {
                if (splitContainer.Orientation == Orientation.Vertical)
                    splitContainer.SplitterDistance = splitContainer.Width / this.GetAttribute(string.Format("Controls.{0}.SplitterDistance", controlName)).ToInt();
                else
                    splitContainer.SplitterDistance = splitContainer.Height / this.GetAttribute(string.Format("Controls.{0}.SplitterDistance", controlName)).ToInt();

                splitContainer.BringToFront();
            }
            catch (Exception exception)
            {
                this.MessageBoxShow(this, exception);
            }

            try
            {
                control = (System.Windows.Forms.Control)Factory.CreateInstance(this.GetAttribute(string.Format("Controls.{0}.Panel1", controlName)), false, true);
                //control = new Menu.DefaultMenu();
                action = (IAction)control;
                action.BeforeActionEventHandler += Menu_BeforeActionEventHandler;
                action.AfterActionEventHandler += Menu_AfterActionEventHandler;

                control.Dock = DockStyle.Fill;
                splitContainer.Panel1.Controls.Add(control);
            }
            catch (Exception exception)
            {
                this.MessageBoxShow(this, exception);
            }

            try
            {
                control = (System.Windows.Forms.Control)Factory.CreateInstance(this.GetAttribute(string.Format("Controls.{0}.Panel2", controlName)), false, true);
                action = (IAction)control;
                action.BeforeActionEventHandler += Home_BeforeActionEventHandler;
                action.AfterActionEventHandler += Home_AfterActionEventHandler;

                control.Dock = DockStyle.Fill;
                splitContainer.Panel2.Controls.Add(control);
            }
            catch (Exception exception)
            {
                this.MessageBoxShow(this, exception);
            }
        }

        private void AddAtomusUserControl(string controlName)
        {
            System.Windows.Forms.Control control;
            IAction action;

            control = null;

            if (this.GetAttribute(string.Format("Controls.{0}.Namespace", controlName)).Equals("Menu.GetControl"))
            {
                AtomusControlEventArgs e;

                //Action, new object[] { _MENU_ID, _ASSEMBLY_ID, AtomusControlEventArgs, addTabControl }
                e = new AtomusControlEventArgs("Menu.GetControl", new object[] { this.GetAttributeDecimal(string.Format("Controls.{0}.MenuID", controlName))
                                                                                , this.GetAttributeDecimal(string.Format("Controls.{0}.AssemblyID", controlName))
                                                                                , null, false });

                this.afterActionEventHandler?.Invoke(this, e);

                if (e.Value is System.Windows.Forms.Control)
                    control = (System.Windows.Forms.Control)e.Value;
            }
            else
                control = (System.Windows.Forms.Control)Factory.CreateInstance(this.GetAttribute(string.Format("Controls.{0}.Namespace", controlName)));

            control.Name = controlName;

            if (controlName.Contains("Menu"))
            {
                action = (IAction)control;
                action.BeforeActionEventHandler += Menu_BeforeActionEventHandler;
                action.AfterActionEventHandler += Menu_AfterActionEventHandler;
            }
            else//if (controlName.Contains("Home"))
            {
                action = (IAction)control;
                action.BeforeActionEventHandler += Home_BeforeActionEventHandler;
                action.AfterActionEventHandler += Home_AfterActionEventHandler;
            }

            control.Size = this.GetAttributeSize(string.Format("Controls.{0}.Size", controlName));

            this.SetDock(control, this.GetAttribute(string.Format("Controls.{0}.Dock", controlName)));

            this.Controls.Add(control);
            control.BringToFront();
        }

        private void SetDock(System.Windows.Forms.Control control, string dock)
        {
            if (dock == null)
                return;

            control.Dock = (DockStyle)Enum.Parse(typeof(DockStyle), dock);
        }


        private void ExecuteSSO(string action)
        {
            if (this.GetAttribute(string.Format("{0}.ActionType", action)) != null && this.GetAttribute(string.Format("{0}.ActionType", action)) == "SSO")
            {
                string tmp1;
                string tmp2;
                string timeKey;

                while (true)
                {
                    timeKey = DateTime.Now.ToString("yyyyMMddhhmmssfff");

                    tmp1 = HttpUtility.UrlEncode(this.Encrypt(Config.Client.GetAttribute("Account.EMAIL").ToString(), timeKey));
                    tmp2 = HttpUtility.UrlDecode(tmp1);

                    if (tmp1 == tmp2)
                        break;
                }

                tmp1 = string.Format(this.GetAttribute(string.Format("{0}.ActionValue", action)), tmp1, "", timeKey);

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(tmp1));
            }
        }
        private string Encrypt(string cipher, string type)
        {
            string EncryptionKey;
            byte[] cipherBytes;

            EncryptionKey = string.Format(this.GetAttribute("EncryptKey"), type);

            cipherBytes = System.Text.Encoding.Unicode.GetBytes(cipher);

            using (System.Security.Cryptography.Rijndael encryptor = System.Security.Cryptography.Rijndael.Create())
            {
                System.Security.Cryptography.Rfc2898DeriveBytes pdb = new System.Security.Cryptography.Rfc2898DeriveBytes(EncryptionKey, Convert.FromBase64String(this.GetAttribute("EncryptSalt")));

                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);

                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    using (System.Security.Cryptography.CryptoStream cs = new System.Security.Cryptography.CryptoStream(ms, encryptor.CreateEncryptor(), System.Security.Cryptography.CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }

                    cipher = Convert.ToBase64String(ms.ToArray());
                }
            }

            return cipher;
        }
        #endregion
    }
}
