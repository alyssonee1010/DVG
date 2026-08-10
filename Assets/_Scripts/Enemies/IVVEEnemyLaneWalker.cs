using UnityEngine;

public interface IVVEEnemyLaneWalker
{
    int LaneIndex { get; }
    VVEHealth Health { get; }
    GameObject gameObject { get; }

    void BeginLaneWalk(int laneIndex, Vector3 startPosition, Vector3 endPosition, float speed, int maxHealth);
}
