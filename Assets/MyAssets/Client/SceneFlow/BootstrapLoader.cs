using UnityEngine;
using UnityEngine.SceneManagement;

namespace MyAssets.Client.SceneFlow
{
    /// <summary>
    /// Loads the persistent ship scene immediately after Bootstrap.
    /// Keep Bootstrap tiny: it exists only to instantiate NetworkRoot.
    /// </summary>
    public sealed class BootstrapLoader : MonoBehaviour
    {
        [SerializeField] private string shipSceneName = "Game";

        private void Start()
        {
            if (!SceneManager.GetSceneByName(shipSceneName).isLoaded)
                SceneManager.LoadScene(shipSceneName, LoadSceneMode.Additive);
        }
    }
}
