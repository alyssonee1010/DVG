using UnityEngine;

public interface IDVGEnemyLaneWalker
{
    int LaneIndex { get; }
    DVGHealth Health { get; }
    GameObject gameObject { get; }

    void BeginLaneWalk(int laneIndex, Vector3 startPosition, Vector3 endPosition, float speed, int maxHealth);
}
