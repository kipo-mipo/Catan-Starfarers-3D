using System.Collections.Generic;
using MyAssets.GameCore;
using UnityEngine;

namespace MyAssets.Client.BoardView
{
    public sealed class TradeSectorView : MonoBehaviour
    {
        [Header("Species Visual Roots")]
        [SerializeField] private GameObject speciesMerchantsVisual;
        [SerializeField] private GameObject speciesGreenfolkVisual;
        [SerializeField] private GameObject speciesScientistsVisual;
        [SerializeField] private GameObject speciesDiplomatsVisual;
        [SerializeField] private GameObject speciesTravelersVisual;

        [Header("Rotation")]
        [Tooltip("Optional. Put trade visual anchors under this if you want the layout to rotate without rotating the prefab root.")]
        [SerializeField] private Transform rotator;

        [Tooltip("Matches PlanetSectorView's default orientation. Set to 0 if your trade prefab was authored opposite.")]
        [SerializeField] private float baseYRotationDegrees = 180f;

        [Tooltip("Applied to SlotOrientation.Up/delta slots.")]
        [SerializeField] private float deltaOrientationYRotationOffsetDegrees = 180f;

        [Header("Visual Upright Correction")]
        [Tooltip("Keeps the trade species icon upright/readable even when the sector layout rotates.")]
        [SerializeField] private bool keepSpeciesVisualUpright = true;

        private readonly Dictionary<Transform, Quaternion> authoredLocalRotations = new();

        public void Apply(SectorPieceDef def, int rotation = 0)
        {
            Apply(def, rotation, SlotOrientation.Down);
        }

        public void Apply(SectorPieceDef def, int rotation, SlotOrientation slotOrientation)
        {
            if (def.Type != SectorPieceType.Trade)
            {
                Debug.LogError($"{name} got non-trade sector {def.Type}.");
                return;
            }

            float appliedYRotationDegrees = baseYRotationDegrees
                + (slotOrientation == SlotOrientation.Up ? deltaOrientationYRotationOffsetDegrees : 0f)
                + rotation * 120f;

            var rotationTarget = rotator != null ? rotator : transform;
            rotationTarget.localRotation = Quaternion.Euler(0f, appliedYRotationDegrees, 0f);

            SetAll(false);

            switch (def.TradeSpecies)
            {
                case TradeSpecies.SpeciesMerchants:
                    Activate(speciesMerchantsVisual, def, appliedYRotationDegrees);
                    break;

                case TradeSpecies.SpeciesGreenfolk:
                    Activate(speciesGreenfolkVisual, def, appliedYRotationDegrees);
                    break;

                case TradeSpecies.SpeciesScientists:
                    Activate(speciesScientistsVisual, def, appliedYRotationDegrees);
                    break;

                case TradeSpecies.SpeciesDiplomats:
                    Activate(speciesDiplomatsVisual, def, appliedYRotationDegrees);
                    break;

                case TradeSpecies.SpeciesTravelers:
                    Activate(speciesTravelersVisual, def, appliedYRotationDegrees);
                    break;

                default:
                    Debug.LogError($"Trade sector {def.Id.Value} has no valid species: {def.TradeSpecies}.");
                    break;
            }
        }

        private void SetAll(bool active)
        {
            SetActive(speciesMerchantsVisual, active);
            SetActive(speciesGreenfolkVisual, active);
            SetActive(speciesScientistsVisual, active);
            SetActive(speciesDiplomatsVisual, active);
            SetActive(speciesTravelersVisual, active);
        }

        private static void SetActive(GameObject visual, bool active)
        {
            if (visual != null)
                visual.SetActive(active);
        }

        private void Activate(GameObject visual, SectorPieceDef def, float appliedYRotationDegrees)
        {
            if (visual == null)
            {
                Debug.LogError($"Missing visual root for trade sector {def.Id.Value} / {def.TradeSpecies}.");
                return;
            }

            var t = visual.transform;
            if (!authoredLocalRotations.TryGetValue(t, out var authoredRotation))
            {
                authoredRotation = t.localRotation;
                authoredLocalRotations[t] = authoredRotation;
            }

            if (keepSpeciesVisualUpright)
            {
                // The layout rotates to match the slot's Up/Down orientation, but the sprite art stays readable.
                // Preserve any prefab-authored local rotation, such as X = -90 for flat sprites.
                t.localRotation = Quaternion.Euler(0f, -appliedYRotationDegrees, 0f) * authoredRotation;
            }
            else
            {
                t.localRotation = authoredRotation;
            }

            visual.SetActive(true);
        }
    }
}
