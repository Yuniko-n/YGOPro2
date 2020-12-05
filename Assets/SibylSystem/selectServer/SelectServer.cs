using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

public class SelectServer : WindowServantSP
{
    UIPopupList list;
    public UIPopupList serversList;

    UIInput inputIP;
    UIInput inputPort;
    UIInput inputPsw;
    UIInput inputVersion;

    public UITexture face;

    public override void initialize()
    {
        createWindow(Program.I().new_ui_selectServer);
        UIHelper.registEvent(gameObject, "exit_", onClickExit);
        UIHelper.registEvent(gameObject, "face_", onClickFace);
        UIHelper.registEvent(gameObject, "join_", onClickJoin);
        UIHelper.registEvent(gameObject, "clearPsw_", onClearPsw);
        UIHelper.registEvent(gameObject, "AddItem_", onAddServer);
        UIHelper.registEvent(gameObject, "RmItem_", onRmServer);
        serversList = UIHelper.getByName<UIPopupList>(gameObject, "server");
        //serversList.fontSize = 30;
        UIHelper.registEvent(gameObject, "server", pickServer);
        UIHelper.getByName<UIInput>(gameObject, "name_").value = Config.Get("name", "YGOPro2 User");
        list = UIHelper.getByName<UIPopupList>(gameObject, "history_");
        UIHelper.registEvent(gameObject, "history_", onSelected);
        name = Config.Get("name", "YGOPro2 User");
        inputIP = UIHelper.getByName<UIInput>(gameObject, "ip_");
        inputPort = UIHelper.getByName<UIInput>(gameObject, "port_");
        inputPsw = UIHelper.getByName<UIInput>(gameObject, "psw_");
        inputVersion = UIHelper.getByName<UIInput>(gameObject, "version_");

        inputIP.value = Config.Get("ip_", "s1.ygo233.com");
        inputPort.value = Config.Get("port_", "233");
        serversList.value = Config.Get("serversPicker", "[OCG]233 Server");

        face = UIHelper.getByName<UITexture>(gameObject, "face_");
        face.mainTexture = UIHelper.getFace(name);
        //
        SetActiveFalse();
    }

    void pickServer()
    {
        inputVersion.value = "0x" + String.Format("{0:X}", Config.ClientVersion);
#if !UNITY_EDITOR && UNITY_ANDROID //Android
        var lines = System.Text.Encoding.UTF8.GetString(Program.AssetsFileToByte("serverlist.conf")).Replace("\r", "").Split("\n");
#else
        var lines = File.ReadAllLines(Application.streamingAssetsPath + "/serverlist.conf", Encoding.UTF8);
#endif
        for (int i = 0; i < lines.Length; i++)
        {
            //名称：mats[0]，地址：mats[1]，端口：mats[2]，版本：mats[3]
            string[] mats = lines[i].Split(new char[]{'#', ':'});
            if (serversList.value == mats[0])
            {
                inputIP.value = mats[1];
                inputPort.value = mats[2];
                if (mats[3] != "default")
                {
                    inputVersion.value = mats[3];
                }
                Config.Set("serversPicker", mats[0]);
            }
        }

        if (File.Exists("config/server.conf"))
        {
            lines = File.ReadAllLines("config/server.conf", Encoding.UTF8);
            for (int i = 0; i < lines.Length; i++)
            {
                //名称：mats[0]，地址：mats[1]，端口：mats[2]，版本：mats[3]
                string[] mats = lines[i].Split(new char[]{'#', ':'});
                if (serversList.value == mats[0])
                {
                    inputIP.value = mats[1];
                    inputPort.value = mats[2];
                    if (mats[3] != "default")
                    {
                        inputVersion.value = mats[3];
                    }
                    Config.Set("serversPicker", mats[0]);
                }
            }
        }
    }

    void onSelected()
    {
        if (list != null)
        {
            readString(list.value);
        }
    }

    private void readString(string str)
    {
        str = str.Substring(5, str.Length - 5);
        inputPsw.value = str;
    }

    void onClearPsw()
    {
        string PswString = File.ReadAllText("config/passwords.conf");
        string[] lines = PswString.Replace("\r", "").Split("\n");
        for (int i = 0; i < lines.Length; i++)
        {
            list.RemoveItem(lines[i]);//清空list
        }
        FileStream stream = new FileStream("config/passwords.conf", FileMode.Truncate, FileAccess.ReadWrite);//清空文件内容
        stream.Close();
        inputPsw.value = "";
        Program.PrintToChat(InterString.Get("房间密码已清空"));
    }

    public override void show()
    {
        base.show();
        Program.I().room.RMSshow_clear();
        printList();
        printFile(true);
        Program.charge();
    }

    public override void preFrameFunction()
    {
        base.preFrameFunction();
        Menu.checkCommend();
    }

    void printList()
    {
        serversList.Clear();
#if !UNITY_EDITOR && UNITY_ANDROID //Android
        var lines = System.Text.Encoding.UTF8.GetString(Program.AssetsFileToByte("serverlist.conf")).Replace("\r", "").Split("\n");
#else
        var lines = File.ReadAllLines(Application.streamingAssetsPath + "/serverlist.conf", Encoding.UTF8);
#endif
        for (int i = 0; i < lines.Length; i++)
        {
            //名称：mats[0]，地址：mats[1]，端口：mats[2]，版本：mats[3]
            string[] mats = lines[i].Split(new char[]{'#', ':'});
            serversList.AddItem(mats[0]);
        }

        if (File.Exists("config/server.conf"))
        {
            lines = File.ReadAllLines("config/server.conf", Encoding.UTF8);
            for (int i = 0; i < lines.Length; i++)
            {
                //名称：mats[0]，地址：mats[1]，端口：mats[2]，版本：mats[3]
                string[] mats = lines[i].Split(new char[]{'#', ':'});
                serversList.AddItem(mats[0]);
            }
        }
    }

    void printFile(bool first)
    {
        list.Clear();
        if (File.Exists("config/passwords.conf") == false)
        {
            File.Create("config/passwords.conf").Close();
        }
        string txtString = File.ReadAllText("config/passwords.conf");
        string[] lines = txtString.Replace("\r", "").Split("\n");
        for (int i = 0; i < lines.Length; i++)
        {
            if (i == 0)
            {
                if (first)
                {
                    readString(lines[i]);
                }
            }
            list.AddItem(lines[i]);
        }
    }

    void onAddServer()
    {
        Program.I().shiftToServant(Program.I().selectServerAdd);
    }

    void onRmServer()
    {
#if !UNITY_EDITOR && UNITY_ANDROID //Android
        var lines = System.Text.Encoding.UTF8.GetString(Program.AssetsFileToByte("serverlist.conf")).Replace("\r", "").Split("\n");
#else
        var lines = File.ReadAllLines(Application.streamingAssetsPath + "/serverlist.conf", Encoding.UTF8);
#endif
        for (int i = 0; i < lines.Length; i++)
        {
            //名称：mats[0]，地址：mats[1]，端口：mats[2]，版本：mats[3]
            string[] mats = lines[i].Split(new char[]{'#', ':'});
            if (serversList.value == mats[0])
            {
                Program.PrintToChat(InterString.Get("该服务器为软件自带，无法移除"));
                return;
            }
        }

        List<string> list = new List<string>(File.ReadAllLines("config/server.conf", Encoding.UTF8));
        for (int i = 0; i < list.Count; i++)
        {
            //名称：mats[0]，地址：mats[1]，端口：mats[2]，版本：mats[3]
            string[] mats = list[i].Split(new char[]{'#', ':'});
            if (serversList.value == mats[0])
            {
                list.RemoveAt(i);
                File.WriteAllLines("config/server.conf", list.ToArray());
                serversList.items.Remove(serversList.value);
                Program.PrintToChat(InterString.Get("该服务器已移除"));
            }
        }
        serversList.value = serversList.items[0];
        printList();
    }

    void onClickExit()
    {
        Program.I().shiftToServant(Program.I().menu);
        if (TcpHelper.tcpClient != null)
        {
            if (TcpHelper.tcpClient.Connected)
            {
                TcpHelper.tcpClient.Close();
            }
        }
        save();
    }

    void onClickJoin()
    {
        if (!isShowed)
        {
            return;
        }
        string Name = UIHelper.getByName<UIInput>(gameObject, "name_").value;
        string ipString = UIHelper.getByName<UIInput>(gameObject, "ip_").value;
        string portString = UIHelper.getByName<UIInput>(gameObject, "port_").value;
        string pswString = UIHelper.getByName<UIInput>(gameObject, "psw_").value;
        string versionString = UIHelper.getByName<UIInput>(gameObject, "version_").value;
        KF_onlineGame(Name, ipString, portString, versionString, pswString);
    }

    public void onClickRoomList()
    {
        if (!isShowed)
        {
            return;
        }
        string Name = UIHelper.getByName<UIInput>(gameObject, "name_").value;
        string ipString = UIHelper.getByName<UIInput>(gameObject, "ip_").value;
        string portString = UIHelper.getByName<UIInput>(gameObject, "port_").value;
        string pswString = "L";
        string versionString = UIHelper.getByName<UIInput>(gameObject, "version_").value;
        KF_onlineGame(Name, ipString, portString, versionString, pswString);
    }

    public void onHide(bool Bool)
    {
        gameObject.SetActive(!Bool);
    }

    public void KF_onlineGame(string Name, string ipString, string portString, string versionString, string pswString = "")
    {
        name = Name;
        Config.Set("name", name);
        if (ipString == "" || portString == "" || versionString == "")
        {
            RMSshow_onlyYes("", InterString.Get("非法输入！请检查输入的主机名。"), null);
        }
        else
        {
            if (name != "")
            {
                string fantasty = "psw: " + pswString;
                list.items.Remove(fantasty);
                list.items.Insert(0, fantasty);
                list.value = fantasty;
                if (list.items.Count > 5)
                {
                    list.items.RemoveAt(list.items.Count - 1);
                }
                string all = "";
                for (int i = 0; i < list.items.Count; i++)
                {
                    all += list.items[i] + "\r\n";
                }
                File.WriteAllText("config/passwords.conf", all);
                printFile(false);
                (new Thread(() => { TcpHelper.join(ipString, name, portString, pswString, versionString); })).Start();
            }
            else
            {
                RMSshow_onlyYes("", InterString.Get("昵称不能为空。"), null);
            }
        }
    }

    GameObject faceShow = null;

    public string name = "";

    void onClickFace()
    {
        name = UIHelper.getByName<UIInput>(gameObject, "name_").value;
        face.mainTexture = UIHelper.getFace(name);
        RMSshow_face("showFace", name);
        Config.Set("name", name);
    }

    public void save()
    {
        Config.Set("name", UIHelper.getByName<UIInput>(gameObject, "name_").value);
        Config.Set("ip_", UIHelper.getByName<UIInput>(gameObject, "ip_").value);
        Config.Set("port_", UIHelper.getByName<UIInput>(gameObject, "port_").value);
    }

}
