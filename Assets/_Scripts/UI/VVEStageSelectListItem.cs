using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One row in the Stage Select list - either a clickable level entry or a plain heading, depending
// on whether `button` is assigned. Visuals (background, font, sizing) are authored on the prefab
// in the Editor; VVEMainMenuController only ever calls Bind() after instantiating a copy.
public class VVEStageSelectListItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI label;
    [SerializeField] Button button;

    public void Bind(string text, UnityEngine.Events.UnityAction onClick)
    {
        if (label != null)
        {
            label.text = text;
        }

        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();

        if (onClick != null)
        {
            button.onClick.AddListener(onClick);
        }
    }
}
