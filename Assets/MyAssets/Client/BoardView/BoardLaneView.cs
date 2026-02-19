using UnityEngine;

public class BoardLaneView : MonoBehaviour
{
    public int laneId;
    public int nodeAId;
    public int nodeBId;

    public bool Connects(int a, int b) =>
        (nodeAId == a && nodeBId == b) || (nodeAId == b && nodeBId == a);
}