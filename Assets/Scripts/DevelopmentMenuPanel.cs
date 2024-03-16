using Assets.Scripts.Constants;
using Assets.Scripts.Core;
using Madhur.InfoPopup;
using System;
using System.Net;
using TMPro;
using FishNet.Object;
using UnityEngine;
using UnityEngine.UI;

public class DevelopmentMenuPanel : NetworkBehaviour
{
    [SerializeField] private Toggle isHostToggle;
    [SerializeField] private Button connectButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TextMeshProUGUI connectButtonText;
    [SerializeField] private TMP_InputField hostIpAddressInput;
    [SerializeField] private TMP_InputField portInput;
    [SerializeField] private TMP_InputField localIpAddressText;
    void Start()
    {
        connectButton.onClick.AddListener(Connect);
        quitButton.onClick.AddListener(Quit);

        localIpAddressText.readOnly = true;
        localIpAddressText.text = GetLocalIpAddress();
        LoadGameSettings();

        isHostToggle.onValueChanged.AddListener((isHost) => OnHostToggleChanged());
        OnHostToggleChanged();
    }

    private void OnHostToggleChanged()
    {
        if (isHostToggle.isOn)
        {
            hostIpAddressInput.text = ConnectionConstants.DefaultLocalConnectionAddress;
            hostIpAddressInput.gameObject.SetActive(false);
            localIpAddressText.gameObject.SetActive(true);
            connectButtonText.text = "Launch";
        }
        else
        {
            localIpAddressText.gameObject.SetActive(false);
            hostIpAddressInput.gameObject.SetActive(true);
            connectButtonText.text = "Connect";
        }
    }

    private void Connect()
    {
        ApplyGameSettings();
        if (isHostToggle.isOn)
        {
            NetworkManager.ServerManager.StartConnection();
        }
        else
        {
            NetworkManager.ClientManager.StartConnection();
        }
        gameObject.SetActive(false);
    }

    private void Quit()
    {
        Application.Quit();
    }

    private void ApplyGameSettings()
    {
        var gameSettings = new GameSettings
        {
            IsHost = isHostToggle.isOn,
            HostIpAddress = hostIpAddressInput.text,
            Port = ushort.Parse(portInput.text)
        };

        SetConnectionData(gameSettings);
        SaveGameSettings(gameSettings);
    }

    private void SetConnectionData(GameSettings gameSettings)
    {
        var transport = NetworkManager.TransportManager.GetTransport(0);
        transport.SetClientAddress(gameSettings.HostIpAddress);
        transport.SetPort(gameSettings.Port);
        transport.SetServerBindAddress(gameSettings.ListenOn, FishNet.Transporting.IPAddressType.IPv4);
    }

    private void LoadGameSettings()
    {
        GameSettings gameSettings = GameSettingsHelper.GetGameSettings();

        hostIpAddressInput.text = gameSettings.HostIpAddress;
        isHostToggle.isOn = gameSettings.IsHost;
        portInput.text = gameSettings.Port.ToString();
    }
    private void SaveGameSettings(GameSettings gameSettings)
    {
        try
        {
            GameSettingsHelper.SaveGameSettings(gameSettings);
        }
        catch (UnauthorizedAccessException exc)
        {
            InfoPopupUtil.ShowAlert("Please run game with admin rights! We cannot save connection settings...");
            throw exc;
        }
    }

    private string GetLocalIpAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        InfoPopupUtil.ShowAlert("No network adapters with IPv4 address were found in the system!");
        return "Not found";
    }
}
