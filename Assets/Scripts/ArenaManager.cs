using Assets.Scripts.Dto;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

enum RestartState
{
    Round,
    Game
}

public class ArenaManager : NetworkBehaviour
{
    [SerializeField] private Canvas gameCanvas;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private int victoryScore = 3;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TextMeshProUGUI victoryText;
    [SerializeField] private Button restartButton;
    public static ArenaManager Singleton { get; private set; }
    private List<Player> activePlayers = new List<Player>();
    private List<PlayerScore> allPlayerScores = new List<PlayerScore>();
    private void Awake()
    {
        var instances = FindObjectsByType<ArenaManager>(FindObjectsSortMode.None);
        if (instances.Length > 1)
        {
            Singleton = instances[0];
            if (IsServer)
            {
                Destroy(gameObject);
            }
            return;
        }
        Singleton = this;
        restartButton.onClick.AddListener(() => RestartServerRpc(RestartState.Game));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RestartServerRpc(RestartState state)
    {
        RestartClientRpc(state);
    }
    [ClientRpc]
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

    [ClientRpc]
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
        if (activePlayers.Count <= 1 && allPlayerScores.Count > 1)
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
    public void PlayerDiedServerRpc(ulong deadPlayerOwnerId, ulong killerOwnerId)
    {
        PlayerDiedClientRpc(deadPlayerOwnerId, killerOwnerId);
        UpdateArenaStateClientRpc();
    }
    [ClientRpc]
    private void PlayerDiedClientRpc(ulong deadPlayerOwnerId, ulong killerOwnerId)
    {
        var killerScore = allPlayerScores.First(p => p.Player.OwnerClientId == killerOwnerId);
        if (deadPlayerOwnerId == killerOwnerId)
        {
            killerScore.Score--;
        }
        else
        {
            killerScore.Score++;
        }
        activePlayers.RemoveAll(p => p.OwnerClientId == deadPlayerOwnerId);
    }
}
