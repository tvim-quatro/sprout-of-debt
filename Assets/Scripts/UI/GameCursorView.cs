using SproutOfDebt.Core;
using UnityEngine;

namespace SproutOfDebt.UI
{
    /// <summary>
    /// The game's mouse cursor (Figma "UI Kit / Cursors"), shown as the system cursor so it never lags behind the mouse.
    /// Its size follows the game window in proportion to the 1920x1080 frame (32 px there), like the rest of the art,
    /// so it is the same relative size on a small window and on a big screen.
    /// Lives in the shared HUD; gameplay code switches the look through GameServices.GameCursor.
    /// </summary>
    public class GameCursorView : MonoBehaviour, IGameCursor
    {
        private const float ReferenceFrameHeight = 1080f;
        private const int ReferenceSize = 32;
        // The cursor never gets smaller than this (in the system's cursor units), so it stays easy to see.
        private const int MinSize = 24;
        // macOS doubles cursors on Retina screens, which report about twice the usual dpi.
        private const float RetinaDpi = 150f;

        [SerializeField] private CursorStyle defaultCursor;
        [SerializeField] private CursorStyle hoverCursor;
        [SerializeField] private CursorStyle grabCursor;

        private readonly Texture2D[] scaledTextures = new Texture2D[3];
        private CursorState state;
        private int size;
        private int screenWidth;
        private int screenHeight;

        private void OnEnable()
        {
            GameServices.GameCursor = this;
            state = CursorState.Default;
            screenWidth = screenHeight = size = 0;
        }

        private void OnDisable()
        {
            if (ReferenceEquals(GameServices.GameCursor, this))
            {
                GameServices.GameCursor = null;
            }
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            DestroyTextures();
        }

        private void Update()
        {
            if (Screen.width == screenWidth && Screen.height == screenHeight)
            {
                return;
            }
            screenWidth = Screen.width;
            screenHeight = Screen.height;
            int newSize = ComputeSize();
            if (newSize != size)
            {
                size = newSize;
                RebuildTextures();
                Apply();
            }
        }

        public void Show(CursorState newState)
        {
            if (newState == state)
            {
                return;
            }
            state = newState;
            Apply();
        }

        /// <summary>Cursor image size in pixels, so the cursor keeps its size relative to the game frame.</summary>
        private int ComputeSize()
        {
            float frameHeight = Mathf.Min(screenHeight, screenWidth * 9f / 16f);
            float wantedSize = ReferenceSize * frameHeight / ReferenceFrameHeight;
            float osScale = IsRetina() ? 2f : 1f;
            return Mathf.Max(MinSize, Mathf.RoundToInt(wantedSize / osScale));
        }

        private static bool IsRetina()
        {
            bool mac = Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor;
            return mac && Screen.dpi > RetinaDpi;
        }

        private void Apply()
        {
            CursorStyle style = StyleFor(state);
            Texture2D texture = scaledTextures[(int)state];
            if (style == null || texture == null)
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                return;
            }
            Cursor.SetCursor(texture, style.Hotspot * size / ReferenceSize, CursorMode.Auto);
        }

        private CursorStyle StyleFor(CursorState cursorState)
        {
            switch (cursorState)
            {
                case CursorState.Hover: return hoverCursor;
                case CursorState.Grab: return grabCursor;
                default: return defaultCursor;
            }
        }

        private void RebuildTextures()
        {
            DestroyTextures();
            for (int i = 0; i < scaledTextures.Length; i++)
            {
                CursorStyle style = StyleFor((CursorState)i);
                scaledTextures[i] = style != null ? ScaleTexture(style.Texture, size) : null;
            }
        }

        /// <summary>Redraws the cursor at the given size with nearest-pixel sampling, so it stays sharp pixel art.</summary>
        private static Texture2D ScaleTexture(Texture2D source, int targetSize)
        {
            if (source == null || !source.isReadable)
            {
                return null;
            }
            Color32[] sourcePixels = source.GetPixels32();
            var pixels = new Color32[targetSize * targetSize];
            for (int y = 0; y < targetSize; y++)
            {
                int sourceRow = y * source.height / targetSize * source.width;
                for (int x = 0; x < targetSize; x++)
                {
                    pixels[y * targetSize + x] = sourcePixels[sourceRow + x * source.width / targetSize];
                }
            }
            var texture = new Texture2D(targetSize, targetSize, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, name = source.name };
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return texture;
        }

        private void DestroyTextures()
        {
            for (int i = 0; i < scaledTextures.Length; i++)
            {
                if (scaledTextures[i] != null)
                {
                    Destroy(scaledTextures[i]);
                    scaledTextures[i] = null;
                }
            }
        }
    }
}
