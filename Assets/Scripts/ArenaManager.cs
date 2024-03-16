using Assets.Scripts.Constants;
using Assets.Scripts.Core;
using Assets.Scripts.Dto;
using FishNet;
using FishNet.Object;
using Madhur.InfoPopup;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

enum RestartState
{
    Round,
    Game
}

public class ArenaManager : NetworkSingleton<ArenaManager>
{
    [SerializeField] private Canvas gameCanvas;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private int victoryScore = 3;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TextMeshProUGUI victoryText;
    [SerializeField] private Button restartButton;

    private List<Player> activePlayers = new List<Player>();
    private List<PlayerScore> allPlayerScores = new List<PlayerScore>();
    override protected void Awake()
    {
        base.Awake();
        restartButton.onClick.AddListener(() => RestartServerRpc(RestartState.Game));
        if (NetworkManager != null)
        {
            NetworkManager.ClientManager.OnClientTimeOut += ClientManager_OnClientTimeOut;
        }
    }

    private void ClientManager_OnClientTimeOut()
    {
        InfoPopupUtil.ShowAlert("Connection was lost...");
        Invoke(nameof(LoadMenuScene), 2f);
    }

    private void OnDestroy()
    {
        NetworkManager.ClientManager.OnClientTimeOut -= ClientManager_OnClientTimeOut;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LoadMenuScene();
        }
    }

    private void LoadMenuScene()
    {
        var loadData = new FishNet.Managing.Scened.SceneLoadData(SceneConstants.MenuSceneName);
        loadData.ReplaceScenes = FishNet.Managing.Scened.ReplaceOption.All;
        InstanceFinder.SceneManager.LoadConnectionScenes(Player.LocalInstance.Owner, loadData);

        InstanceFinder.SceneManager.OnLoadEnd += SceneManager_OnLoadEnd;
    }

    private void SceneManager_OnLoadEnd(FishNet.Managing.Scened.SceneLoadEndEventArgs obj)
    {
        obj.LoadedScenes.Any(ls => ls.name == SceneConstants.MenuSceneName);
        OnMenuSceneLoaded();
    }

    private void OnMenuSceneLoaded()
    {
        Debug.Log("Disconnected from server");
        NetworkManager.ClientManager.Connection.Disconnect(true);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RestartServerRpc(RestartState state)
    {
        RestartClientRpc(state);
    }
    [ObserversRpc]
    private void RestartClientRpc(RestartState state)
    {
        victoryPanel.SetActive(false);
        activePlayers = allPlayerScores.Select(ps => ps.Player).ToList();
        if (state == RestartState.Game)
        {
            allPlayerScores.ForEach(ps => ps.Score = 0);
        }
        UpdateArenaScoreText();
        activePlayers.ForEach(p => p.Spawn());
    }

    private void UpdateArenaScoreText()
    {
        scoreText.text = string.Join("       ", allPlayerScores.Select(ps => $"{ps.Player.GetName()} - {ps.Score}"));
    }

    [ObserversRpc]
    private void UpdateArenaStateClientRpc()
    {
        UpdateArenaScoreText();

        var winner = allPlayerScores.FirstOrDefault(p => p.Score >= victoryScore);
        if (winner != null)
        {
            winner.Player.DisableControl();
            victoryPanel.SetActive(true);
            victoryText.text = $"{winner.Player.GetName()} won!";
            return;
        }
        if (activePlayers.Count <= 1)
        {
            RestartClientRpc(RestartState.Round);
        }
    }

    public void AddPlayer(Player player)
    {
        activePlayers.Add(player);
        allPlayerScores.Add(new PlayerScore { Player = player });
        UpdateArenaScoreText();
    }
    public void RemovePlayer(Player player)
    {
        activePlayers.Remove(player);
        allPlayerScores.RemoveAll(ps => ps.Player == player);
        UpdateArenaScoreText();
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerDiedServerRpc(int deadPlayerOwnerId, int killerOwnerId)
    {
        PlayerDiedClientRpc(deadPlayerOwnerId, killerOwnerId);
        UpdateArenaStateClientRpc();
    }
    [ObserversRpc]
    private void PlayerDiedClientRpc(int deadPlayerOwnerId, int killerOwnerId)
    {
        var killerScore = allPlayerScores.First(p => p.Player.OwnerId == killerOwnerId);
        if (deadPlayerOwnerId == killerOwnerId)
        {
            killerScore.Score--;
        }
        else
        {
            killerScore.Score++;
        }
        activePlayers.RemoveAll(p => p.OwnerId == deadPlayerOwnerId);
    }
}
