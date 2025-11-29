using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using System.Linq;

public class AudioManager : MonoBehaviour
{
    public Sound[] music;
    public Sound[] sounds;

    public static AudioManager Instance;

    private Coroutine stopGameBackground;
    private float stopTime = 4f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds.Union(music).ToArray())
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }        

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
        if (arg0.name == "MainMenu" || arg0.name == "RoomSystem")
        {
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
        }
        if (arg0.name == "MultiplayerGame")
        {
            Stop("MenuBackground");
            Play("GameBackground");
        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds.Union(music).ToArray(), sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Play();
    }
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
        float timer = 0f;
        while (timer < stopTime)
        {
            timer += Time.deltaTime;
            s.source.volume = maxVolume * Mathf.Clamp(stopTime - timer, 0f, stopTime) / stopTime;
            yield return null;
        }

        //int n = 1000;
        //float step = 4f / n;
        //for (int i = n; i >= 0; i--)
        //{
        //    s.source.volume = Mathf.Clamp01(s.source.volume - maxVolume / n);
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