using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SortTarget
{
    public Component target;   // SpriteRenderer 또는 Canvas
    public int offset;         // 개별 미세조정
}

public class YSort : MonoBehaviour
{
    [SerializeField] private int baseOffset;          // 전체 공통 미세조정
    [SerializeField] private List<SortTarget> targets = new List<SortTarget>();

    void Awake()
    {
        AutoCollect();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoCollect();
    }
#endif

    void LateUpdate()
    {
        foreach (var entry in targets)
        {
            if (entry.target == null) continue;

            float yValue = -entry.target.transform.position.y * 10f;

            int finalOrder = Mathf.RoundToInt(yValue) + baseOffset + entry.offset;

            if (entry.target is SpriteRenderer sr)
            {
                sr.sortingOrder = finalOrder;
            }
            else if (entry.target is Canvas canvas)
            {
                canvas.overrideSorting = true;
                canvas.sortingOrder = finalOrder;
            }
        }
    }

    void AutoCollect()
    {
        var spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        var canvases = GetComponentsInChildren<Canvas>(true);

        Dictionary<Component, int> existing = new Dictionary<Component, int>();

        foreach (var t in targets)
        {
            if (t.target != null && !existing.ContainsKey(t.target))
                existing.Add(t.target, t.offset);
        }

        targets.Clear();

        foreach (var sr in spriteRenderers)
        {
            targets.Add(new SortTarget
            {
                target = sr,
                offset = existing.ContainsKey(sr) ? existing[sr] : 0
            });
        }

        foreach (var canvas in canvases)
        {
            targets.Add(new SortTarget
            {
                target = canvas,
                offset = existing.ContainsKey(canvas) ? existing[canvas] : 0
            });
        }
    }
}
