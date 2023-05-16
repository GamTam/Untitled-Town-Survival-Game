using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioMixerGroup group;
    [SerializeField] private List<Music> allMusic;
    
    public static MusicManager instance;

    [HideInInspector] public Music musicPlaying = null;

    public static float currentPoint = 0;

    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (Globals.MusicManager != null)
        {
            Destroy(gameObject);
            return;
        }
        Globals.MusicManager = this;

        Dictionary<string, ArrayList> musicDict = new Dictionary<string, ArrayList>();
        musicDict = Globals.LoadTSV("Music Data");

        foreach(KeyValuePair<string, ArrayList> entry in musicDict) {
            if (entry.Key == "" || entry.Key[0] == '#' || (string) entry.Value[0] == "Intro Point") continue;
            Music music = new Music();
                
            music.name = entry.Key;
            
            try
            {
                music.loopStart = Convert.ToSingle(entry.Value[0]);
                music.loopEnd = Convert.ToSingle(entry.Value[1]);
            }
            catch
            {
                music.redirect = Convert.ToString(entry.Value[2]);
            }

            String path = "Music/" + music.name;

            music.source = gameObject.AddComponent<AudioSource>();
            music.source.clip = Resources.Load(path) as AudioClip;
            music.source.pitch = music.pitch;
            music.source.loop = true;
            music.source.outputAudioMixerGroup = group;

            allMusic.Add(music);
        }
    }

    public void setPoint()
    {
        currentPoint = musicPlaying.source.time;
    }

    public void goToPoint()
    {
        musicPlaying.source.time = currentPoint;
    }
    
    public void Stop(Music song=null)
    {
        if (song == null) song = musicPlaying;
        
        try { song?.source.Stop(); }
        catch {}

        if (song == musicPlaying) musicPlaying = null;
    }

    public Music PlayRandom()
    {
        System.Random rand = new System.Random();

        Music s = allMusic.ElementAt(rand.Next(0, allMusic.Count));

        return Play(s);
    }

    public Music Play(string name, float point=0, bool dontStopPreviousSong=false)
    {
        
        Music s = allMusic.Find(x => x.name == name);
        if (s == null) return null;

        if (!string.IsNullOrEmpty(s.redirect)) return Play(s.redirect);
        return Play(s, point, dontStopPreviousSong);
    }
    
    public Music Play (Music s, float point=0, bool dontStopPreviousSong=false)
    {
        if (musicPlaying == s && Math.Abs(musicPlaying.source.volume - 1) < 0.1 && musicPlaying.source.isPlaying)
        {
            return musicPlaying;
        }

        try
        {
            if (musicPlaying.source.isPlaying && !dontStopPreviousSong)
            {
                fadeOut();
            }
        } catch {}

        musicPlaying = s;

        s.source.volume = 1;
        s.source.time = point;
        s.source.Play();

        return s;
    }

    private void Update()
    {
        if (musicPlaying == null) return;
        
        if (musicPlaying.name != "")
        {
            if (musicPlaying.source.time > musicPlaying.loopEnd && musicPlaying.loopEnd != -1)
            {
                musicPlaying.source.time -= (musicPlaying.loopEnd - musicPlaying.loopStart);
            }
        }
    }
    
    public void FadeVariation(string song, float duration = 1f)
    {
        Music music = musicPlaying;
        StartCoroutine(fadeTo(duration, 0, music));
        Play(song, musicPlaying.source.time, true);
        musicPlaying.source.volume = 0;
        StartCoroutine(fadeTo(duration, 1, musicPlaying));
    }

    public void fadeOut(float length=0.1f)
    {
        try
        {
            setPoint();
        }
        catch
        {
            return;
        }

        StartCoroutine(fadeTo(length, 0, musicPlaying));
    }
    
    public void fadeIn(float length=0.1f)
    {
        musicPlaying.source.volume = 0f;
        musicPlaying.source.Play();
        goToPoint();
        StartCoroutine(fadeTo(length, 1, musicPlaying));
    }
    
    public IEnumerator fadeTo(float duration, float targetVolume, Music audioSource=null)
    {
        if (audioSource == null)
        {
            audioSource = musicPlaying;
        }
        
        float currentTime = 0;
        float start = audioSource.source.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.source.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }

        audioSource.source.volume = targetVolume;

        if (audioSource.source.volume <= 0.1)
        {
            Stop(audioSource);
        }
    }

    public void SetLowpass(float duration, float targetValue)
    {
        StartCoroutine(FadeLowpassFilter(duration, targetValue));
    }

    public IEnumerator FadeLowpassFilter(float duration, float targetValue)
    {
        float currentTime = 0;
        float start;
        group.audioMixer.GetFloat("Music Lowpass", out start);

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            @group.audioMixer.SetFloat("Music Lowpass", Mathf.Lerp(start, targetValue, currentTime / duration));
            yield return null;
        }

        @group.audioMixer.SetFloat("Music Lowpass", targetValue);
    }

    public Music GetMusicPlaying() {
        return musicPlaying;
    }

    public void setMusicPlaying(Music music)
    {
        musicPlaying = music;
    }
}
