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


namespace BalloonRss
{
    class FormSettings : Form
    {
        private const String textFieldSize = "###########";

        private Label fillLabel;
        private Control cntlDisplayIntervall;
        private Control cntlRetrieveIntervall;
        private Control cntlBalloonTimespan;
        private Control cntlChannelAsTitle;
        private Control cntlHistoryDepth;
        private Control cntlClickInfluence;


        public FormSettings()
        {
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

            // setting
            int maxXSize = 0;
            cntlDisplayIntervall = CreateSettingControl(Properties.Settings.Default.displayIntervall, Properties.Resources.str_settingsDisplayIntervall, out panel, 10, Int32.MaxValue);
            maxXSize = Math.Max(maxXSize, panel.Width);
            flPanelMain.Controls.Add(panel);
            cntlRetrieveIntervall = CreateSettingControl(Properties.Settings.Default.retrieveIntervall, Properties.Resources.str_settingsRetrieveIntervall, out panel, 15, Int32.MaxValue);
            maxXSize = Math.Max(maxXSize, panel.Width);
            flPanelMain.Controls.Add(panel);
            cntlBalloonTimespan = CreateSettingControl(Properties.Settings.Default.balloonTimespan, Properties.Resources.str_settingsBalloonTimespan, out panel, 10, Int32.MaxValue);
            maxXSize = Math.Max(maxXSize, panel.Width);
            flPanelMain.Controls.Add(panel);
            cntlChannelAsTitle = CreateSettingControl(Properties.Settings.Default.channelAsTitle, Properties.Resources.str_settingsChannelAsTitle, out panel);
            maxXSize = Math.Max(maxXSize, panel.Width);
            flPanelMain.Controls.Add(panel);
            cntlHistoryDepth = CreateSettingControl(Properties.Settings.Default.historyDepth, Properties.Resources.str_settingsHistoryDepth, out panel, 0, Int32.MaxValue);
            maxXSize = Math.Max(maxXSize, panel.Width);
            flPanelMain.Controls.Add(panel);
            cntlClickInfluence = CreateSettingControl(Properties.Settings.Default.clickInfluence, Properties.Resources.str_settingsClickInfluence, out panel, 0, 10);
            maxXSize = Math.Max(maxXSize, panel.Width);
            flPanelMain.Controls.Add(panel);

            // OK/Cancel button panel
            FlowLayoutPanel flPanel = new FlowLayoutPanel();
            flPanel.FlowDirection = FlowDirection.LeftToRight;
            button = new Button();
            button.Text = Properties.Resources.str_settingsFormOKButton;
            button.Click += new System.EventHandler(this.OnOK);
            this.AcceptButton = button;
            flPanel.Controls.Add(button);
            button = new Button();
            button.Text = Properties.Resources.str_settingsFormCancelButton;
            button.Click += new System.EventHandler(this.OnCancel);
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
            this.Text = Properties.Resources.str_settingsFormTitle;
            this.Icon = BalloonRss.Properties.Resources.ico_yellow32;
            this.Controls.Add(flPanelMain);
            this.Resize += new System.EventHandler(this.OnResize);

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
            if (settingsObject.GetType() == typeof(int))
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
                    TextRenderer.MeasureText(textFieldSize, control.Font).Width);
                (control as TextBox).TextAlign = HorizontalAlignment.Right;
            }
            else if (settingsObject.GetType() == typeof(bool))
            {
                control = new CheckBox();
                ((CheckBox)control).Checked = (bool)settingsObject;
            }
            else if (settingsObject.GetType() == typeof(byte))
            {
                control = new TrackBar();
                ((TrackBar)control).Minimum = minValue;
                ((TrackBar)control).Maximum = maxValue;
                ((TrackBar)control).Value = (byte)settingsObject;
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
            if (!(cntlBalloonTimespan as NumericTextBox).IsValueValid())
            {
                errorMessage = (cntlBalloonTimespan as NumericTextBox).GetErrorMessage();
                return false;
            }
            if (!(cntlDisplayIntervall as NumericTextBox).IsValueValid())
            {
                errorMessage = (cntlDisplayIntervall as NumericTextBox).GetErrorMessage();
                return false;
            }
            if (!(cntlHistoryDepth as NumericTextBox).IsValueValid())
            {
                errorMessage = (cntlHistoryDepth as NumericTextBox).GetErrorMessage();
                return false;
            }
            if (!(cntlRetrieveIntervall as NumericTextBox).IsValueValid())
            {
                errorMessage = (cntlRetrieveIntervall as NumericTextBox).GetErrorMessage();
                return false;
            }
            // no check for cntlConfigFilename, cntlChannelAsTitle
            
            // interdependencies
            if ( (cntlRetrieveIntervall as NumericTextBox).IntValue <= (cntlDisplayIntervall as NumericTextBox).IntValue)
            {
                errorMessage = Properties.Resources.str_settingsErrorRetrieveSmallerDisplay;
                return false;
            }

            return true;
        }


        private void GetData()
        {
            // get data
            Properties.Settings.Default.balloonTimespan = (cntlBalloonTimespan as NumericTextBox).IntValue;
            Properties.Settings.Default.channelAsTitle = (cntlChannelAsTitle as CheckBox).Checked;
            Properties.Settings.Default.displayIntervall = (cntlDisplayIntervall as NumericTextBox).IntValue;
            Properties.Settings.Default.historyDepth = (cntlHistoryDepth as NumericTextBox).IntValue;
            Properties.Settings.Default.retrieveIntervall = (cntlRetrieveIntervall as NumericTextBox).IntValue;
            Properties.Settings.Default.clickInfluence = (byte)(cntlClickInfluence as TrackBar).Value;
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
                    Properties.Resources.str_settingsFormErrorHeader,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            else
            {
                // transfer data from GUI into settings block
                GetData();
                // save the settings file
                Properties.Settings.Default.Save();
                // close window
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