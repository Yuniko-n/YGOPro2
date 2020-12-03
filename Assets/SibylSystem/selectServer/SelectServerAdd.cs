using UnityEngine;
using System;
using System.IO;
using System.Text;

public class SelectServerAdd : WindowServantSP
{
    UIInput inputName;
    UIInput inputIP;
    UIInput inputPort;
    UIInput inputVersion;
    UISprite inputVersion_;

    UIToggle isEnabled;

    public override void initialize()
    {
        createWindow(Program.I().new_ui_selectServerAdd);
        //按键
        UIHelper.registEvent(gameObject, "apply_", onClickApply);
        UIHelper.registEvent(gameObject, "exit_", onClickExit);
        UIHelper.registEvent(gameObject, "enabled_", onEnabled);
        //输入框
        inputName = UIHelper.getByName<UIInput>(gameObject, "name_");
        inputIP = UIHelper.getByName<UIInput>(gameObject, "ip_");
        inputPort = UIHelper.getByName<UIInput>(gameObject, "port_");
        inputVersion = UIHelper.getByName<UIInput>(gameObject, "version_");
        inputVersion_ = UIHelper.getByName<UISprite>(gameObject, "version_");
        //选择框
        isEnabled = UIHelper.getByName<UIToggle>(gameObject, "enabled_");
        //
        inputVersion_.enabled = isEnabled.value;
        UIHelper.getByName<UILabel>(inputVersion.gameObject, "!default").text = "0x" + string.Format("{0:X}", Config.ClientVersion);
        SetActiveFalse();
    }

    public override void show()
    {
        base.show();
    }

    void onClickApply()
    {
        string srvFile = "config/server.conf";
        string version = "default";
        if (inputIP.value == "")
        {
            RMSshow_onlyYes("showToast", InterString.Get("主机地址不能为空"), null);
            return;
        }
        if (inputPort.value == "")
        {
            RMSshow_onlyYes("showToast", InterString.Get("端口不能为空"), null);
            return;
        }
        if (isEnabled.value)
        {
            version = inputVersion.value;
        }
        if (inputName.value == "") inputName.value = inputIP.value;
        if (inputName.value.IndexOf("#") > -1) inputName.value = inputName.value.Replace("#", "-");
        if (inputName.value.IndexOf(":") > -1) inputName.value = inputName.value.Replace(":", "|");

        var lines = Program.I().selectServer.serversList.items;
        for (int i = 0; i < lines.Count; i++)
        {
            if (inputName.value == lines[i])
            {
                RMSshow_onlyYes("showToast", InterString.Get("服务器名称重复，请重新修改！"), null);
                return;
            }
        }

        if (inputIP.value.IndexOf("#") > -1) inputIP.value = inputIP.value.Replace("#", "-");
        if (inputIP.value.IndexOf(":") > -1) inputIP.value = inputIP.value.Replace(":", "|");
        string txt = inputName.value + "#" + inputIP.value + ":" + inputPort.value + "#" + version;
        using (System.IO.StreamWriter file = new System.IO.StreamWriter(srvFile, true))
        {
    　　    file.WriteLine(txt);
        }
		Program.PrintToChat(InterString.Get("服务器添加成功"));
		Program.I().selectServer.serversList.value = inputName.value;
        onClickExit();
    }

    void onClickExit()
    {
        hide();
        Program.I().selectServer.show();
    }

    void onEnabled()
    {
         inputVersion_.enabled = isEnabled.value;
    }
}
