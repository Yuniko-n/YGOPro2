using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

public class MenuButton : MonoBehaviour
{
    public static MenuButton Instance;

    public MenuButton()
    {
        MenuButton.Instance = this;
    }

    public Canvas buttonCanvas;
    public CanvasGroup buttonCanvasGroup;
    public GameObject button;

    public float Ping;
    private bool IsStart = false;
    private float LastTime = 0;

#if !UNITY_EDITOR && UNITY_ANDROID
    string ver_client = Application.persistentDataPath + "/version_pre.txt";
#else
    string ver_client = "updates/version_pre.txt";
#endif
    string ver_pre = "";

    // Use this for initialization
    void Start()
    {
        buttonCanvas = GetComponent<Canvas>();
        buttonCanvasGroup = GetComponent<CanvasGroup>();
        button = GameObject.Find("Button");
    }

    public void onMenu()
    {
        Program.I().menu.onMenu();
    }

    public void onLoad(bool bStart)
    {
        IsStart = bStart;
        if(IsStart)
        {
            LastTime = Time.time;
        }
        else if(LastTime != 0)
        {
            LastTime = 0;
            onDown();
        }
    }

    private void onRemove()
    {
        List<string> Resource = new List<string>();
        Resource.Add(ver_client);
        Resource.Add("cdb/pre-release.cdb");
        Resource.Add("cdb/pre-update.cdb");
        Resource.Add("cdb/pre-strings.conf");
        Resource.Add("expansions/pre-release.cdb");
        Resource.Add("expansions/pre-update.cdb");
        Resource.Add("expansions/pre-strings.conf");
        Resource.Add("expansions/ygo233.com-pre-release.ypk");
        for(int i = 0; i < Resource.Count; i++)
        {
            if (File.Exists(Resource[i]))
            {
                File.Delete(Resource[i]);
            }
        }
        ver_pre = "";
        Program.I().menu.showToast("先行卡补丁已删除！\n请手动[重启软件]");
    }

    private void onDown()
    {
        try
        {
            HttpDldFile df = new HttpDldFile();
            string url = df.DownloadString("http://dl.ygo2020.link/ygo233/");
            if (File.Exists(ver_client))
            {
                ver_pre = File.ReadAllText(ver_client);
            }
            string ver_server = url.Substring(url.Length - 32, 14);
            if (ver_pre != ver_server)
            {
                string upver = string.Format("{0},{1}", ver_client, ver_server);
#if !UNITY_EDITOR && UNITY_ANDROID //Android
                AndroidJavaObject jo = new AndroidJavaObject("cn.ygopro2.API");
                jo.Call("doDownloadZipFile", url, Program.GAME_PATH, upver);
#else
                if (df.Download(url, "updates/ygosrv233-pre.zip"))
                {
                    Program.I().ExtractZipFile("updates/ygosrv233-pre.zip", Program.GAME_PATH);
                    Program.I().doWriteText(upver);
                    File.Delete("updates/ygosrv233-pre.zip");
                }
#endif
            } else {
                Program.I().menu.showToast("当前[先行卡]已是最新！\n\n(长按按键5秒可移除[先行卡])");
            }
        }
        catch (System.Exception e)
        {
            Program.I().menu.showToast("先行卡更新失败！");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsStart && Ping > 0 && LastTime > 0 && Time.time - LastTime > Ping)
        {
            onRemove();
            IsStart = false;
            LastTime = 0;
        }
        if (Program.I().menu!=null && Program.I().setting !=null && Program.I().menu.isShowed && !Program.I().setting.isShowed)
        {
            buttonCanvasGroup.alpha = 1f;
            button.SetActive(true);
        }
        else
        {
            buttonCanvasGroup.alpha = 0f;
            button.SetActive(false);
        }
    }
}
