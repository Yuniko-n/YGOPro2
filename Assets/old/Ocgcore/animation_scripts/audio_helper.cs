using UnityEngine;
using System.Collections;

public class audio_helper : MonoBehaviour {
    public AudioSource audioMgr;
    // Use this for initialization
    void Start () {
    
    }
    
    // Update is called once per frame
    void Update () {
        if (played == false && audioMgr.clip != null && !audioMgr.isPlaying && audioMgr.clip.loadState == AudioDataLoadState.Loaded)
        {
            audioMgr.Play();
            played = true;
        }
    }
    bool played = false;
    public void play(string u, float vol, bool resources = false)
    {
        AudioClip ac;
        played = false;

        if (resources)
        {
            ac = Resources.Load<AudioClip>(u.Substring(0, u.Length - 4));
        }
        else
        {
            WWW www = new WWW(u);
            ac = www.GetAudioClip(true, true);
        }

        audioMgr.clip = ac;
        audioMgr.volume = vol;
    }

    public void change_bgm(string str)
    {
        played = false;
        WWW www = new WWW(str);

        AudioClip ac = www.GetAudioClip(true, true);
        audioMgr.clip = ac;
        audioMgr.loop = true;
    }

    public void close_bgm()
    {
        audioMgr.Pause();
    }

}
