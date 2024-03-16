using FishNet.Object;
using UnityEngine;
using Assets.Scripts.Core;
using FishNet;

public class GameLoader : NetworkBehaviour
{
    public void Awake()
    {
        var gameSettings = GameSettingsHelper.GetGameSettings();
        if (gameSettings.IsHost)
        {
            LoadHost();
        }
        else
        {
            LoadClient();
        }
    }

    private void LoadHost()
    {
        InstanceFinder.ServerManager.StartConnection();
        InstanceFinder.ClientManager.StartConnection();
        Debug.Log("Host");
    }
    private void LoadClient()
    {
        InstanceFinder.ClientManager.StartConnection();
        Debug.Log("Client");
    }
}
