using Assets.Scripts.Constants;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostPreload : NetworkBehaviour
{
    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.LoadScene(SceneConstants.GameSceneIndex, LoadSceneMode.Single);
        StartCoroutine(InitializeGameScene());
    }

    IEnumerator InitializeGameScene()
    {
        while (SceneManager.GetActiveScene().buildIndex != SceneConstants.GameSceneIndex ||
            !SceneManager.GetActiveScene().isLoaded)
        {
            yield return null;
        }
        NetworkManager.Singleton.StartHost();
        Debug.Log("Host");
        if (IsServer)
        {
            Destroy(gameObject);
        }
    }
}
