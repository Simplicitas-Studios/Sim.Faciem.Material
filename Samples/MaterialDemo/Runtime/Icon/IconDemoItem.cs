using Unity.Properties;
using UnityEngine.UIElements;

namespace Sim.Faciem.Material.Samples.Icon
{
    /// <summary>Bindable icon grid item.</summary>
    public class IconDemoItem : Bindable<IconDemoItem>
    {
        private string _fileName;
        private StyleBackground _iconSprite;

        [CreateProperty]
        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }

        [CreateProperty]
        public StyleBackground IconSprite
        {
            get => _iconSprite;
            set => SetProperty(ref _iconSprite, value);
        }

        /// <inheritdoc />
        public override string ToString() => FileName;
    }
}