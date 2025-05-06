using System;
using System.Collections;
using UnityEngine;

[ExecuteAlways]
public class RectOutlineEffect : MonoBehaviour
{
    public static RectOutlineEffect Instance;
    private static readonly int RectsID = Shader.PropertyToID("_Rects");
    private RectRenderer[] rects = new RectRenderer[4];
    private Coroutine[] animCoroutines = new Coroutine[4];
    public Material overlayMaterial;
    Matrix4x4 rectMatrix = Matrix4x4.zero;

    private Camera cam;

    [SerializeField] private AnimationCurve startAnimCurve;
    [SerializeField] private Vector2 startAnimSize;
    
    [SerializeField] private AnimationCurve endAnimCurve;
    [SerializeField] private Vector2 endAnimSize;

    [SerializeField] private float animDuration = 0.5f;
    

    private void OnDestroy()
    {
        Instance = null;
    }

    private void OnEnable()
    {
        if (Instance != null)
        {
            DestroyImmediate(this);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (cam == null) cam = Camera.main;
        UpdateRects();
    }

    private void OnDisable()
    {
        Instance = null;
        if (overlayMaterial != null) overlayMaterial.SetMatrix(RectsID, Matrix4x4.zero);
    }

    public void AddRect(Renderer r)
    {
        RectRenderer rectRenderer = new RectRenderer(r);
        for (int i = 0; i < rects.Length; i++)
        {
            if (rects[i] == null)
            {
                rects[i] = rectRenderer;
                if (animCoroutines[i] != null)
                    StopCoroutine(animCoroutines[i]);
                animCoroutines[i] = StartCoroutine(rectRenderer.Anim(startAnimCurve, animDuration, startAnimSize, Vector2.one));
                return;
            }
        }
    }
    public void RemoveRect(Renderer r)
    {
        for (int i = 0; i < rects.Length; i++)
        {
            if (rects[i] == null) continue;
            if (rects[i].renderer == r)
            {
                if (animCoroutines[i] != null)
                    StopCoroutine(animCoroutines[i]);
                animCoroutines[i] = StartCoroutine(RemoveRectCoroutine(i));
                return;
            }
        }
    }
    private IEnumerator RemoveRectCoroutine(int ind)
    {
        if (rects[ind] == null) yield break;
        yield return rects[ind].Anim(endAnimCurve, animDuration, Vector2.one, endAnimSize);
        rects[ind] = null;
    }

    /// <summary>
    /// Calcule et envoie les bounds viewport pour tous les targets.
    /// </summary>
    public void UpdateRects()
    {
        if (cam == null || overlayMaterial == null || rects == null) return;

        rectMatrix = Matrix4x4.zero;
        for (int i = 0; i < rects.Length && i < 4; i++)
        {
            if (rects[i] == null) continue;
            Rect r = rects[i].GetRect(cam) ?? default;
            if (r == default) continue;
            rectMatrix.SetRow(i, new Vector4(r.xMin, r.yMin, r.xMax, r.yMax));
        }
        overlayMaterial.SetMatrix(RectsID, rectMatrix);
    }

    private class RectRenderer
    {
        public Renderer renderer;
        private Vector2 size = Vector2.zero;

        public RectRenderer(Renderer renderer)
        {
            this.renderer = renderer;
        }

        public IEnumerator Anim(AnimationCurve curve, float duration, Vector2 startSize, Vector2 endSize)
        {
            float time = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                float t = curve.Evaluate(time / duration);
                size = Vector2.Lerp(startSize, endSize, t);
                yield return null;
            }
            size = endSize;
        }

        public Rect? GetRect(Camera cam)
        {
            {
                if (renderer == null || cam == null) return null;

                // 1) Récupère le Bounds en world space
                Bounds b = renderer.bounds;  // AABB en world space :contentReference[oaicite:3]{index=3}

                // 2) Définit les 8 coins du volume
                Vector3[] corners = new Vector3[8]
                {
                    b.center + new Vector3( b.extents.x,  b.extents.y,  b.extents.z),
                    b.center + new Vector3( b.extents.x,  b.extents.y, -b.extents.z),
                    b.center + new Vector3( b.extents.x, -b.extents.y,  b.extents.z),
                    b.center + new Vector3( b.extents.x, -b.extents.y, -b.extents.z),
                    b.center + new Vector3(-b.extents.x,  b.extents.y,  b.extents.z),
                    b.center + new Vector3(-b.extents.x,  b.extents.y, -b.extents.z),
                    b.center + new Vector3(-b.extents.x, -b.extents.y,  b.extents.z),
                    b.center + new Vector3(-b.extents.x, -b.extents.y, -b.extents.z),
                };
                
                for (int i = 0; i < corners.Length; i++)
                {
                    Debug.Log(corners[i]);
                }

                Vector2 min = new Vector2(1f, 1f);
                Vector2 max = new Vector2(0f, 0f);
                bool anyVisible = false;

                // 3) Projection en viewport space [0;1]
                foreach (var corner in corners)
                {
                    Vector3 vp = cam.WorldToViewportPoint(corner);  
                    // z > 0 ⇒ devant la caméra :contentReference[oaicite:4]{index=4}
                    if (vp.z < 0f) continue;
                    anyVisible = true;
                    min = Vector2.Min(min, new Vector2(vp.x, vp.y));
                    max = Vector2.Max(max, new Vector2(vp.x, vp.y));
                }
                Debug.Log($"min: {min}, max: {max}");
                Debug.Log("size: " + size);

                if (!anyVisible)
                    return null;

                Vector2 uvCenter = (min + max) * 0.5f;
                Vector2 uvSize = (max - min) * 0.5f;

                min = new Vector2(uvCenter.x - uvSize.x * size.x, uvCenter.y - uvSize.y * size.y);
                max = new Vector2(uvCenter.x + uvSize.x * size.x, uvCenter.y + uvSize.y * size.y);

                // 4) Clamp entre 0 et 1 pour rester dans la fenêtre
                min = Vector2.Max(min, Vector2.zero);
                max = Vector2.Min(max, Vector2.one);

                // 5) Construit le Rect (x,y,width,height)
                return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            }
        }
    }
}