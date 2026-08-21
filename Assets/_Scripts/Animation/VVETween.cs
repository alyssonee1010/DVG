using UnityEngine;
using PrimeTween;

public static class VVETween
{
    public static Tween TweenTo(this Transform transform, Vector3 position, float duration = 0.5f)
    {
        return Tween.Position(transform, new TweenSettings<Vector3>(position, duration));
    }
}
