using UnityEngine;
using TMPro;

public class VVEPlacementCharacterSlot : MonoBehaviour
{
    const string PriceTextObjectName = "Price";

    [SerializeField] PlantPlacementManager placementManager;
    [SerializeField] GameObject selectionIndicator;
    [SerializeField] float selectedScaleMultiplier = 1.2f;
    [SerializeField] Transform iconAnchor;

    VVEDefenderCard defenderCard;
    Vector3 baseScale;
    bool hasBaseScale;
    GameObject[] originalArt;
    GameObject spawnedIconInstance;

    public GameObject CharacterPrefab => defenderCard.defenderType;
    public int Cost => defenderCard.defenderType.GetComponent<VVEDefender>().cost;

    void ApplyIcon(GameObject prefab)
    {
        EnsureOriginalArtCaptured();

        if (spawnedIconInstance != null)
        {
            Destroy(spawnedIconInstance);
            spawnedIconInstance = null;
        }

        // No override requested (e.g. Configure was never called): show the slot's own baked art.
        bool useOriginalArt = prefab == null;
        SetOriginalArtVisible(useOriginalArt);

        Transform parent = iconAnchor != null ? iconAnchor : transform;

        // Instantiate under a scratch object deactivated BEFORE instantiation so Awake/OnEnable
        // never run on the preview's components (Unity defers both until the instance is
        // active in the hierarchy). Some character prefabs have OnEnable-driven visual side
        // effects (gameplay-only state, e.g. staying hidden until placed) that would otherwise
        // still fire even though we disable the components immediately afterward — disabling
        // enabled=false first means OnEnable is skipped for good when it's later parented in.
        GameObject scratchRoot = new GameObject("Icon Instantiation Scratch");
        scratchRoot.SetActive(false);

        spawnedIconInstance = Instantiate(prefab, scratchRoot.transform);
        spawnedIconInstance.name = prefab.name + " Slot Icon";

        spawnedIconInstance.transform.SetParent(parent, false);
        spawnedIconInstance.transform.localPosition = Vector3.zero;
        spawnedIconInstance.transform.localRotation = Quaternion.identity;
        spawnedIconInstance.transform.localScale = Vector3.one;

        Destroy(scratchRoot);
    }

    void EnsureOriginalArtCaptured()
    {
        if (originalArt != null)
        {
            return;
        }

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalArt = new GameObject[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalArt[i] = renderers[i].gameObject;
        }
    }

    void SetOriginalArtVisible(bool visible)
    {
        if (originalArt == null)
        {
            return;
        }

        foreach (GameObject art in originalArt)
        {
            if (art != null)
            {
                art.SetActive(visible);
            }
        }
    }

    void Awake()
    {
        defenderCard = GetComponent<VVEDefenderCard>();
        FreezeSelectorAnimator();
        CaptureBaseScale();
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        CaptureBaseScale();
        transform.localScale = selected ? baseScale * selectedScaleMultiplier : baseScale;
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(selected);
        }
    }

    void CaptureBaseScale()
    {
        if (hasBaseScale)
        {
            return;
        }

        baseScale = transform.localScale;
        hasBaseScale = true;
    }

    void FreezeSelectorAnimator()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = false;
        }
    }

}
