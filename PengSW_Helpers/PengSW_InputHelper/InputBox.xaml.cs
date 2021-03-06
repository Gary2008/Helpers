﻿using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace PengSW.InputHelper
{
    /// <summary>
    /// InputBox.xaml 的交互逻辑
    /// </summary>
    public partial class InputBox : Window
    {
        public InputBox(string aCaption, string aPrompt, string aDefaultValue, string aOkPattern = null, string aNotOkPattern = null)
        {
            Caption = aCaption;
            Prompt = aPrompt;
            Value = aDefaultValue;
            if (aOkPattern != null) _OkRegex = new Regex(aOkPattern);
            if (aNotOkPattern != null) _NotOkRegex = new Regex(aNotOkPattern);

            InitializeComponent();
            this.DataContext = this;
        }

        public string Caption { get; private set; }
        public string Prompt { get; private set; }
        public string Value { get; set; }

        private Regex _OkRegex = null;
        private Regex _NotOkRegex = null;

        private void OnOk_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (_OkRegex != null)
                e.CanExecute = _OkRegex.IsMatch(Value);
            else if (_NotOkRegex != null)
                e.CanExecute = !_NotOkRegex.IsMatch(Value);
            else
                e.CanExecute = true;
        }

        private void OnOk_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void txtValue_GotFocus(object sender, RoutedEventArgs e)
        {
            txtValue.SelectAll();
        }

        private void Window_Activated(object sender, System.EventArgs e)
        {
            if (!txtValue.IsFocused)
            {
                txtValue.Focus();
                txtValue.SelectAll();
            }
        }
    }
}
