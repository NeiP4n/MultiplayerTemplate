using UnityEngine;
using UnityEngine.SceneManagement;

public static class BootstrapLoader
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureBootstrap()
    {
        if (SceneManager.GetActiveScene().name == "Startup")
            return;

        if (Object.FindFirstObjectByType<PurrNet.NetworkManager>() != null)
            return;

        SceneManager.LoadScene("Startup", LoadSceneMode.Single);
    }
}
