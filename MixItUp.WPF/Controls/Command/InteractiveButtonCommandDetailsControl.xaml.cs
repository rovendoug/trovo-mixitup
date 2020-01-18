﻿using Mixer.Base.Model.MixPlay;
using MixItUp.Base;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.WPF.Util;
using StreamingClient.Base.Util;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Command
{
    /// <summary>
    /// Interaction logic for InteractiveButtonCommandDetailsControl.xaml
    /// </summary>
    public partial class InteractiveButtonCommandDetailsControl : CommandDetailsControlBase
    {
        public MixPlayGameModel Game { get; private set; }
        public MixPlayGameVersionModel Version { get; private set; }
        public MixPlaySceneModel Scene { get; private set; }
        public MixPlayButtonControlModel Control { get; private set; }

        private MixPlayButtonCommand command;

        public InteractiveButtonCommandDetailsControl(MixPlayGameModel game, MixPlayGameVersionModel version, MixPlayButtonCommand command)
        {
            this.Game = game;
            this.Version = version;
            this.command = command;
            this.Control = command.Button;

            InitializeComponent();
        }

        public InteractiveButtonCommandDetailsControl(MixPlayGameModel game, MixPlayGameVersionModel version, MixPlaySceneModel scene, MixPlayButtonControlModel control)
        {
            this.Game = game;
            this.Version = version;
            this.Scene = scene;
            this.Control = control;

            InitializeComponent();
        }

        public override Task Initialize()
        {
            this.Requirements.SettingsRequirement.HideDeleteChatCommandWhenRun();

            this.ButtonTriggerComboBox.ItemsSource = EnumHelper.GetEnumNames<MixPlayButtonCommandTriggerType>();
            this.HeldRateTextBox.Text = "1";

            if (this.Control != null)
            {
                this.NameTextBox.Text = this.Control.controlID;
                this.ButtonTriggerComboBox.IsEnabled = true;
                this.ButtonTriggerComboBox.SelectedItem = EnumHelper.GetEnumName(MixPlayButtonCommandTriggerType.MouseKeyDown);
                this.SparkCostTextBox.IsEnabled = true;
                this.SparkCostTextBox.Text = this.Control.cost.ToString();
            }

            if (this.Scene != null)
            {
                this.SceneTextBox.Text = this.Scene.sceneID;
            }

            if (this.command != null)
            {
                this.SceneTextBox.Text = this.command.SceneID;
                this.ButtonTriggerComboBox.SelectedItem = EnumHelper.GetEnumName(this.command.Trigger);
                this.HeldRateTextBox.Text = this.command.HeldRate.ToString();
                this.UnlockedControl.Unlocked = this.command.Unlocked;
                this.Requirements.SetRequirements(this.command.Requirements);

                if (this.Game != null)
                {
                    this.Scene = this.Version.controls.scenes.FirstOrDefault(s => s.sceneID.Equals(this.command.SceneID));
                }
            }

            return Task.FromResult(0);
        }

        public override async Task<bool> Validate()
        {
            if (this.ButtonTriggerComboBox.SelectedIndex < 0)
            {
                await DialogHelper.ShowMessage("An trigger type must be selected");
                return false;
            }

            MixPlayButtonCommandTriggerType trigger = EnumHelper.GetEnumValueFromString<MixPlayButtonCommandTriggerType>((string)this.ButtonTriggerComboBox.SelectedItem);
            if (trigger == MixPlayButtonCommandTriggerType.MouseKeyHeld)
            {
                if (string.IsNullOrEmpty(this.HeldRateTextBox.Text) || !int.TryParse(this.HeldRateTextBox.Text, out int heldRate) || heldRate < 1)
                {
                    await DialogHelper.ShowMessage("A valid held rate of 1 or greater must be entered");
                    return false;
                }
            }

            if (!int.TryParse(this.SparkCostTextBox.Text, out int sparkCost) || sparkCost < 0)
            {
                await DialogHelper.ShowMessage("A valid spark cost must be entered");
                return false;
            }

            if (!await this.Requirements.Validate())
            {
                return false;
            }

            return true;
        }

        public override CommandBase GetExistingCommand() { return this.command; }

        public override async Task<CommandBase> GetNewCommand()
        {
            if (await this.Validate())
            {
                MixPlayButtonCommandTriggerType trigger = EnumHelper.GetEnumValueFromString<MixPlayButtonCommandTriggerType>((string)this.ButtonTriggerComboBox.SelectedItem);

                RequirementViewModel requirements = this.Requirements.GetRequirements();

                if (this.command == null)
                {
                    this.command = new MixPlayButtonCommand(this.Game, this.Scene, this.Control, trigger, requirements);
                    ChannelSession.Settings.MixPlayCommands.Add(this.command);
                }

                this.command.Trigger = trigger;
                this.command.Button.cost = int.Parse(this.SparkCostTextBox.Text);
                this.command.Unlocked = this.UnlockedControl.Unlocked;
                this.command.Requirements = requirements;

                if (this.command.Trigger == MixPlayButtonCommandTriggerType.MouseKeyHeld)
                {
                    int.TryParse(this.HeldRateTextBox.Text, out int heldRate);
                    this.command.HeldRate = heldRate;
                }

                await ChannelSession.MixerUserConnection.UpdateMixPlayGameVersion(this.Version);
                return this.command;
            }
            return null;
        }

        private void ButtonTriggerComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.ButtonTriggerComboBox.SelectedIndex >= 0)
            {
                MixPlayButtonCommandTriggerType trigger = EnumHelper.GetEnumValueFromString<MixPlayButtonCommandTriggerType>((string)this.ButtonTriggerComboBox.SelectedItem);
                if (trigger == MixPlayButtonCommandTriggerType.MouseKeyHeld)
                {
                    this.HeldRateTextBox.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    this.HeldRateTextBox.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }
    }
}
