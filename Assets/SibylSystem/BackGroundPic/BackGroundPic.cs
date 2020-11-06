using UnityEngine;
using System;
using System.IO;
public class BackGroundPic : Servant
{
    GameObject backGround;
    public override void initialize()
    {
        backGround = create(Program.I().mod_simple_ngui_background_texture, Vector3.zero, Vector3.zero, false, Program.ui_back_ground_2d);
        string path = "texture/common/";
        LoadPic();
        if (File.Exists(path + "bg.png"))
        {
            LoadJpgOrPng(path + "bg.png");
        }
        else if (File.Exists(path + "desk.png"))
        {
            LoadJpgOrPng(path + "desk.png");
        }
        else if (File.Exists(path + "bg.jpg"))
        {
            LoadJpgOrPng(path + "bg.jpg");
        }
        else if (File.Exists(path + "desk.jpg"))
        {
            LoadJpgOrPng(path + "desk.jpg");
        }
    }

    public void LoadPic()
    {
        Texture2D pic = (Texture2D)Resources.Load("bg_menu");
        backGround.GetComponent<UITexture>().mainTexture = pic;
    }

    public void LoadJpgOrPng(string fileName)
    {
        backGround.GetComponent<UITexture>().mainTexture = UIHelper.getTexture2D(fileName);
        backGround.GetComponent<UITexture>().depth = -100;
    }

    public override void applyShowArrangement()
    {
        UIRoot root = Program.ui_back_ground_2d.GetComponent<UIRoot>();
        float s = root.activeHeight / Screen.height;
        var tex = backGround.GetComponent<UITexture>().mainTexture;
        float ss = (float)tex.height / (float)tex.width;
        int width = (int)(Screen.width * s);
        int height = (int)(width * ss);
        if (height < Screen.height)
        {
            height = (int)(Screen.height * s);
            width = (int)(height / ss);
        }
        backGround.GetComponent<UITexture>().height = height+2;
        backGround.GetComponent<UITexture>().width = width+2;
    }

    public override void applyHideArrangement()
    {
        applyShowArrangement();
    }
}
