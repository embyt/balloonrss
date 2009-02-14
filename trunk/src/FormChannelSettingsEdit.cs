/*
BalloonRSS - Simple RSS news aggregator using balloon tooltips
    Copyright (C) 2009  Roman Morawek <romor@users.sourceforge.net>

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
using System.Windows.Forms;
using BalloonRss.Properties;


namespace BalloonRss
{
    class FormChannelSettingsEdit : Form
    {
        // GUI elements
        private Label fillLabel;
        private Control cntlUrl;
        private Control cntlPriority;
        private Control cntlMarkAsRead;

        // class data
        private ChannelInfo channelInfo;


        public FormChannelSettingsEdit(ChannelInfo channelInfo)
        {
            this.channelInfo = channelInfo;

            // create panel
            InitializeComponent();
        }


        private void InitializeComponent()
        {
            this.SuspendLayout();

            // the main container panel
            FlowLayoutPanel flPanelMain = new FlowLayoutPanel();
            flPanelMain.FlowDirection = FlowDirection.TopDown;
            flPanelMain.AutoSize = true;
            //flPanelMain.Dock = DockStyle.Fill;
            flPanelMain.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

            // setting controls
            Panel panel;
            int maxXSize = 0;
            cntlUrl = CreateSettingControl(channelInfo.link, Resources.str_channelSettingsHeaderTitle, out panel);
            maxXSize = Math.Max(maxXSize, panel.Width);
            flPanelMain.Controls.Add(panel);
            cntlPriority = CreateSettingControl(channelInfo.priority, Resources.str_channelSettingsHeaderPriority, out panel, 0, Byte.MaxValue);
            maxXSize = Math.Max(maxXSize, panel.Width);
            flPanelMain.Controls.Add(panel);
            cntlMarkAsRead = CreateSettingControl(channelInfo.markAsReadAtStartup, Resources.str_channelSettingsHeaderMarkAsRead, out panel);
            maxXSize = Math.Max(maxXSize, panel.Width);
            flPanelMain.Controls.Add(panel);

            // OK/Cancel button panel
            FlowLayoutPanel flPanel = new FlowLayoutPanel();
            flPanel.FlowDirection = FlowDirection.LeftToRight;
            Button okButton = new Button();
            okButton.Text = Resources.str_settingsFormOKButton;
            okButton.Click += new EventHandler(this.OnOK);
            this.AcceptButton = okButton;
            flPanel.Controls.Add(okButton);
            Button cancelButton = new Button();
            cancelButton.Text = Resources.str_settingsFormCancelButton;
            cancelButton.Click += new EventHandler(this.OnCancel);
            this.CancelButton = cancelButton;
            flPanel.Controls.Add(cancelButton);
            flPanel.AutoSize = true;
            flPanel.Anchor = AnchorStyles.Right;
            flPanelMain.Controls.Add(flPanel);

            // add a dummy element to resize the settings to full horizontal scale
            fillLabel = new Label();
            fillLabel.Size = new System.Drawing.Size(maxXSize, 0);
            flPanelMain.Controls.Add(fillLabel);

            // dialog settings
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.Text = Resources.str_channelSettingsEditFormTitle;
            this.Icon = Resources.ico_yellow32;
            this.Controls.Add(flPanelMain);
            this.Resize += new EventHandler(this.OnResize);
            this.HelpButton = true;
            this.HelpRequested += new HelpEventHandler(FormChannelSettingsEdit_HelpRequested);

            this.ResumeLayout();

            // now, we can resize it
            this.ClientSize = new System.Drawing.Size(maxXSize, flPanelMain.Height);
            this.MinimumSize = this.Size;
        }

        void FormChannelSettingsEdit_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            Help.ShowHelp(this, Settings.Default.helpFilename, HelpNavigator.Topic, Settings.Default.helpNameChannelSettingsEdit);
        }


        private Control CreateSettingControl(Object settingsObject, String labelText, out Panel panel)
        {
            return CreateSettingControl(settingsObject, labelText, out panel, 0, 0);
        }

        private Control CreateSettingControl(Object settingsObject, String labelText, out Panel panel, int minValue, int maxValue)
        {
            // create the label
            Label label = new Label();
            label.Text = labelText + ":";
            label.Anchor = System.Windows.Forms.AnchorStyles.Left;
            label.Size = new System.Drawing.Size(
                TextRenderer.MeasureText(label.Text, label.Font).Width,
                TextRenderer.MeasureText(label.Text, label.Font).Height);

            // create the settings control
            Control control;
            if ( (settingsObject.GetType() == typeof(int)) || (settingsObject.GetType() == typeof(byte)) )
            {
                control = new NumericTextBox(minValue, maxValue, labelText);
                control.Width = TextRenderer.MeasureText(maxValue.ToString(), control.Font).Width;
                control.Text = "" + settingsObject;
            }
            else if (settingsObject.GetType() == typeof(String))
            {
                control = new TextBox();
                control.Text = "" + settingsObject;
                control.Width = Math.Max(
                    TextRenderer.MeasureText(control.Text, control.Font).Width,
                    TextRenderer.MeasureText(Settings.Default.channelSettingsLinkTextfieldSize, control.Font).Width);
                (control as TextBox).TextAlign = HorizontalAlignment.Left;
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

            // create the panel
            panel = new Panel();
            label.Location = new System.Drawing.Point(0, Math.Max((control.Height - label.Height) / 2, 0));
            panel.Size = new System.Drawing.Size(label.Width + control.Width + 5, Math.Max(label.Height, control.Height));
            panel.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            panel.Controls.Add(label);
            panel.Controls.Add(control);

            return control;
        }


        private bool CheckData(out String errorMessage)
        {
            errorMessage = null;

            // check link format
            if (!ChannelInfo.IsValidLink(cntlUrl.Text))
            {
                errorMessage = Resources.str_channelSettingsIllegalLink + cntlUrl.Text;
                return false;
            }

            // check data ranges
            if (!(cntlPriority as NumericTextBox).IsValueValid())
            {
                errorMessage = (cntlPriority as NumericTextBox).GetErrorMessage();
                return false;
            }
            
            return true;
        }


        private void GetData()
        {
            // store the data in the associated channel info
            // the data were already checked for validy before
            channelInfo.link = cntlUrl.Text;
            channelInfo.priority = (byte)(cntlPriority as NumericTextBox).IntValue;
            channelInfo.markAsReadAtStartup = (cntlMarkAsRead as CheckBox).Checked;
        }


        private void OnResize(object sender, EventArgs e)
        {
            fillLabel.Size = new System.Drawing.Size(this.ClientSize.Width-5, 0);
        }

        private void OnOK(object sender, EventArgs e)
        {
            String errorMessage;

            // close window
            if (CheckData(out errorMessage) == false)
            {
                MessageBox.Show(
                    errorMessage,
                    Resources.str_settingsFormErrorHeader,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            else
            {
                GetData();
                this.DialogResult = DialogResult.OK;
                Dispose();
            }
        }

        private void OnCancel(object sender, EventArgs e)
        {
            // close window
            this.DialogResult = DialogResult.Cancel;
            Dispose();
        }
    }
}