using System;
using R3;
using Sim.Faciem.CommandBinding;
using Sim.Faciem.Material.Icons;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Sim.Faciem.Material.Controls
{
    /// <summary>
    /// Declarative child item used by <see cref="MatMenu"/>.
    /// Define these as UXML children of <c>&lt;Sim.Faciem.Material.Controls.MatMenu&gt;</c>
    /// to describe a flat popup menu.
    /// </summary>
    [UxmlElement]
    public partial class MatMenuItem : VisualElement
    {
        private string _label = string.Empty;
        private bool _disabled;
        private IconCollection _iconCollection;
        private string _iconName = string.Empty;
        private SerializedCommand _command;
        private DisposableBag _commandSubscriptions = new();
        private bool _commandCanExecute = true;
        private bool _isVisible = true;

        internal event Action Changed;

        [UxmlAttribute, CreateProperty]
        public string Label
        {
            get => _label;
            set
            {
                var next = value ?? string.Empty;
                if (_label == next) return;
                _label = next;
                NotifyChanged();
            }
        }

        [UxmlAttribute, CreateProperty]
        public bool Disabled
        {
            get => _disabled;
            set
            {
                if (_disabled == value) return;
                _disabled = value;
                NotifyChanged();
            }
        }

        [UxmlAttribute, CreateProperty]
        public IconCollection IconCollection
        {
            get => _iconCollection;
            set
            {
                if (_iconCollection == value) return;
                _iconCollection = value;
                NotifyChanged();
            }
        }

        [UxmlAttribute, CreateProperty]
        public string IconName
        {
            get => _iconName;
            set
            {
                var next = value ?? string.Empty;
                if (_iconName == next) return;
                _iconName = next;
                NotifyChanged();
            }
        }

        [UxmlAttribute, CreateProperty]
        public SerializedCommand Command
        {
            get => _command;
            set
            {
                _command = value;
                RegisterCommandCallbacks();
                NotifyChanged();
            }
        }

        internal bool IsVisible => _isVisible;
        internal bool IsEffectivelyDisabled => _disabled || !_commandCanExecute;
        internal bool HasIcon => !string.IsNullOrWhiteSpace(_iconName);

        public MatMenuItem()
        {
            style.display = DisplayStyle.None;
            pickingMode = PickingMode.Ignore;
        }

        internal void Execute()
        {
            if (IsEffectivelyDisabled || !IsVisible)
                return;

            _command?.Command?.Execute(Unit.Default);
            NotifyChanged();
        }

        private void RegisterCommandCallbacks()
        {
            _commandSubscriptions.Dispose();
            _commandSubscriptions = new DisposableBag();

            _commandCanExecute = _command?.Command?.CanExecute() ?? true;
            _isVisible = true;

            if (_command?.Command == null)
                return;

            _commandSubscriptions.Add(
                _command.Command.CanExecuteObs
                    .Prepend(_command.Command.CanExecute)
                    .Subscribe(canExecute =>
                    {
                        _commandCanExecute = canExecute;
                        NotifyChanged();
                    }));

            _commandSubscriptions.Add(
                _command.Command.IsVisibleObs
                    .Prepend(true)
                    .Subscribe(isVisible =>
                    {
                        _isVisible = isVisible;
                        NotifyChanged();
                    }));
        }

        private void NotifyChanged() => Changed?.Invoke();
    }
}
