using System;
using System.Linq;
using UnityEngine;

public enum AudioName
{
    BGMMainMenu = 100,
    BGMInGame = 110,
    BGMWin = 120,
    BGMLose = 130,
    SFXMorse = 200,
    SFXMorseCorrect = 260,
    SFXMorseWrong = 261,
    SFXButtonClick = 270,
    SFXWin = 280,
    SFXLose = 290,
}

[Serializable]
public class AudioEntry
{
    public AudioType type;
    public AudioName name;
    public AudioClip clip;
}

public enum AudioType
{
    BGM,
    SFX,
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    [SerializeField] private AudioEntry[] audioClips;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayBGM(params AudioName[] names)
    {
        if (names == null || names.Length == 0)
        {
            bgmSource.Play();
            return;
        }
        AudioName selectedName = names.Length == 1 ? names[0] : names[UnityEngine.Random.Range(0, names.Length)];
        AudioEntry entry = audioClips.FirstOrDefault((clip) => clip.name == selectedName && clip.type == AudioType.BGM);
        if (entry != null)
        {
            bgmSource.clip = entry.clip;
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning($"BGM With Name {selectedName} Not Found");
        }
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PlayOneShotSFX(params AudioName[] names)
    {
        if (names == null || names.Length == 0) return;
        AudioName selectedName = names.Length == 1 ? names[0] : names[UnityEngine.Random.Range(0, names.Length)];
        AudioEntry entry = audioClips.FirstOrDefault((clip) => clip.name == selectedName && clip.type == AudioType.SFX);
        if (entry != null)
        {
            sfxSource.PlayOneShot(entry.clip);
        }
        else
        {
            Debug.LogWarning($"SFX With Name {selectedName} Not Found");
        }
    }
}
