using UnityEngine;

public class PhysicsProbe : MonoBehaviour
{
    public BoardNode targetNode; // drag BoardNode(0) here in inspector
    public float radius = 0.5f;

    void Start()
    {
        if (targetNode == null)
        {
            Debug.LogError("[PhysicsProbe] targetNode not assigned");
            return;
        }

        var col = targetNode.GetComponent<Collider>();
        Debug.Log($"[PhysicsProbe] Node name={targetNode.name} id={targetNode.nodeId} " +
                  $"pos={targetNode.transform.position} layer={LayerMask.LayerToName(targetNode.gameObject.layer)} " +
                  $"collider={(col ? col.GetType().Name : "NULL")} enabled={(col && col.enabled)}");

        Vector3 p = targetNode.transform.position;

        // Overlap using EVERYTHING + collide triggers
        var hits = Physics.OverlapSphere(p, radius, ~0, QueryTriggerInteraction.Collide);
        Debug.Log($"[PhysicsProbe] OverlapSphere @ {p} r={radius} hits={hits.Length}");
        foreach (var h in hits)
            Debug.Log($" - hit {h.name} layer={LayerMask.LayerToName(h.gameObject.layer)}");

        Debug.Log($"[PhysicsProbe] ClosestPoint = {col.ClosestPoint(p)}");

        var s = gameObject.scene;
        Debug.Log($"[PhysicsProbe] Unity scene: {s.name} valid={s.IsValid()} loaded={s.isLoaded}");
        var ps = s.GetPhysicsScene();
        Debug.Log($"[PhysicsProbe] PhysicsScene valid={ps.IsValid()}");

        var all = FindObjectsByType<Collider>(FindObjectsSortMode.None);
        Debug.Log($"[PhysicsProbe] Total colliders found by FindObjects: {all.Length}");
        Debug.Log($"[PhysicsProbe] Physics.OverlapSphereNonAlloc sanity: " +
          $"{Physics.OverlapSphereNonAlloc(transform.position, 1000f, new Collider[2048])}");

        var n = GameObject.Find("BoardNode (0)");
        var spcol = n.GetComponent<SphereCollider>();
        Debug.Log($"Node world pos={n.transform.position} bounds center={spcol.bounds.center} extents={spcol.bounds.extents}");

    }
}
