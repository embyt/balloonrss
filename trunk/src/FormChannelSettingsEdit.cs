/*
BalloonRSS - Simple RSS news aggregator using balloon tooltips
    Copyright (C) 2008  Roman Morawek <romor@users.sourceforge.net>

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
using BalloonRss.Properties;


namespace BalloonRss
{
    class FormChannelSettingsEdit : Form
    {
        // GUI elements
        private Label fillLabel;
        private Control cntlUrl;
        private Control cntlPriority;
        private Button ctrlClearChannelData;

        // class data
        private ChannelInfo channelInfo;
        private bool channelDataCleared = false;


        public FormChannelSettingsEdit(ChannelInfo channelInfo)
        {
            this.channelInfo = channelInfo;

            // create panel
            InitializeComponent();
        }


        private void InitializeComponent()
        {
            Panel panel;
            Button button;

            this.SuspendLayout();

            // the main container panel
            FlowLayoutPanel flPanelMain = new FlowLayoutPanel();
            flPanelMain.FlowDirection = FlowDirection.TopDown;
            flPanelMain.AutoSize = true;
            //flPanelMain.Dock = DockStyle.Fill;
            flPanelMain.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

            // setting controls
            int maxXSize = 0;
            cntlUrl = CreateSettingControl(channelInfo.link, Resources.str_channelSettingsHeaderTitle, out panel);
            maxXSize = Math.Max(maxXSize, panel.Width);
            flPanelMain.Controls.Add(panel);
            cntlPriority = CreateSettingControl(channelInfo.priority, Resources.str_channelSettingsHeaderPriority, out panel, 0, Byte.MaxValue);
            maxXSize = Math.Max(maxXSize, panel.Width);
            flPanelMain.Controls.Add(panel);

            // clear data button
            ctrlClearChannelData = new Button();
            ctrlClearChannelData.Text = Resources.str_channelSettingsEditClearChannelData;
            ctrlClearChannelData.Width = TextRenderer.MeasureText(ctrlClearChannelData.Text, ctrlClearChannelData.Font).Width+10;
            ctrlClearChannelData.Click += new EventHandler(this.OnClearChannelData);
            maxXSize = Math.Max(maxXSize, ctrlClearChannelData.Width);
            flPanelMain.Controls.Add(ctrlClearChannelData);

            // OK/Cancel button panel
            FlowLayoutPanel flPanel = new FlowLayoutPanel();
            flPanel.FlowDirection = FlowDirection.LeftToRight;
            button = new Button();
            button.Text = Resources.str_settingsFormOKButton;
            button.Click += new EventHandler(this.OnOK);
            this.AcceptButton = button;
            flPanel.Controls.Add(button);
            button = new Button();
            button.Text = Resources.str_settingsFormCancelButton;
            button.Click += new EventHandler(this.OnCancel);
            this.CancelButton = button;
            flPanel.Controls.Add(button);
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

            this.ResumeLayout();

            // now, we can resize it
            this.ClientSize = new System.Drawing.Size(maxXSize, flPanelMain.Height);
            this.MinimumSize = this.Size;
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
            // get data
            channelInfo.link = cntlUrl.Text;
            channelInfo.priority = (byte)(cntlPriority as NumericTextBox).IntValue;
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
                // we use the dialog result as an indication whether we cleared the channel data
                if (channelDataCleared)
                    this.DialogResult = DialogResult.Yes;
                else
                    this.DialogResult = DialogResult.OK;
                Dispose();
            }
        }

        private void OnCancel(object sender, EventArgs e)
        {
            // close window
            // we use the dialog result as an indication whether we cleared the channel data
            if (channelDataCleared)
                this.DialogResult = DialogResult.No;
            else
                this.DialogResult = DialogResult.Cancel;
            Dispose();
        }

        private void OnClearChannelData(object sender, EventArgs e)
        {
            // clear the file system data
            RssChannel.ClearChannelData(channelInfo);
            channelDataCleared = true;

            // show confirmation dialog
            MessageBox.Show(
                this,
                Resources.str_channelSettingsEditClearChannelDataConfirm,
                Resources.str_channelSettingsEditClearChannelData,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            // update channel
        }
    }
}