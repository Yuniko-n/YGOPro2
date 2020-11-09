using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Video;

public class BackGroundPlayMP4 : MonoBehaviour
{
    GameObject backGround;

    VideoPlayer videoPlayer;

    public static BackGroundPlayMP4 Instance;

    public BackGroundPlayMP4()
    {
        BackGroundPlayMP4.Instance = this;
    }

    public void LoadMP4(GameObject bg, string fileName)
    {
        backGround = bg;
        Uri fileURI = new Uri(new Uri("file:///"), Environment.CurrentDirectory.Replace("\\", "/") + "/" + fileName);
        string bgFilePath = fileURI.ToString();

        videoPlayer = backGround.AddComponent<VideoPlayer>();
        //Android 10+ 无法使用“file:///”
        if (Program.APIVersion < 29)
        {
            videoPlayer.url = bgFilePath;
        }
        else
        {
            HttpListenerApp.HttpProvider.Init("http://localhost:2020/Download/", fileName);
            videoPlayer.url = "http://localhost:2020/Download/" + fileName;
        }
        videoPlayer.isLooping = true;
        videoPlayer.Play();

        StartCoroutine(PlayMP4());
    }

    IEnumerator PlayMP4()
    {
        yield return new WaitForSeconds(1f);//延时播放，否则会黑屏

        backGround.GetComponent<UITexture>().mainTexture = backGround.GetComponent<VideoPlayer>().texture;
        backGround.GetComponent<UITexture>().depth = -100;

        if (Program.APIVersion < 29 == false) HttpListenerApp.HttpProvider.httpFiledownload.Stop();
    }

}