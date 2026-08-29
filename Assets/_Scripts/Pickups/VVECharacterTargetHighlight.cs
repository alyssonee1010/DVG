using System.Collections.Generic;
using UnityEngine;

// Reusable hover/selection highlight for potions and other character-targeting tools.
// It renders a slightly enlarged colored copy of each character sprite without
// changing the character's original colors or its health bar.
public class VVECharacterTargetHighlight : MonoBehaviour
{
    [SerializeField] Color highlightColor = new Color(1f, 0.82f, 0.12f, 0.65f);
    [SerializeField, Min(1f)] float scaleMultiplier = 1.08f;
    [SerializeField] int sortingOrderOffset = 2;
    [SerializeField] float depthOffset = -0.02f;

    readonly Dictionary<SpriteRenderer, SpriteRenderer> ghosts =
        new Dictionary<SpriteRenderer, SpriteRenderer>();

    VVEDefender currentTarget;

    public VVEDefender CurrentTarget => currentTarget;

    void OnDisable()
    {
        Clear();
    }

    public void Show(VVEDefender target)
    {
        if (target != currentTarget)
        {
            Clear();
            currentTarget = target;
        }

        if (target == null)
        {
            return;
        }

        HashSet<SpriteRenderer> visibleSources = new HashSet<SpriteRenderer>();
        foreach (SpriteRenderer source in target.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (source == null || IsHealthBarRenderer(source) || IsHighlightRenderer(source))
            {
                continue;
            }

            visibleSources.Add(source);
            EnsureGhost(source);
        }

        List<SpriteRenderer> staleSources = new List<SpriteRenderer>();
        foreach (SpriteRenderer source in ghosts.Keys)
        {
            if (source == null || !visibleSources.Contains(source))
            {
                staleSources.Add(source);
            }
        }

        foreach (SpriteRenderer source in staleSources)
        {
            RemoveGhost(source);
        }
    }

    public void Clear()
    {
        foreach (SpriteRenderer ghost in ghosts.Values)
        {
            if (ghost != null)
            {
                Destroy(ghost.gameObject);
            }
        }

        ghosts.Clear();
        currentTarget = null;
    }

    public bool IsHighlightRenderer(SpriteRenderer renderer)
    {
        return renderer != null
            && (ghosts.ContainsValue(renderer) || renderer.name == "Character Target Highlight");
    }

    void EnsureGhost(SpriteRenderer source)
    {
        if (!ghosts.TryGetValue(source, out SpriteRenderer ghost) || ghost == null)
        {
            GameObject ghostObject = new GameObject("Character Target Highlight");
            ghostObject.transform.SetParent(source.transform, false);
            ghostObject.transform.localPosition = new Vector3(0f, 0f, depthOffset);
            ghostObject.transform.localRotation = Quaternion.identity;
            ghostObject.transform.localScale = new Vector3(scaleMultiplier, scaleMultiplier, 1f);
            ghost = ghostObject.AddComponent<SpriteRenderer>();
            ghosts[source] = ghost;
        }

        ghost.sprite = source.sprite;
        ghost.flipX = source.flipX;
        ghost.flipY = source.flipY;
        ghost.sortingLayerID = source.sortingLayerID;
        ghost.sortingOrder = source.sortingOrder + sortingOrderOffset;
        ghost.color = highlightColor;
    }

    void RemoveGhost(SpriteRenderer source)
    {
        if (ghosts.TryGetValue(source, out SpriteRenderer ghost) && ghost != null)
        {
            Destroy(ghost.gameObject);
        }

        ghosts.Remove(source);
    }

    static bool IsHealthBarRenderer(SpriteRenderer renderer)
    {
        Transform candidate = renderer.transform;
        while (candidate != null)
        {
            if (candidate.name == "Health Bar")
            {
                return true;
            }

            candidate = candidate.parent;
        }

        return false;
    }
}
