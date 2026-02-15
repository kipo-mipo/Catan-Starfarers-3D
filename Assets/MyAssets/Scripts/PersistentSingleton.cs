using UnityEngine;

public class PersistentSingleton : MonoBehaviour
{
    static PersistentSingleton instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
