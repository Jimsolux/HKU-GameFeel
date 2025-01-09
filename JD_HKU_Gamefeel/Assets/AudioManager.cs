using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public Sound[] musicSounds, sfxSounds, playerSounds, dashSounds;
    public AudioSource musicSource, sfxSource, playerSource, DashSource;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }
    public void PlayMusic(string name)
    {
        Sound s = Array.Find(musicSounds, x => x.soundName == name);

        if (s == null)
        {
            Debug.Log("Song not found");
        }
        else
        {
            musicSource.clip = s.audioClip;
            musicSource.Play();
        }
    }    
    
    public void PlaySFX(string name)
    {
        Sound s = Array.Find(sfxSounds, x => x.soundName == name);

        if (s == null)
        {
            Debug.Log("Sfx not found");
        }
        else
        {
            if (s.soundName == "Dash")
            {
                sfxSource.pitch = UnityEngine.Random.Range(0.8f, 0.95f);
            }
            else if (s.soundName == "Jump")
            {
                sfxSource.pitch = UnityEngine.Random.Range(0.8f, 1.1f);

            }
            else
            {
                sfxSource.pitch = 1;
            }
            sfxSource.PlayOneShot(s.audioClip);
            //sfxSource.Play();
        }
    }

    public void PlayPlayerSounds(string name)
    {
        Sound s = Array.Find(playerSounds, x => x.soundName == name);

        if (s == null)
        {
            Debug.Log("PlayerSound not found");
        }
        else
        {
            //if (!playerSource.isPlaying || playerSource.clip.name != s.soundName)
            //{
            playerSource.clip = s.audioClip;
            if (name == "Walk") {
                playerSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
                

                float clipLength = playerSource.clip.length;

                float randomTime = UnityEngine.Random.Range(0.95f, clipLength);

                playerSource.time = randomTime;
            }
            else
            {
                playerSource.pitch = 1;
            }
            playerSource.Play();
        }
    }

    public void PlayDash(string name)
    {
        Sound s = Array.Find(dashSounds, x => x.soundName == name);

        if (s == null)
        {
            Debug.Log("Sfx not found");
        }
        else
        {
            if (s.soundName == "Dash")
            {
                DashSource.pitch = UnityEngine.Random.Range(0.8f, 0.95f);
            }
            else if (s.soundName == "Jump")
            {
                DashSource.pitch = UnityEngine.Random.Range(0.85f, 1.15f);

            }
            else
            {
                DashSource.pitch = 1;
            }
            DashSource.PlayOneShot(s.audioClip);
            //sfxSource.Play();
        }
    }

}
