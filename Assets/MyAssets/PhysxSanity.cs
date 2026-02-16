using UnityEngine;

public class PhysxSanity : MonoBehaviour
{
    void Start()
    {
        Debug.Log($"Physics.gravity = {Physics.gravity}");
        Debug.Log($"DefaultPhysicsScene valid = {Physics.defaultPhysicsScene.IsValid()}");
        Debug.Log($"Raycast sanity = {Physics.Raycast(new Vector3(0,10,0), Vector3.down, 1000f)}");
    }
}
