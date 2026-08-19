using UnityEngine;

// Rebinds the scene's fixed VVEPlacementCharacterSlot objects to whichever defenders are
// currently in the player's loadout (VVEDefenderUnlocks), in loadout order. Slots beyond the
// loadout size are hidden. Call Rebind() whenever the loadout changes or a level starts.
public class VVEPlacementSlotBinder : MonoBehaviour
{
    [SerializeField] VVEPlacementCharacterSlot[] slots;

    void OnEnable()
    {
        VVEDefenderUnlocks.LoadoutChanged += Rebind;
        Rebind();
    }

    void OnDisable()
    {
        VVEDefenderUnlocks.LoadoutChanged -= Rebind;
    }

    public void Rebind()
    {
        if (slots == null || VVEDefenderCatalog.Instance == null)
        {
            return;
        }

        var loadout = VVEDefenderUnlocks.GetLoadout();
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                continue;
            }

            if (i >= loadout.Count)
            {
                slots[i].gameObject.SetActive(false);
                continue;
            }

            VVEDefenderCatalog.Entry entry = VVEDefenderCatalog.Instance.FindById(loadout[i]);
            if (entry == null || entry.prefab == null)
            {
                slots[i].gameObject.SetActive(false);
                continue;
            }

            slots[i].gameObject.SetActive(true);
        }
    }
}
