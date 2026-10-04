using System.Collections.Generic;
using MyAssets.GameCore;
using UnityEngine;
using UnityEngine.Serialization;

namespace MyAssets.Client.BoardView
{
    public sealed class PlanetSectorView : MonoBehaviour
    {
        [System.Serializable]
        public sealed class PlanetSlotView
        {
            public Transform planetAnchor;
            public Transform tokenAnchor;
        }

        [Header("Rotation")]
        [SerializeField] private Transform rotator;
        [SerializeField] private float baseYRotationDegrees = 180f;
        [SerializeField] private float deltaOrientationYRotationOffsetDegrees = 180f;

        [Header("Planet Slots")]
        [SerializeField] private List<PlanetSlotView> planetSlots = new();

        [Header("Planet Prefabs")]
        [SerializeField] private GameObject orePlanetPrefab;
        [SerializeField] private GameObject carbonPlanetPrefab;
        [SerializeField] private GameObject foodPlanetPrefab;
        [SerializeField] private GameObject goodsPlanetPrefab;
        [FormerlySerializedAs("gasPlanetPrefab")]
        [SerializeField] private GameObject fuelPlanetPrefab;

        [Header("Token Prefab")]
        [SerializeField] private GameObject tokenDisplayPrefab;

        [Header("Visual Upright Correction")]
        [Tooltip("Keeps spawned planet and token visuals upright/readable even when the sector layout rotates.")]
        [SerializeField] private bool keepSpawnedVisualsUpright = true;

        public void Apply(SectorPieceDef def, int rotation)
        {
            Apply(def, rotation, SlotOrientation.Down, null, null);
        }

        public void Apply(SectorPieceDef def, int rotation, SlotOrientation slotOrientation)
        {
            Apply(def, rotation, slotOrientation, null, null);
        }

        public void Apply(
            SectorPieceDef def,
            int rotation,
            SlotOrientation slotOrientation,
            SlotId? slotId,
            IReadOnlyDictionary<PlacedPlanetKey, PlanetTokenState> tokenStates)
        {
            Clear();

            float appliedYRotationDegrees = baseYRotationDegrees
                + (slotOrientation == SlotOrientation.Up ? deltaOrientationYRotationOffsetDegrees : 0f)
                + rotation * 120f;

            if (rotator != null)
            {
                rotator.localRotation = Quaternion.Euler(
                    0f,
                    appliedYRotationDegrees,
                    0f
                );
            }

            foreach (var planet in def.Planets)
            {
                if (planet.IndexOnPiece < 0 || planet.IndexOnPiece >= planetSlots.Count)
                {
                    Debug.LogError($"Planet index {planet.IndexOnPiece} is outside slot count on {name}.");
                    continue;
                }

                var slot = planetSlots[planet.IndexOnPiece];

                var planetPrefab = GetPlanetPrefab(planet.Resource);
                if (planetPrefab != null && slot.planetAnchor != null)
                {
                    var planetObject = Instantiate(planetPrefab, slot.planetAnchor);
                    KeepVisualUpright(planetObject, appliedYRotationDegrees);
                }

                if (tokenDisplayPrefab != null && slot.tokenAnchor != null)
                {
                    var tokenObject = Instantiate(tokenDisplayPrefab, slot.tokenAnchor);
                    KeepVisualUpright(tokenObject, appliedYRotationDegrees);
                    var tokenView = tokenObject.GetComponentInChildren<TokenDisplayView>(true);
                    if (tokenView == null)
                        tokenView = tokenObject.AddComponent<TokenDisplayView>();

                    if (TryGetTokenState(slotId, tokenStates, planet.IndexOnPiece, out var tokenState))
                        tokenView?.SetToken(tokenState);
                    else
                        tokenView?.SetHidden();
                }
            }
        }

        private static bool TryGetTokenState(
            SlotId? slotId,
            IReadOnlyDictionary<PlacedPlanetKey, PlanetTokenState> tokenStates,
            int planetIndex,
            out PlanetTokenState tokenState)
        {
            tokenState = default;

            if (slotId == null || tokenStates == null)
                return false;

            return tokenStates.TryGetValue(new PlacedPlanetKey(slotId.Value, planetIndex), out tokenState);
        }

        private void KeepVisualUpright(GameObject visualRoot, float appliedYRotationDegrees)
        {
            if (!keepSpawnedVisualsUpright || visualRoot == null)
                return;

            // The anchors/layout rotate with the sector, but the sprite/token art should stay readable.
            // Preserve any prefab-authored local rotation, such as X = -90 for flat sprites.
            visualRoot.transform.localRotation =
                Quaternion.Euler(0f, -appliedYRotationDegrees, 0f) * visualRoot.transform.localRotation;
        }

        private GameObject GetPlanetPrefab(ResourceType resource)
        {
            return resource switch
            {
                ResourceType.Ore => orePlanetPrefab,
                ResourceType.Carbon => carbonPlanetPrefab,
                ResourceType.Food => foodPlanetPrefab,
                ResourceType.Goods => goodsPlanetPrefab,
                ResourceType.Fuel => fuelPlanetPrefab,
                _ => null
            };
        }

        private void Clear()
        {
            foreach (var slot in planetSlots)
            {
                ClearChildren(slot.planetAnchor);
                ClearChildren(slot.tokenAnchor);
            }
        }

        private static void ClearChildren(Transform parent)
        {
            if (parent == null) return;

            for (int i = parent.childCount - 1; i >= 0; i--)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Object.DestroyImmediate(parent.GetChild(i).gameObject);
                else
                    Object.Destroy(parent.GetChild(i).gameObject);
#else
                Object.Destroy(parent.GetChild(i).gameObject);
#endif
            }
        }
    }
}
