using Sim.Faciem;
using Unity.Properties;

namespace Sim.Faciem.Material.Samples
{
    /// <summary>
    /// Simple bindable row model used by the Material list demo templates.
    /// </summary>
    public class MaterialListDemoItem : Bindable<MaterialListDemoItem>
    {
        private string _title;
        private string _supportingText;

        [CreateProperty]
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        [CreateProperty]
        public string SupportingText
        {
            get => _supportingText;
            set => SetProperty(ref _supportingText, value);
        }

        /// <inheritdoc />
        public override string ToString() => Title;
    }
}
