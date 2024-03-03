using Assets.Scripts.Constants;
using Madhur.InfoPopup;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Netcode.Transports.UTP.UnityTransport;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private Transform lobbyPanel;
    [SerializeField] private Transform scorePanel;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private TMP_InputField addressInput;
    [SerializeField] private TMP_InputField portInput;
    [SerializeField] private TMP_InputField localIpAddressText;

    private string ConnectionSettingsFilePath => Path.Combine(Application.dataPath, ConnectionConstants.ConnectionSettingsFileName);
    void Start()
    {
        hostButton.onClick.AddListener(StartHost);
        clientButton.onClick.AddListener(StartClient);

        localIpAddressText.readOnly = true;
        localIpAddressText.text = GetLocalIpAddress();
        var connectionData = LoadConnectionData();
        addressInput.text = connectionData.Address;
        portInput.text = connectionData.Port.ToString();
        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData = connectionData;
    }

    private void StartHost()
    {
        SetConnectionData();

        NetworkManager.Singleton.StartHost();
        Debug.Log("HOST");
        lobbyPanel.gameObject.SetActive(false);
        scorePanel.gameObject.SetActive(true);
    }

    private void StartClient()
    {
        SetConnectionData();

        NetworkManager.Singleton.StartClient();
        Debug.Log("CLIENT");
        lobbyPanel.gameObject.SetActive(false);
        scorePanel.gameObject.SetActive(true);
    }

    private void SetConnectionData()
    {
        var connectionData = new ConnectionAddressData { Address = addressInput.text, Port = ushort.Parse(portInput.text) };

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(connectionData.Address, connectionData.Port, "127.0.0.1");
        SaveConnectionData(connectionData);
    }

    private ConnectionAddressData LoadConnectionData()
    {
        if (!File.Exists(ConnectionSettingsFilePath))
        {
            return new ConnectionAddressData { Address = ConnectionConstants.DefaultConnectionAddress, Port = ConnectionConstants.DefaultPort };
        }
        string json = File.ReadAllText(ConnectionSettingsFilePath);
        return JsonConvert.DeserializeObject<ConnectionAddressData>(json);
    }
    private void SaveConnectionData(ConnectionAddressData connectionData)
    {
        try
        {
            File.WriteAllText(ConnectionSettingsFilePath, JsonConvert.SerializeObject(connectionData));
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
