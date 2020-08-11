﻿using MixItUp.Base;
using MixItUp.Base.Model.API;
using MixItUp.Base.Util;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for ChangelogControl.xaml
    /// </summary>
    public partial class ChangelogControl : MainControlBase
    {
        public ChangelogControl()
        {
            InitializeComponent();

            GlobalEvents.OnMainMenuStateChanged += GlobalEvents_OnMainMenuStateChanged;
        }

        protected override async Task InitializeInternal()
        {
            MixItUpUpdateModel update = await ChannelSession.Services.MixItUpService.GetLatestUpdate();
            if (update != null)
            {
                using (HttpClient client = new HttpClient())
                {
                    string changelogHTML = await client.GetStringAsync(update.ChangelogLink);
                    this.ChangelogWebBrowser.NavigateToString(changelogHTML);
                }
            }
            await base.InitializeInternal();
        }

        private void GlobalEvents_OnMainMenuStateChanged(object sender, bool state)
        {
            this.ChangelogWebBrowser.Visibility = (state) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
