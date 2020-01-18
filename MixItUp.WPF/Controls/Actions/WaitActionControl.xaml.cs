﻿using MixItUp.Base.Actions;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Actions
{
    /// <summary>
    /// Interaction logic for WaitActionControl.xaml
    /// </summary>
    public partial class WaitActionControl : ActionControlBase
    {
        private WaitAction action;

        public WaitActionControl() : base() { InitializeComponent(); }

        public WaitActionControl(WaitAction action) : this() { this.action = action; }

        public override Task OnLoaded()
        {
            if (this.action != null)
            {
                this.WaitAmountTextBox.Text = this.action.Amount;
            }
            return Task.FromResult(0);
        }

        public override ActionBase GetAction()
        {
            if (!string.IsNullOrEmpty(this.WaitAmountTextBox.Text))
            {
                return new WaitAction(this.WaitAmountTextBox.Text);
            }
            return null;
        }
    }
}
