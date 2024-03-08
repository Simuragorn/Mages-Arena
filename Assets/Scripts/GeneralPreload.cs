using Assets.Scripts.Constants;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GeneralPreload : MonoBehaviour
{
    public void Awake()
    {
        SceneManager.LoadScene(SceneConstants.MenuSceneIndex, LoadSceneMode.Single);
    }
}
