using System;

namespace Milex.GMS1.Core.UI
{
    /// <summary>
    /// Common interface for UI renderers (Classic IMGUI, Modern Canvas Dashboard).
    /// Decouples menu drawing and user interaction from core mod management.
    /// </summary>
    public interface IMenuRenderer
    {
        /// <summary>
        /// Human-readable name of this renderer engine.
        /// </summary>
        string EngineName { get; }

        /// <summary>
        /// Whether the menu interface is currently visible/active.
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// Initializes the renderer with the host GameObject and core plugin references.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Shows the menu interface.
        /// </summary>
        void Show();

        /// <summary>
        /// Hides the menu interface.
        /// </summary>
        void Hide();

        /// <summary>
        /// Cleans up any resources, GameObjects or listeners created by this renderer.
        /// </summary>
        void Cleanup();
    }
}
