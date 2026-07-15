using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MyAssets.Client.Input;
using MyAssets.Client.Net;
using MyAssets.GameCore;

namespace MyAssets.Client.UI
{
    public sealed class SetupPhasePanel : MonoBehaviour
    {
        [Header("Text")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text roundText;
        [SerializeField] private TMP_Text currentPlayerText;
        [SerializeField] private TMP_Text instructionText;

        [Header("Round 4 Choices")]
        [SerializeField] private GameObject roundFourChoicesRoot;
        [SerializeField] private Button colonyShipButton;
        [SerializeField] private Button tradeShipButton;
        [SerializeField] private Button freightPodButton;
        [SerializeField] private Button cannonButton;
        [SerializeField] private Button boosterButton;

        [Header("Refs")]
        [SerializeField] private SelectionController selection;

        private void Awake()
        {
            ResolvePersistentRefs();

            if (colonyShipButton != null)
                colonyShipButton.onClick.AddListener(() => SetShipType(SetupShipType.ColonyShip));

            if (tradeShipButton != null)
                tradeShipButton.onClick.AddListener(() => SetShipType(SetupShipType.TradeShip));

            if (freightPodButton != null)
                freightPodButton.onClick.AddListener(() => SetUpgrade(UpgradeType.FreightPod));

            if (cannonButton != null)
                cannonButton.onClick.AddListener(() => SetUpgrade(UpgradeType.Cannon));

            if (boosterButton != null)
                boosterButton.onClick.AddListener(() => SetUpgrade(UpgradeType.Booster));
        }

        private void OnEnable()
        {
            ResolvePersistentRefs();
        }

        public void Refresh(ClientSessionModel session)
        {
            ResolvePersistentRefs();

            if (session == null)
                return;

            if (titleText != null)
                titleText.text = "Setup Phase";

            if (roundText != null)
                roundText.text = $"Round: {GetRoundName(session.CurrentSetupRound)}";

            if (currentPlayerText != null)
                currentPlayerText.text = $"Current Player: {session.CurrentSetupPlayerId}";

            if (instructionText != null)
                instructionText.text = GetInstruction(session);

            bool showRoundFourChoices =
                session.CurrentSetupRound == SetupRound.SpaceportShipUpgrade &&
                session.IsLocalSetupTurn;

            if (roundFourChoicesRoot != null)
                roundFourChoicesRoot.SetActive(showRoundFourChoices);
        }

        private void ResolvePersistentRefs()
        {
            if (selection == null)
                selection = FindAnyObjectByType<SelectionController>();
        }

        private void SetShipType(SetupShipType shipType)
        {
            ResolvePersistentRefs();

            if (selection != null)
                selection.SetSetupShipType(shipType);
            else
                Debug.LogWarning("[SetupPhasePanel] No SelectionController found. Put one on persistent NetworkRoot.");
        }

        private void SetUpgrade(UpgradeType upgrade)
        {
            ResolvePersistentRefs();

            if (selection != null)
                selection.SetSetupUpgrade(upgrade);
            else
                Debug.LogWarning("[SetupPhasePanel] No SelectionController found. Put one on persistent NetworkRoot.");
        }

        private static string GetRoundName(SetupRound round)
        {
            return round switch
            {
                SetupRound.FirstColony => "First Colony",
                SetupRound.SecondColony => "Second Colony",
                SetupRound.ThirdColony => "Third Colony",
                SetupRound.SpaceportShipUpgrade => "Spaceport, Ship, and Upgrade",
                _ => round.ToString()
            };
        }

        private static string GetInstruction(ClientSessionModel session)
        {
            if (!session.IsLocalSetupTurn)
                return $"Waiting for Player {session.CurrentSetupPlayerId}.";

            return session.CurrentSetupRound switch
            {
                SetupRound.FirstColony => "Place your first colony.",
                SetupRound.SecondColony => "Place your second colony.",
                SetupRound.ThirdColony => "Place your third colony.",
                SetupRound.SpaceportShipUpgrade => "Choose ship/upgrade, click one of your colonies to upgrade, then click a starting ship location.",
                _ => "Complete setup."
            };
        }
    }
}
