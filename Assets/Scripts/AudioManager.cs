using Assets.Scripts.Constants;
using Assets.Scripts.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoSingleton<AudioManager>
{
    private float _soundVolume = 0;
    private float _musicVolume = 0;
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioClip menuMusicClip;
    [SerializeField] private AudioClip gameMusicClip;

    override protected void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        Refresh();
        LoadSceneMusic(SceneManager.GetActiveScene());
        musicAudioSource.clip = menuMusicClip;
        musicAudioSource.loop = true;
        musicAudioSource.Play();
    }

    private void SceneManager_activeSceneChanged(Scene previousScene, Scene currentScene)
    {
        LoadSceneMusic(currentScene);
    }

    private void LoadSceneMusic(Scene currentScene)
    {
        if (currentScene.buildIndex == SceneConstants.MenuSceneIndex)
        {
            musicAudioSource.clip = menuMusicClip;
            musicAudioSource.PlayDelayed(1f);
        }
        else if (currentScene.buildIndex == SceneConstants.GameSceneIndex)
        {
            musicAudioSource.clip = gameMusicClip;
            musicAudioSource.PlayDelayed(1f);
        }
        else
        {
            musicAudioSource.Stop();
            musicAudioSource.clip = null;
        }
    }

    public void PlaySound(AudioClip clip, Vector2 position)
    {
        AudioSource.PlayClipAtPoint(clip, position, _soundVolume);
    }

    public void Refresh()
    {
        GameSettings gameSettings = GameSettingsHelper.GetGameSettings();
        Refresh(gameSettings.SoundVolume, gameSettings.MusicVolume);
    }
    public void Refresh(float soundVolume, float musicVolume)
    {
        _soundVolume = soundVolume;
        _musicVolume = musicVolume;

        musicAudioSource.volume = _musicVolume;
    }
}
