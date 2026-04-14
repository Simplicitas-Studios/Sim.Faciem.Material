namespace Sim.Faciem.Material.Editor
{
    /// <summary>
    /// The four built-in Material colour themes available for editor windows.
    /// Select a theme via <see cref="MatEditorStyles.SetTheme"/> or through
    /// the <c>Faciem / Theme</c> menu.
    /// </summary>
    public enum MatEditorTheme
    {
        /// <summary>Purple 500 primary · Green A200 accent · dark surface (default).</summary>
        PurpleGreen = 0,

        /// <summary>Indigo 500 primary · Pink A200 accent · light surface.</summary>
        Indigo = 1,

        /// <summary>Deep Purple 500 primary · Amber A200 accent · light surface.</summary>
        DeepPurple = 2,

        /// <summary>Pink 500 primary · Blue Grey 300 accent · dark surface.</summary>
        PinkBlueGrey = 3,
    }
}

