using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using System.Linq;

public class AudioManager : MonoBehaviour
{
    public Sound[] music;
    public Sound[] sounds;

    public static AudioManager Instance;

    private Coroutine stopGameBackground;

    //public bool soundPlaying = true;
    //public bool gaming = false;
    //private string currentBackground;


    //public bool IsBackgroundPlaying(string name)
    //{
    //    Sound s = Array.Find(backgrounds, sound => sound.name == name);
    //    if (s == null)
    //    {
    //        Debug.LogWarning("Sound: " + name + " not found!");
    //        return false;
    //    }
    //    return s.source.isPlaying;
    //}

    //public bool BackgroundPlaying()
    //{
    //    foreach (Sound s in backgrounds)
    //    {
    //        if (s.source.isPlaying)
    //        {
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    //public void BackgroundPlay(string name)
    //{
    //    Sound s = Array.Find(backgrounds, sound => sound.name == name);
    //    if (s == null)
    //    {
    //        Debug.LogWarning("Sound: " + name + " not found!");
    //        return;
    //    }
    //    s.source.Play();
    //}

    //public void BackgroundStopAll()
    //{
    //    foreach (Sound s in backgrounds)
    //    {
    //        s.source.Stop();
    //    }
    //}

    //private void Update()
    //{
    //    if (!BackgroundPlaying() && !AudioListener.pause)
    //    {
    //        //Debug.Log("yes");
    //        if (gaming)
    //        {
    //            int index = UnityEngine.Random.Range(1, backgrounds.Length);
    //            while (currentBackground == backgrounds[index].name)
    //            {
    //                index = UnityEngine.Random.Range(1, backgrounds.Length);
    //            }
    //            currentBackground = backgrounds[index].name;
    //            BackgroundPlay(currentBackground);
    //        }
    //        else
    //        {
    //            BackgroundPlay("mainmenu");
    //        }
    //    }
    //}

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            //GP_Logger.Info("DESTROY AUDIO");
            //Debug.Log("AudioManager already exists");
            //gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        //foreach (Sound s in music)
        //{
        //    s.source = gameObject.AddComponent<AudioSource>();
        //    s.source.clip = s.clip;

        //    s.source.volume = s.volume;
        //    s.source.pitch = s.pitch;
        //    s.source.loop = s.loop;
        //}
        foreach (Sound s in sounds.Union(music).ToArray())
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
        //foreach (Sound s in backgrounds)
        //{
        //    //Debug.Log(s.name);
        //    s.source = gameObject.AddComponent<AudioSource>();
        //    s.source.clip = s.clip;

        //    s.source.volume = s.volume;
        //    s.source.pitch = s.pitch;
        //    s.source.loop = s.loop;
        //}
        

        SceneManager.sceneLoaded += SceneLoaded;
    }

    private Sound[] GetAudio(Audio audio)
    {
        switch (audio)
        {
            case Audio.Music:
                return music;
            case Audio.Sound:
                return sounds;
        }
        return null;
    }

    //private float GetMultiplier(Audio audio)
    //{
    //    switch (audio)
    //    {
    //        case Audio.Music:
    //            return musicMultiplier;
    //        case Audio.Sound:
    //            return soundMultiplier;
    //    }
    //    return 0f;
    //}

    //private void UpdateAllVolumes()
    //{
    //    UpdateVolumes(Audio.Music);
    //    UpdateVolumes(Audio.Sound);
    //}

    public void UpdateVolumes(Audio audio, float multiplier)
    {
        foreach (Sound s in GetAudio(audio))
        {
            s.source.volume = s.volume * multiplier;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
    }

    private void SceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        //switch (arg0.name)
        //{
        //    case "MainMenu":
        //        float initMusicMultiplier = 1f - PlayerPrefs.GetFloat("musicSilencer");
        //        float initSoundMultiplier = 1f - PlayerPrefs.GetFloat("soundSilencer");

        //        musicSlider.value = initMusicMultiplier;
        //        soundSlider.value = initSoundMultiplier;
        //    case "RoomSystem":
        //        Stop("GameBackground");
        //        Sound s = Array.Find(sounds, sound => sound.name == "MenuBackground");
        //        if (s == null)
        //        {
        //            Debug.LogWarning("Sound: " + name + " not found!");
        //            return;
        //        }
        //        if (s.source != null && !s.source.isPlaying)
        //            Play("MenuBackground");
        //        break;
        //    case "MultiplayerGame":
        //        Stop("MenuBackground");
        //        Play("GameBackground");
        //        break;
        //}

        if (arg0.name == "MainMenu" || arg0.name == "RoomSystem")
        {
            //Stop("GameBackground");
            if (stopGameBackground != null)
            {
                StopCoroutine(stopGameBackground);

                Sound sound = Array.Find(music, sound => sound.name == "GameBackground");
                if (sound != null)
                {
                    sound.source.volume = sound.volume;
                }
            }
            StopAllBut("MenuBackground");
            Sound s = Array.Find(music, sound => sound.name == "MenuBackground");
            if (s == null)
            {
                Debug.LogWarning("Sound: MenuBackground not found!");
                return;
            }
            if (s.source != null && !s.source.isPlaying)
                Play("MenuBackground");

            //if (arg0.name == "MainMenu")
            //{

            //}
        }
        if (arg0.name == "MultiplayerGame")
        {
            Stop("MenuBackground");
            Play("GameBackground");
        }
    }

    //private void Start()
    //{

    //}

    public void Play(string name)
    {
        //Debug.Log($"Playing sound {name}");
        Sound s = Array.Find(sounds.Union(music).ToArray(), sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Play();
    }

    //public bool IsPlaying(string name)
    //{
    //    Sound s = Array.Find(sounds, sound => sound.name == name);
    //    if (s == null)
    //    {
    //        Debug.LogWarning("Sound: " + name + " not found!");
    //        return false;
    //    }
    //    return s.source.isPlaying;
    //}

    public void Pause(string name)
    {
        Sound s = Array.Find(sounds.Union(music).ToArray(), sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Pause();
    }

    public void Stop(string name)
    {
        Sound s = Array.Find(sounds.Union(music).ToArray(), sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        if (s.source != null && s.source.isPlaying)
        {
            //Debug.Log($"Stop coroutine {s.name}");
            //StopCoroutine(stopGameBackground);
            s.source.Stop();
        }
    }

    public void GraduallyStop(string name)
    {
        Sound s = Array.Find(sounds.Union(music).ToArray(), sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        stopGameBackground = StartCoroutine(StopAudioWaiter(s));
    }

    IEnumerator StopAudioWaiter(Sound s)
    {
        float maxVolume = s.source.volume;
        int n = 1000;
        float step = 4f / n;
        for (int i = n; i >= 0; i--)
        {
            s.source.volume = Mathf.Clamp01(s.source.volume - maxVolume / n);
            //Debug.Log(s.source.volume);
            yield return new WaitForSeconds(step);
        }
        //while (s.volume > 0f)
        //{
        //    s.volume = Mathf.Clamp(s.volume - step, 0f, 1f);
        //    yield return new WaitForSeconds(step);
        //}
        Stop(s.name);
        Debug.Log("Stopped");
        s.source.volume = maxVolume;
    }

    public void StopAll()
    {
        foreach (Sound s in sounds.Union(music).ToArray())
        {
            s.source.Stop();
        }
    }

    public void StopAllBut(string name)
    {
        foreach (Sound s in sounds.Union(music).ToArray())
        {
            if (s.name == name) continue;
            s.source.Stop();
        }
    }
}

public enum Audio
{
    Music,
    Sound
}