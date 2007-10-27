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

/* this class is taken from the MSDN example */

using System;
using System.Windows.Forms;
using System.Globalization;

namespace BalloonRss
{
    public class NumericTextBox : TextBox
    {
        private bool allowSpace = false;

        private int minValue;
        private int maxValue;
        private string description;

        public NumericTextBox(int minValue, int maxValue, string description)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.description = description;
            this.TextAlign = HorizontalAlignment.Right;
        }

        // Restricts the entry of characters to digits (including hex), the negative sign,
        // the decimal point, and editing keystrokes (backspace).
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            NumberFormatInfo numberFormatInfo = System.Globalization.CultureInfo.CurrentCulture.NumberFormat;
            string decimalSeparator = numberFormatInfo.NumberDecimalSeparator;
            string groupSeparator = numberFormatInfo.NumberGroupSeparator;
            string negativeSign = numberFormatInfo.NegativeSign;

            string keyInput = e.KeyChar.ToString();

            if (Char.IsDigit(e.KeyChar))
            {
                // Digits are OK
            }
            else if (keyInput.Equals(decimalSeparator) || keyInput.Equals(groupSeparator) || keyInput.Equals(negativeSign))
            {
                // Decimal and negative numbers are not OK
                // swallow this invalid key
                e.Handled = true;
            }
            else if (e.KeyChar == '\b')
            {
                // Backspace key is OK
            }
            else if ((ModifierKeys & (Keys.Control | Keys.Alt)) != 0)
            {
                // Let the edit control handle control and alt key combinations
            }
            else if (this.allowSpace && e.KeyChar == ' ')
            {

            }
            else
            {
                // swallow this invalid key
                e.Handled = true;
                //    MessageBeep();
            }
        }

        public int IntValue
        {
            get
            {
                return Int32.Parse(this.Text);
            }
        }

        public bool IsValueValid()
        {
            int value;
            try
            {
                value = IntValue;
            }
            catch(Exception)
            {
                return false;
            }

            if ( (value >= minValue) && (value <= maxValue) )
                return true;
            else
                return false;
        }

        public string GetErrorMessage()
        {
            if (IntValue < minValue)
                return description + ": " + resources.str_settingsErrorValueToSmall + minValue + ".";

            if (IntValue > maxValue)
                return description + ": " + resources.str_settingsErrorValueToLarge + maxValue + ".";

            return null;
        }
    }
}