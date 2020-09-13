﻿using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.Window.Commands;
using MixItUp.WPF.Controls.Commands;
using System.Threading.Tasks;

namespace MixItUp.WPF.Windows.Commands
{
    /// <summary>
    /// Interaction logic for CommandEditorWindow.xaml
    /// </summary>
    public partial class CommandEditorWindow : LoadingWindowBase
    {
        public CommandEditorDetailsControlBase editorDetailsControl { get; private set; }

        public CommandEditorWindowViewModelBase viewModel { get; private set; }

        public CommandEditorWindow(CommandModelBase existingCommand)
            : this()
        {
            switch (existingCommand.Type)
            {
                case CommandTypeEnum.Timer:
                    this.editorDetailsControl = new TimerCommandEditorDetailsControl();
                    this.viewModel = new TimerCommandEditorWindowViewModel((TimerCommandModel)existingCommand);
                    break;
            }
            this.DataContext = this.ViewModel = this.viewModel;

            this.ViewModel.StartLoadingOperationOccurred += (sender, eventArgs) => { this.StartLoadingOperation(); };
            this.ViewModel.EndLoadingOperationOccurred += (sender, eventArgs) => { this.EndLoadingOperation(); };
        }

        public CommandEditorWindow(CommandTypeEnum commandType)
            : this()
        {
            switch (commandType)
            {
                case CommandTypeEnum.Timer:
                    this.editorDetailsControl = new TimerCommandEditorDetailsControl();
                    this.viewModel = new TimerCommandEditorWindowViewModel();
                    break;
            }
            this.DataContext = this.ViewModel = this.viewModel;

            this.ViewModel.StartLoadingOperationOccurred += (sender, eventArgs) => { this.StartLoadingOperation(); };
            this.ViewModel.EndLoadingOperationOccurred += (sender, eventArgs) => { this.EndLoadingOperation(); };
        }

        private CommandEditorWindow()
            : base()
        {
            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            this.DetailsContentControl.Content = this.editorDetailsControl;

            await base.OnLoaded();
        }
    }
}