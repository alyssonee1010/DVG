using TMPro;
using UnityEngine;

public class VVEButton : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textMeshPro;

    public void SetButtonText(string text)
    {
        textMeshPro.text = text;
    }

}
