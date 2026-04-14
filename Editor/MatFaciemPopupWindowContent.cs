using System;
using Cysharp.Threading.Tasks;
using Plugins.Sim.Faciem.Editor;
using UnityEngine.UIElements;

namespace Sim.Faciem.Material.Editor
{
    /// <summary>
    /// Base class for popup window contents that use Material Design controls.
    /// Extends <see cref="FaciemPopupWindowContent"/>, injects the active Material
    /// theme stylesheet into the root <see cref="VisualElement"/>, and re-styles
    /// automatically when the user switches themes via <c>Faciem / Theme</c>.
    /// </summary>
    public abstract class MatFaciemPopupWindowContent : FaciemPopupWindowContent
    {
        private IDisposable _themeSubscription;

        public override VisualElement CreateGUI()
        {
            var root = base.CreateGUI();
            _themeSubscription = MatEditorStyles.ApplyTo(root);
            return root;
        }

        protected override UniTask NavigateAwayPopup()
        {
            _themeSubscription?.Dispose();
            _themeSubscription = null;
            return base.NavigateAwayPopup();
        }
    }
}
