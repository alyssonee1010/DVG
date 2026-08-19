using TMPro;
using UnityEngine;

public class VVEDefenderCard : MonoBehaviour
{
    public TextMeshPro priceTag;
    public GameObject defenderType;
    public Transform previewContainer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var visualization = Instantiate(defenderType, previewContainer, false);
        visualization.transform.localScale *= 0.67f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
