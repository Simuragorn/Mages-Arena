using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private Transform lobbyPanel;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    void Start()
    {
        hostButton.onClick.AddListener(StartHost);
        clientButton.onClick.AddListener(StartClient);
    }

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        Debug.Log("HOST");
        lobbyPanel.gameObject.SetActive(false);
    }
    
    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        Debug.Log("CLIENT");
        lobbyPanel.gameObject.SetActive(false);
    }
}
