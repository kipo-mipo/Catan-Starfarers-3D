using MyAssets.GameCore;
using TMPro;
using UnityEngine;

namespace MyAssets.Client.BoardView
{
    public sealed class TokenDisplayView : MonoBehaviour
    {
        [Header("Text")]
        [SerializeField] private TMP_Text tokenText;

        [Header("Optional Visual States")]
        [SerializeField] private GameObject hiddenVisual;
        [SerializeField] private GameObject assignedVisual;
        [SerializeField] private GameObject specialVisual;

        private void Awake()
        {
            if (tokenText == null)
                tokenText = GetComponentInChildren<TMP_Text>(true);
        }

        public void SetHidden()
        {
            if (tokenText != null)
                tokenText.text = "?";

            SetActive(hiddenVisual, true);
            SetActive(assignedVisual, false);
            SetActive(specialVisual, false);
        }

        public void SetToken(PlanetTokenState tokenState)
        {
            if (tokenState.State == TokenState.Hidden || tokenState.TokenId == null)
            {
                SetHidden();
                return;
            }

            if (tokenText != null)
                tokenText.text = GetDisplayText(tokenState);

            bool isSpecial = tokenState.State == TokenState.SpecialTokenPresent;
            SetActive(hiddenVisual, false);
            SetActive(assignedVisual, !isSpecial);
            SetActive(specialVisual, isSpecial);
        }

        private static string GetDisplayText(PlanetTokenState tokenState)
        {
            string rollText;

            if (tokenState.RollA == null)
                rollText = "?";
            else if (tokenState.RollB != null)
                rollText = $"{tokenState.RollA.Value}/{tokenState.RollB.Value}";
            else
                rollText = tokenState.RollA.Value.ToString();

            if (tokenState.State == TokenState.SpecialTokenPresent)
                return $"req {rollText}";

            return rollText;
        }

        private static void SetActive(GameObject go, bool active)
        {
            if (go != null)
                go.SetActive(active);
        }
    }
}
