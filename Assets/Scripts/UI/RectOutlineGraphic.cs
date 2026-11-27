using UnityEngine;
using UnityEngine.UI;

namespace SproutOfDebt.UI
{
    /// <summary>
    /// A plain rectangle outline drawn inside the rect (Figma rectangle with a stroke and no fill),
    /// e.g. the debt bar's hover outline.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class RectOutlineGraphic : MaskableGraphic
    {
        [Tooltip("Stroke width in reference pixels (Figma stroke weight).")]
        [SerializeField] private float thickness = 3f;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = rectTransform.rect;
            float t = Mathf.Min(thickness, r.width * 0.5f, r.height * 0.5f);
            AddQuad(vh, Rect.MinMaxRect(r.xMin, r.yMax - t, r.xMax, r.yMax));
            AddQuad(vh, Rect.MinMaxRect(r.xMin, r.yMin, r.xMax, r.yMin + t));
            AddQuad(vh, Rect.MinMaxRect(r.xMin, r.yMin + t, r.xMin + t, r.yMax - t));
            AddQuad(vh, Rect.MinMaxRect(r.xMax - t, r.yMin + t, r.xMax, r.yMax - t));
        }

        private void AddQuad(VertexHelper vh, Rect q)
        {
            int start = vh.currentVertCount;
            vh.AddVert(new Vector3(q.xMin, q.yMin), color, Vector2.zero);
            vh.AddVert(new Vector3(q.xMin, q.yMax), color, Vector2.zero);
            vh.AddVert(new Vector3(q.xMax, q.yMax), color, Vector2.zero);
            vh.AddVert(new Vector3(q.xMax, q.yMin), color, Vector2.zero);
            vh.AddTriangle(start, start + 1, start + 2);
            vh.AddTriangle(start + 2, start + 3, start);
        }
    }
}
