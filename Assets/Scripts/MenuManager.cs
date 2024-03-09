using Assets.Scripts.Constants;
using Assets.Scripts.Core;
using Madhur.InfoPopup;
using System;
using System.Net;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Toggle isHostToggle;
    [SerializeField] private Button connectButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TextMeshProUGUI connectButtonText;
    [SerializeField] private TMP_InputField hostIpAddressInput;
    [SerializeField] private TMP_InputField portInput;
    [SerializeField] private TMP_InputField localIpAddressText;
    [SerializeField] private Slider soundVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    void Start()
    {
        connectButton.onClick.AddListener(Connect);
        quitButton.onClick.AddListener(Quit);

        localIpAddressText.readOnly = true;
        localIpAddressText.text = GetLocalIpAddress();
        LoadGameSettings();

        isHostToggle.onValueChanged.AddListener(OnHostToggleChanged);
        OnHostToggleChanged(isHostToggle.isOn);

        soundVolumeSlider.onValueChanged.AddListener(OnSoundVolumeChanged);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
    }

    private void OnSoundVolumeChanged(float soundVolume)
    {
        AudioManager.Singleton.Refresh(soundVolume, musicVolumeSlider.value);
    }

    private void OnMusicVolumeChanged(float musicVolume)
    {
        AudioManager.Singleton.Refresh(soundVolumeSlider.value, musicVolume);
    }

    private void OnHostToggleChanged(bool isHost)
    {
        if (isHost)
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
            SceneManager.LoadScene(SceneConstants.PreloadHostSceneIndex);
        }
        else
        {
            SceneManager.LoadScene(SceneConstants.PreloadClientSceneIndex);
        }
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
            Port = ushort.Parse(portInput.text),

            MusicVolume = musicVolumeSlider.value,
            SoundVolume = soundVolumeSlider.value
        };

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(gameSettings.HostIpAddress, gameSettings.Port, gameSettings.ListenOn);
        SaveGameSettings(gameSettings);
    }

    private void LoadGameSettings()
    {
        GameSettings gameSettings = GameSettingsHelper.GetGameSettings();

        hostIpAddressInput.text = gameSettings.HostIpAddress;
        isHostToggle.isOn = gameSettings.IsHost;
        portInput.text = gameSettings.Port.ToString();

        soundVolumeSlider.value = gameSettings.SoundVolume;
        musicVolumeSlider.value = gameSettings.MusicVolume;
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
