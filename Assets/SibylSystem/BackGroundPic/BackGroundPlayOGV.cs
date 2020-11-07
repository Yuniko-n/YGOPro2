using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

public class BackGroundPlayOGV : MonoBehaviour
{
    GameObject backGround;

    MovieTexture movieTexture;

    string bgFilePath;

    public static BackGroundPlayOGV Instance;

    public BackGroundPlayOGV()
    {
        BackGroundPlayOGV.Instance = this;
    }

    public void LoadOGV(GameObject bg, string fileName)
    {
        backGround = bg;
        Uri fileURI = new Uri(new Uri("file:///"), Environment.CurrentDirectory.Replace("\\", "/") + "/" + fileName);
        bgFilePath = fileURI.ToString();

        StartCoroutine(PlayOGV());
    }

    IEnumerator PlayOGV()
    {
        WWW request = new WWW(bgFilePath);
        movieTexture = request.GetMovieTexture();

        while(!movieTexture.isReadyToPlay)
            yield return request;

        backGround.GetComponent<UITexture>().mainTexture = movieTexture;
        backGround.GetComponent<UITexture>().depth = -100;

        movieTexture.loop = true;
        movieTexture.Play();
    }
}