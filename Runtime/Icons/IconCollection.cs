using UnityEngine;
using UnityEngine.UIElements;

namespace Sim.Faciem.Material.Icons
{
    [CreateAssetMenu(fileName = "IconCollection", menuName = "Sim/Faciem.Material/IconCollection")]
    public class IconCollection : ScriptableObject
    {
        public string CollectionName;
        
        public VectorImage[] Icons;
    }
}