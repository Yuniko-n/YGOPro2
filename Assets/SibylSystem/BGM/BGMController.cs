using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using NLayer;// Loads mp3 files

public class BGMController : MonoBehaviour
{
    private bool IsPlaying = false;

    public string soundFilePath;
    public AudioSource audioSource;
    AudioClip audioClip;
    List<string> advantage;
    List<string> deck;
    List<string> disadvantage;
    List<string> duel;
    List<string> lose;
    List<string> menu;
    List<string> win;
    BGMType currentPlaying;
    Coroutine soundRoutine;
    Coroutine soundPlayNext;
    Uri SoundURI;
    public static BGMController Instance;

    public enum BGMType
    {
       none = 0,
       advantage = 1,
       deck = 2,
       disadvantage = 3,
       duel = 4,
       lose = 5,
       menu = 6,
       win = 7
    }

    public enum CHANT
    {
        SUMMON = 0,
        ATTACK = 1,
        ACTIVATE = 2
    }

    public BGMController()
    {
        currentPlaying = BGMType.none;
        BGMController.Instance = this;
        RefreshBGMDir();
    }
    // Use this for initialization
    public void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = 0;
    }

    public void PlayNext()
    {
        IsPlaying = false;
        StartBGM(currentPlaying);
    }

    public void StartBGM(BGMType kind)
    {
        if (currentPlaying == kind && IsPlaying)
            return;

        System.Random rnd = new System.Random();
        int bgmNumber = 0;
        switch (kind)
        {
            case BGMType.advantage:
                if (advantage.Count != 0)
                {
                    bgmNumber = rnd.Next(0, advantage.Count);
                    PlayMusic(advantage[bgmNumber]);
                }
                break;
            case BGMType.deck:
                if (deck.Count != 0)
                {
                    bgmNumber = rnd.Next(0, deck.Count);
                    PlayMusic(deck[bgmNumber]);
                }
                break;
            case BGMType.disadvantage:
                if (disadvantage.Count != 0)
                {
                    bgmNumber = rnd.Next(0, disadvantage.Count);
                    PlayMusic(disadvantage[bgmNumber]);
                }
                break;
            case BGMType.duel:
                if (duel.Count != 0)
                {
                    bgmNumber = rnd.Next(0, duel.Count);
                    PlayMusic(duel[bgmNumber]);
                }
                break;
            case BGMType.lose:
                if (lose.Count != 0)
                {
                    bgmNumber = rnd.Next(0, lose.Count);
                    PlayMusic(lose[bgmNumber]);
                }
                break;
            case BGMType.menu:
                if (menu.Count != 0)
                {
                    bgmNumber = rnd.Next(0, menu.Count);
                    PlayMusic(menu[bgmNumber]);
                }
                break;
            case BGMType.win:
                if (win.Count != 0)
                {
                    bgmNumber = rnd.Next(0, win.Count);
                    PlayMusic(win[bgmNumber]);
                }
                break;
        }

        currentPlaying = kind;
    }

    public void PlaySound(string sound, bool resources = false)
    {
        if (Ocgcore.inSkiping || (!File.Exists(sound) && !resources))
        {
            return;
        }
        if (!resources)
        {
            Uri www = new Uri(new Uri("file:///"), Environment.CurrentDirectory.Replace("\\", "/") + "/" + sound);
            sound = www.ToString();
        }
        GameObject audio_helper = Program.I().ocgcore.create_s(Program.I().mod_audio_effect);
        audio_helper.GetComponent<audio_helper>().play(sound, Program.I().setting.soundValue(), resources);
        Program.I().destroy(audio_helper, 20f);
    }

    public void PlayMusic(string bgmName)
    {
        SoundURI = new Uri(new Uri("file:///"), Environment.CurrentDirectory.Replace("\\", "/") + "/" + bgmName);
        soundFilePath = SoundURI.ToString();
        if (Program.I().setting != null && Program.I().setting.isEnableBGM.value)
        {
            if (soundRoutine != null) { StopCoroutine(soundRoutine); }
            if (soundPlayNext != null) { StopCoroutine(soundPlayNext); }

            if (bgmName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
            {
            #if UNITY_ANDROID || UNITY_IPHONE
                soundRoutine = StartCoroutine(LoadBGM());
            #else
                soundFilePath = bgmName;
                soundRoutine = StartCoroutine(LoadMP3());
            #endif
            } else {
                soundRoutine = StartCoroutine(LoadBGM());
            }
            IsPlaying = true;
        }
    }

    public bool PlayChant(CHANT chant, int code)
    {
        bool SFX = false;
        if (!Program.I().setting.isEnableChantSound.value)
            return SFX;

        if (code == 0 ) return SFX;
        switch (chant)
        {
            case CHANT.SUMMON:
                SFX = PlayChant("summon", code);
                if (!SFX)
                    SFX = PlayChant("summon", YGOSharp.CardsManager.Get(code).Alias);
                break;
            case CHANT.ATTACK:
                SFX = PlayChant("attack", code);
                if (!SFX)
                    SFX = PlayChant("attack", YGOSharp.CardsManager.Get(code).Alias);
                break;
            case CHANT.ACTIVATE:
                SFX = PlayChant("activate", code);
                if (!SFX)
                    SFX = PlayChant("activate", YGOSharp.CardsManager.Get(code).Alias);
                break;
        }
        return SFX;
    }

    public bool PlayChant(string type, int code)
    {
        List<string> chantType = new List<string>();
        chantType.Add(string.Format("sound/chants/{0}/{1}.mp3", type, code));
        chantType.Add(string.Format("sound/chants/{0}/{1}.wav", type, code));
        chantType.Add(string.Format("sound/chants/{0}/{1}.ogg", type, code));
        foreach (string path in chantType)
        {
            if (File.Exists(path))
            {
                if (path.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) && audioClip.name != Path.GetFileName(path))
                {
                    IsPlaying = false;
                    PlayMusic(path);
                }
                else if (path.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) == false)
                {
                    PlaySound(path);
                    return true;
                }
            }
        }

        string chant = string.Format("AudioClip/{0}/{1}.wav", type, code);
        if (Program.AudioClipFile.Contains(chant) && Program.I().setting.isEnableBuiltInSound.value)
        {
            PlaySound(chant, true);
            return true;
        }

        return false;
    }

    public void RefreshBGMDir()
    {
        advantage = new List<string>();
        deck = new List<string>();
        disadvantage = new List<string>();
        duel = new List<string>();
        lose = new List<string>();
        menu = new List<string>();
        win = new List<string>();

        string soundPath = "sound/";
        string bgmPath = soundPath + "BGM/";
        dirPath(soundPath);
        //允许播放的音频格式：.mp3 .ogg .wav
        advantage.AddRange(Directory.GetFiles(string.Concat(bgmPath, "advantage"), "*.*", SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".mp3") || s.EndsWith(".ogg") || s.EndsWith(".wav")));
        deck.AddRange(Directory.GetFiles(string.Concat(bgmPath, "deck"), "*.*", SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".mp3") || s.EndsWith(".ogg") || s.EndsWith(".wav")));
        disadvantage.AddRange(Directory.GetFiles(string.Concat(bgmPath, "disadvantage"), "*.*", SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".mp3") || s.EndsWith(".ogg") || s.EndsWith(".wav")));
        duel.AddRange(Directory.GetFiles(string.Concat(bgmPath, "duel"), "*.*", SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".mp3") || s.EndsWith(".ogg") || s.EndsWith(".wav")));
        lose.AddRange(Directory.GetFiles(string.Concat(bgmPath, "lose"), "*.*", SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".mp3") || s.EndsWith(".ogg") || s.EndsWith(".wav")));
        menu.AddRange(Directory.GetFiles(string.Concat(bgmPath, "menu"), "*.*", SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".mp3") || s.EndsWith(".ogg") || s.EndsWith(".wav")));
        win.AddRange(Directory.GetFiles(string.Concat(bgmPath, "win"), "*.*", SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".mp3") || s.EndsWith(".ogg") || s.EndsWith(".wav")));
    }

    public void dirPath(string path)
    {
        List<string> BGMdir = new List<string>();
        //音乐文件夹
        BGMdir.Add("BGM/advantage/");
        BGMdir.Add("BGM/deck/");
        BGMdir.Add("BGM/disadvantage/");
        BGMdir.Add("BGM/duel/");
        BGMdir.Add("BGM/lose/");
        BGMdir.Add("BGM/menu/");
        BGMdir.Add("BGM/win/");
        BGMdir.Add("chants/attack/");
        BGMdir.Add("chants/activate/");
        BGMdir.Add("chants/summon/");
        //创建文件夹
        foreach (string dir in BGMdir)
        {
            if (!Directory.Exists(Path.GetDirectoryName(path + dir)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path + dir));
            }
        }
    }

    public void changeBGMVol(float vol)
    {
        try
        {
            if (audioSource != null)
            {
                audioSource.volume = vol;
            }
        }
        catch { }
    }

    private IEnumerator LoadBGM()
    {
        WWW request = new WWW(soundFilePath);
        yield return request;
        audioClip = request.GetAudioClip(true, true);
        audioClip.name = Path.GetFileName(soundFilePath);
        PlayAudioFile();
    }

    private IEnumerator LoadMP3()
    {
        yield return null;
        audioClip = Mp3Loader.LoadMp3(soundFilePath);
        audioClip.name = Path.GetFileName(soundFilePath);
        PlayAudioFile();
    }

    private IEnumerator PlayNext(float time)
    {
        yield return new WaitForSeconds(time);

        IsPlaying = false;
        StartBGM(currentPlaying);
    }

    private void PlayAudioFile()
    {
        audioSource.clip = audioClip;
        //audioSource.loop = true;
        audioSource.Play();
        soundPlayNext = StartCoroutine(PlayNext(audioClip.length));
    }

}
