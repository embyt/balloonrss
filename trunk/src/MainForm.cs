using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;


namespace BalloonRss
{
    public class MainForm : System.Windows.Forms.Form
    {
        public int balloonTimespan = 500;   // ms

        private NotifyIcon notifyIcon;
        private IContainer components;

        private Retriever retriever;


        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainForm rssForm = new MainForm();
            Application.Run(rssForm);
            rssForm.Hide();
        }

        public MainForm()
        {
            InitializeComponent();

            this.components = new System.ComponentModel.Container();
            ContextMenu contextMenu = new ContextMenu();

            // menuItem exit
            MenuItem mi_exit = new System.Windows.Forms.MenuItem();
            mi_exit.Text = "E&xit";
            mi_exit.Click += new System.EventHandler(this.mi_exit_Click);

            // menuItem settings
            MenuItem mi_settings = new System.Windows.Forms.MenuItem();
            mi_settings.Text = "&Settings";
            mi_settings.Click += new System.EventHandler(this.mi_settings_Click);

            // menuItem toolTip
            MenuItem mi_toolTip = new System.Windows.Forms.MenuItem();
            mi_toolTip.Text = "&Tool Tip";
            mi_toolTip.Click += new System.EventHandler(this.mi_tip_Click);

            // Initialize contextMenu
            contextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] 
            { 
                mi_toolTip,
                mi_settings,
                mi_exit,
            });

            // Set up how the form should be displayed.
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Text = "BalloonRss Settings";

            // Create the NotifyIcon.
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);

            // The Icon property sets the icon that will appear
            // in the systray for this application.
            notifyIcon.Icon = BalloonRss.resources.appicon;

            // The ContextMenu property sets the menu that will
            // appear when the systray icon is right clicked.
            notifyIcon.ContextMenu = contextMenu;

            // The Text property sets the text that will be displayed,
            // in a tooltip, when the mouse hovers over the systray icon.
            notifyIcon.Text = "BalloonRss initialized";
            notifyIcon.Visible = true;

            // hide the panel
            this.Visible = false;
            this.WindowState = FormWindowState.Minimized;

            // setup and start the background worker
            retriever = new Retriever();
            retriever.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.RetrieverCompleted);
            retriever.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.RetrieverProgressChanged);
            retriever.RunWorkerAsync();
        }


        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // RssForm
            // 
            this.ClientSize = new System.Drawing.Size(292, 267);
            this.Name = "RssForm";
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);

        }
 
        protected override void Dispose(bool disposing)
        {
            // Clean up any components being used.
            if (disposing)
                if (components != null)
                    components.Dispose();

            base.Dispose(disposing);
        }


        private void mi_settings_Click(object sender, EventArgs e)
        {
            // cancel background worker operation
            retriever.CancelAsync();

            // Show the form

            // Set the WindowState to normal if the form is minimized.
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal;

            // Activate the form.
            this.Activate();
        }

        private void mi_exit_Click(object sender, EventArgs e)
        {
            // Close the form, which closes the application.
            this.Close();
        }

        private void mi_tip_Click(object sender, EventArgs e)
        {
            notifyIcon.ShowBalloonTip(400, "Hallo meine Anna", "Ich mag dich sooo.", ToolTipIcon.None);
        }


        private void RetrieverCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            notifyIcon.ShowBalloonTip(400, "Status Update", "Worker completed.", ToolTipIcon.Info);
        }

        private void RetrieverProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case Retriever.PROGRESS_NEWRSS:
                    RssItem rssItem = e.UserState as RssItem;
                    notifyIcon.ShowBalloonTip(balloonTimespan, rssItem.title, rssItem.description, ToolTipIcon.None);
                    break;

                case Retriever.PROGRESS_ERROR:
                    notifyIcon.ShowBalloonTip(balloonTimespan, "Error message", e.UserState as string, ToolTipIcon.Error);
                    break;
            }
        }
   }
}