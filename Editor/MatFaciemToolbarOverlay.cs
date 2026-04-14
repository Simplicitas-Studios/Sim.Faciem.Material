using System;
using Cysharp.Threading.Tasks;
using Plugins.Sim.Faciem.Editor;
using UnityEngine.UIElements;

namespace Sim.Faciem.Material.Editor
{
    /// <summary>
    /// Base class for toolbar overlays that use Material Design controls.
    /// Extends <see cref="FaciemToolbarOverlay"/> and injects the correct editor
    /// theme stylesheet into the root element created by
    /// <see cref="CreateRootElement"/>.
    ///
    /// Because <c>FaciemToolbarOverlay.CreatePanelContent()</c> is sealed and calls
    /// <see cref="CreateRootElement"/> internally, overriding
    /// <see cref="CreateRootElement"/> here is sufficient to style all panel content.
    ///
    /// Note: if the editor skin changes while the overlay is open, toggle it
    /// off and on again to pick up the updated theme.
    /// </summary>
    public abstract class MatFaciemToolbarOverlay : FaciemToolbarOverlay
    {
        private VisualElement _overlayRoot;
        private IDisposable   _themeSubscription;

        protected override VisualElement CreateRootElement()
        {
            _overlayRoot = base.CreateRootElement();
            _themeSubscription?.Dispose();
            _themeSubscription = MatEditorStyles.ApplyTo(_overlayRoot);
            return _overlayRoot;
        }

        protected override UniTask NavigateAwayPanel()
        {
            _themeSubscription?.Dispose();
            _themeSubscription = null;

            // Remove injected stylesheets when the overlay panel is torn down
            // so they don't accumulate if the panel is recreated.
            if (_overlayRoot != null)
            {
                MatEditorStyles.RemoveFrom(_overlayRoot);
                _overlayRoot = null;
            }

            return base.NavigateAwayPanel();
        }
    }
}
