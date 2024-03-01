using UnityEngine;
using UnityEngine.SceneManagement;

public class ProjectSceneManager : MonoBehaviour
{
    private void Start()
    {
        SceneManager.LoadScene("GameScene");
    }

}
