using UnityEngine;

public static class TransformExtensions
{
    public static void DestroyChildren(this Transform transform)
    {
        // Loop backward to safely avoid index shifting bugs
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            // Access the child by index and destroy its GameObject
            Object.Destroy(transform.GetChild(i).gameObject);
        }
    }
}