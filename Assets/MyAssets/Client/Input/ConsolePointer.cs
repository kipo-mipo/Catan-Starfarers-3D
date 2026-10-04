using UnityEngine;
using UnityEngine.EventSystems;

public class ConsoleRayPointer : MonoBehaviour
{
    [Header("Ray")]
    public Camera cam;
    public float maxDistance = 5f;
    public LayerMask uiBlockers = ~0;

    [Header("Pointer")]
    public RectTransform reticle; // optional UI reticle on screen
    public KeyCode clickKey = KeyCode.Mouse0;

    readonly PointerEventData _pointerData = new PointerEventData(null);

    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
        // Unity UI needs an EventSystem in the scene.
        if (EventSystem.current == null) return;

        // Aim at center of screen
        _pointerData.position = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        // Hover + click are handled by UI modules if your Canvas has GraphicRaycaster
        // and EventSystem is present. We just ensure a raycaster exists on the camera.
        // (PhysicsRaycaster on camera helps for certain setups, but GraphicRaycaster handles UI.)

        // Optional: you can drive a reticle color change with a Physics ray hit for feedback
        if (reticle)
        {
            bool hit = Physics.Raycast(cam.transform.position, cam.transform.forward, maxDistance, uiBlockers);
            reticle.localScale = hit ? Vector3.one * 1.1f : Vector3.one;
        }

        // Clicking: let the normal EventSystem handle it (mouse click works).
        // If you later want controller input, switch clickKey to a mapped input action.
        if (Input.GetKeyDown(clickKey))
        {
            // nothing needed; UI system processes the click this frame.
        }
    }
}