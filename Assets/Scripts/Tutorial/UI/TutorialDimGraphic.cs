using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SproutOfDebt.Tutorial.UI
{
    /// <summary>
    /// Full-frame dim that leaves see-through, clickable holes over the highlighted targets
    /// (Figma "Dim overlay": one rectangle minus one or more cutouts).
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class TutorialDimGraphic : MaskableGraphic, ICanvasRaycastFilter
    {
        private readonly List<Rect> holes = new List<Rect>();
        private readonly List<float> xs = new List<float>();
        private readonly List<float> ys = new List<float>();

        /// <summary>Sets the holes in this graphic's local space; an empty list dims the whole frame.</summary>
        public void SetHoles(IReadOnlyList<Rect> newHoles)
        {
            if (SameHoles(newHoles))
            {
                return;
            }
            holes.Clear();
            if (newHoles != null)
            {
                for (int i = 0; i < newHoles.Count; i++)
                {
                    holes.Add(newHoles[i]);
                }
            }
            SetVerticesDirty();
        }

        // The hole edges split the frame into a grid; every grid cell outside the holes becomes a dark quad.
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect frame = rectTransform.rect;
            xs.Clear();
            ys.Clear();
            xs.Add(frame.xMin);
            xs.Add(frame.xMax);
            ys.Add(frame.yMin);
            ys.Add(frame.yMax);
            foreach (Rect hole in holes)
            {
                xs.Add(Mathf.Clamp(hole.xMin, frame.xMin, frame.xMax));
                xs.Add(Mathf.Clamp(hole.xMax, frame.xMin, frame.xMax));
                ys.Add(Mathf.Clamp(hole.yMin, frame.yMin, frame.yMax));
                ys.Add(Mathf.Clamp(hole.yMax, frame.yMin, frame.yMax));
            }
            xs.Sort();
            ys.Sort();

            for (int i = 0; i < xs.Count - 1; i++)
            {
                for (int j = 0; j < ys.Count - 1; j++)
                {
                    var cell = Rect.MinMaxRect(xs[i], ys[j], xs[i + 1], ys[j + 1]);
                    if (cell.width > 0f && cell.height > 0f && !InsideHole(cell.center))
                    {
                        AddQuad(vh, cell);
                    }
                }
            }
        }

        /// <summary>
        /// When true, clicks inside a hole pass through to the highlighted target. When false the holes
        /// only show the target and the whole screen stays blocked (steps that just point at something).
        /// </summary>
        public bool HolesClickThrough { get; set; } = true;

        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            if (!HolesClickThrough)
            {
                return true;
            }
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out Vector2 local))
            {
                return true;
            }
            return !InsideHole(local);
        }

        private bool InsideHole(Vector2 point)
        {
            foreach (Rect hole in holes)
            {
                if (hole.Contains(point))
                {
                    return true;
                }
            }
            return false;
        }

        private bool SameHoles(IReadOnlyList<Rect> other)
        {
            int count = other != null ? other.Count : 0;
            if (count != holes.Count)
            {
                return false;
            }
            for (int i = 0; i < count; i++)
            {
                if (other[i] != holes[i])
                {
                    return false;
                }
            }
            return true;
        }

        private void AddQuad(VertexHelper vh, Rect cell)
        {
            int start = vh.currentVertCount;
            vh.AddVert(new Vector3(cell.xMin, cell.yMin), color, Vector2.zero);
            vh.AddVert(new Vector3(cell.xMin, cell.yMax), color, Vector2.zero);
            vh.AddVert(new Vector3(cell.xMax, cell.yMax), color, Vector2.zero);
            vh.AddVert(new Vector3(cell.xMax, cell.yMin), color, Vector2.zero);
            vh.AddTriangle(start, start + 1, start + 2);
            vh.AddTriangle(start + 2, start + 3, start);
        }
    }
}
