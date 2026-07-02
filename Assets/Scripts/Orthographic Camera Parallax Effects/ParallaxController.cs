using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Finds all of the gameObjects that have a ParallaxLayer.cs script, and moves them!*/

public class ParallaxController : MonoBehaviour
{
    public delegate void ParallaxCameraDelegate(float cameraPositionChangeX, float cameraPositionChangeY);
    public ParallaxCameraDelegate onCameraMove;
    private Vector2 oldCameraPosition;
    List<ParallaxLayer> parallaxLayers = new List<ParallaxLayer>();
    [SerializeField] private Camera targetCamera;

    void Start()
    {
        if(!Camera.main) targetCamera = GameObject.Find("Camera").GetComponent<Camera>();
        onCameraMove += MoveLayer;
        FindLayers();
        oldCameraPosition.x = targetCamera.transform.position.x;
        oldCameraPosition.y = targetCamera.transform.position.y;
    }

    private void FixedUpdate()
    {
        if (targetCamera.transform.position.x != oldCameraPosition.x || (targetCamera.transform.position.y) != oldCameraPosition.y)
        {
            if (onCameraMove != null)
            {
                Vector2 cameraPositionChange;
                cameraPositionChange = new Vector2(oldCameraPosition.x - targetCamera.transform.position.x, oldCameraPosition.y - targetCamera.transform.position.y);
                onCameraMove(cameraPositionChange.x, cameraPositionChange.y);
            }

            oldCameraPosition = new Vector2(targetCamera.transform.position.x, targetCamera.transform.position.y);
        }
    }

    //Finds all the objects that have a ParallaxLayer component, and adds them to the parallaxLayers list.
    void FindLayers()
    {
        parallaxLayers.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            ParallaxLayer layer = transform.GetChild(i).GetComponent<ParallaxLayer>();

            if (layer != null)
            {
                parallaxLayers.Add(layer);
            }
        }
    }

    //Move each layer based on each layers position. This is being used via the ParallaxLayer script
    void MoveLayer(float positionChangeX, float positionChangeY)
    {
        foreach (ParallaxLayer layer in parallaxLayers)
        {
            layer.MoveLayer(positionChangeX, positionChangeY);
        }
    }
}