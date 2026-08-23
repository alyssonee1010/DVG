using System;
using PrimeTween;
using UnityEngine;

public class VVETweenAnimation : MonoBehaviour
{
    public TweenSettings<Vector3> command;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.position = command.endValue;
        Tween.Position(transform, command);
    }
}

[Serializable]
public class VVETweenCommand
{
    public Transform transform;
    public TweenSettings settings;
}