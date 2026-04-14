using System;
using Cysharp.Threading.Tasks;
using Plugins.Sim.Faciem.Editor;

namespace Sim.Faciem.Material.Editor
{
    /// <summary>
    /// Base class for editor windows that use Material Design controls.
    /// Extends <see cref="FaciemEditorWindow"/>, injects the active Material theme
    /// stylesheet into <c>rootVisualElement</c>, and re-styles automatically when
    /// the user switches themes via <c>Faciem / Theme</c>.
    /// </summary>
    public abstract class MatFaciemEditorWindow : FaciemEditorWindow
    {
        private IDisposable _themeSubscription;

        protected override UniTask NavigateTo()
        {
            _themeSubscription?.Dispose();
            _themeSubscription = MatEditorStyles.ApplyTo(rootVisualElement);
            return base.NavigateTo();
        }

        protected override UniTask NavigateAway()
        {
            _themeSubscription?.Dispose();
            _themeSubscription = null;
            return base.NavigateAway();
        }
    }
}
