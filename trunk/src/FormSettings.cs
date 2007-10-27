/*
BalloonRSS - Simple RSS news aggregator using balloon tooltips
    Copyright (C) 2007  Roman Morawek <romor@users.sourceforge.net>

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace BalloonRss
{
    class FormSettings : Form
    {
        private const int panelWidth = 450;
        private Label fillLabel;


        public FormSettings()
        {
            this.SuspendLayout();

            InitializeComponent();
            FillData();

            this.ResumeLayout();
        }


        private void InitializeComponent()
        {
            Panel panel;
            Button button;

            // the main container panel
            FlowLayoutPanel flPanelMain = new FlowLayoutPanel();
            flPanelMain.FlowDirection = FlowDirection.TopDown;
            flPanelMain.AutoSize = true;
            flPanelMain.Dock = DockStyle.Fill;

            // add a dummy element to resize the settings to full horizontal scale
            fillLabel = new Label();
            fillLabel.Size = new System.Drawing.Size(panelWidth - 5, 0);
            flPanelMain.Controls.Add(fillLabel);

            // setting
            panel = CreateSettingControl(Properties.Settings.Default.displayIntervall, resources.str_settingsDisplayIntervall);
            flPanelMain.Controls.Add(panel);
            panel = CreateSettingControl(Properties.Settings.Default.retrieveIntervall, resources.str_settingsRetrieveIntervall);
            flPanelMain.Controls.Add(panel);
            panel = CreateSettingControl(Properties.Settings.Default.balloonTimespan, resources.str_settingsBalloonTimespan);
            flPanelMain.Controls.Add(panel);
            panel = CreateSettingControl(Properties.Settings.Default.channelConfigFileName, resources.str_settingsChannelConfigFilename);
            flPanelMain.Controls.Add(panel);
            panel = CreateSettingControl(Properties.Settings.Default.channelAsTitle, resources.str_settingsChannelAsTitle);
            flPanelMain.Controls.Add(panel);
            panel = CreateSettingControl(Properties.Settings.Default.historyDepth, resources.str_settingsHistoryDepth);
            flPanelMain.Controls.Add(panel);

            // OK/Cancel button panel
            FlowLayoutPanel flPanel = new FlowLayoutPanel();
            flPanel.FlowDirection = FlowDirection.LeftToRight;
            flPanel.AutoSize = true;
            flPanel.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            button = new Button();
            button.Text = resources.str_settingsFormOKButton;
            button.Click += new System.EventHandler(this.OnOK);
            this.AcceptButton = button;
            flPanel.Controls.Add(button);
            button = new Button();
            button.Text = resources.str_settingsFormCancelButton;
            button.Click += new System.EventHandler(this.OnCancel);
            this.CancelButton = button;
            flPanel.Controls.Add(button);
            flPanelMain.Controls.Add(flPanel);

            this.ClientSize = new System.Drawing.Size(panelWidth, 250);
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.Text = resources.str_settingsFormTitle;
            this.Icon = BalloonRss.resources.ico_yellow32;
            this.Controls.Add(flPanelMain);
            this.Resize += new System.EventHandler(this.OnResize);
        }


        private Panel CreateSettingControl(Object settingsObject, String labelText)
        {
            // create the label
            Label label = new Label();
            label.Text = labelText + ":";
            label.AutoSize = true;
            label.Anchor = System.Windows.Forms.AnchorStyles.Left;
            //label.Dock = DockStyle.Left;

            // create the settings control
            Control control;
            if (settingsObject.GetType() == typeof(int))
            {
                control = new NumericTextBox();
                control.Text = "" + settingsObject;
            }
            else if (settingsObject.GetType() == typeof(string))
            {
                control = new TextBox();
                control.Text = "" + settingsObject;
            }
            else if (settingsObject.GetType() == typeof(bool))
            {
                control = new CheckBox();
                ((CheckBox)control).Checked = (bool)settingsObject;
            }
            else
                throw new Exception("Internal error: Illegal settings data type");

            //control.Anchor = AnchorStyles.Right;
            control.Dock = DockStyle.Right;
            control.AutoSize = true;
            int x = Math.Max((control.Height - label.Height) / 2, 0);
            label.Location = new System.Drawing.Point(0, x);

            // create the panel
            Panel flPanel = new Panel();
            int height = Math.Max(label.Height, control.Height);
            flPanel.Size = new System.Drawing.Size(panelWidth-5, height);
            //flPanel.FlowDirection = FlowDirection.LeftToRight;
            //flPanel.AutoSize = true;
            //flPanel.Dock = DockStyle.Fill;
            flPanel.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            flPanel.Controls.Add(label);
            flPanel.Controls.Add(control);

            return flPanel;
        }


        private void FillData()
        {
        }


        private void OnResize(object sender, EventArgs e)
        {
            fillLabel.Size = new System.Drawing.Size(this.ClientSize.Width-5, 0);
        }

        private void OnOK(object sender, EventArgs e)
        {
            // close window
            Dispose();
        }

        private void OnCancel(object sender, EventArgs e)
        {
            // close window
            Dispose();
        }
    }
}