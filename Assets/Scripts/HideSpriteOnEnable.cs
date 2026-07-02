using UnityEngine;

public class HideSpriteOnEnable : MonoBehaviour
{
    [SerializeField] bool disableAllChildrenSprites; //Should we also hide all child SpriteRenderers?

    void OnEnable()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = false;
        }

        if (disableAllChildrenSprites)
        {
            SpriteRenderer[] childRenderers = GetComponentsInChildren<SpriteRenderer>();
            for (int i = 0; i < childRenderers.Length; i++)
            {
                childRenderers[i].enabled = false;
            }
        }
    }
}