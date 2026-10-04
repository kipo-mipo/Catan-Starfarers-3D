using System.Collections;
using UnityEngine;
using MyAssets.Client.Net;
using MyAssets.GameCore;

namespace MyAssets.Client.UI
{
    public sealed class GameUI : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private ClientSessionModel session;
        [SerializeField] private SetupPhasePanel setupPanel;

        private Coroutine _resolveRoutine;

        private void Awake()
        {
            ResolveSceneRefs();
        }

        private void OnEnable()
        {
            _resolveRoutine = StartCoroutine(ResolvePersistentRefsWhenReady());
        }

        private void OnDisable()
        {
            if (_resolveRoutine != null)
            {
                StopCoroutine(_resolveRoutine);
                _resolveRoutine = null;
            }

            UnhookSession();
        }

        private IEnumerator ResolvePersistentRefsWhenReady()
        {
            while (enabled)
            {
                ResolveSceneRefs();

                if (session != null)
                {
                    HookSession();
                    Refresh();
                    yield break;
                }

                yield return null;
            }
        }

        private void ResolveSceneRefs()
        {
            if (session == null)
                session = FindAnyObjectByType<ClientSessionModel>();

            if (setupPanel == null)
                setupPanel = GetComponentInChildren<SetupPhasePanel>(true);
        }

        private void HookSession()
        {
            if (session == null)
                return;

            session.Changed -= Refresh;
            session.ActionRejected -= OnActionRejected;

            session.Changed += Refresh;
            session.ActionRejected += OnActionRejected;
        }

        private void UnhookSession()
        {
            if (session == null)
                return;

            session.Changed -= Refresh;
            session.ActionRejected -= OnActionRejected;
        }

        private void OnActionRejected(ActionRejectedEvent rejected)
        {
            ShowMessage(rejected.Reason);
            Refresh();
        }

        private void Refresh()
        {
            if (session == null)
            {
                ResolveSceneRefs();
                if (session == null)
                    return;
            }

            bool setupActive = session.Phase == MatchPhase.Setup;

            if (setupPanel != null)
            {
                setupPanel.gameObject.SetActive(setupActive);
                if (setupActive)
                    setupPanel.Refresh(session);
            }
        }

        public void ShowMessage(string msg)
        {
            Debug.Log(msg);
        }
    }
}
