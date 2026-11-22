using System;
using UnityEngine;

namespace SproutOfDebt.Core
{
    /// <summary>One cursor image from Figma "UI Kit / Cursors" and the pixel it points with.</summary>
    [Serializable]
    public class CursorStyle
    {
        [Tooltip("32x32 cursor image; its import settings must have Read/Write enabled.")]
        [SerializeField] private Texture2D texture;
        [Tooltip("The pixel of the 32x32 image that points, from its top-left corner.")]
        [SerializeField] private Vector2 hotspot;

        public Texture2D Texture => texture;
        public Vector2 Hotspot => hotspot;
    }
}
