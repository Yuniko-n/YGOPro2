using System;
using System.Collections.Generic;
using System.Linq;

public class RoomList : WindowServantSP
{
    UIselectableList superScrollView = null;
    List<string[]> listOfRooms = new List<string[]>();
    bool hideAI;
    bool hideStarted;
    UILabel roomPSWLabel;
    public override void initialize()
    {
        createWindow(Program.I().new_ui_RoomList);
        UIHelper.registEvent(gameObject, "exit_", onClickExit);
        UIHelper.registEvent(gameObject, "refresh_", onRefresh);
        //UIHelper.getByName(gameObject, "join").SetActive(false);
        UIHelper.registEvent(gameObject, "join_", onJoin);
        roomPSWLabel = UIHelper.getByName<UILabel>(gameObject, "roomNameLabel");
        hideAI =UIHelper.getByName<UIToggle>(gameObject, "hideAIrooms_").value = UIHelper.fromStringToBool(Config.Get("hideAIrooms_", "1"));
        hideStarted=UIHelper.getByName<UIToggle>(gameObject, "hideStarted_").value = UIHelper.fromStringToBool(Config.Get("hideStarted_", "1"));
        UIHelper.registEvent(gameObject, "hideAIrooms_", save);
        UIHelper.registEvent(gameObject, "hideStarted_", save);
        superScrollView = gameObject.GetComponentInChildren<UIselectableList>();
        superScrollView.selectedAction = onSelected;
        superScrollView.install();
        SetActiveFalse();
    }

    private void save()
    {
        hideAI = UIHelper.getByName<UIToggle>(gameObject, "hideAIrooms_").value;
        hideStarted  = UIHelper.getByName<UIToggle>(gameObject, "hideStarted_").value;
        Config.Set("hideAIrooms_", UIHelper.fromBoolToString(UIHelper.getByName<UIToggle>(gameObject, "hideAIrooms_").value));
        Config.Set("hideStarted_", UIHelper.fromBoolToString(UIHelper.getByName<UIToggle>(gameObject, "hideStarted_").value));
    }

    private void onRefresh()
    {
        Program.I().selectServer.onClickRoomList();
    }

    public void UpdateList(List<string[]> roomList)
    {
        show();
        listOfRooms.Clear();
        listOfRooms.AddRange(roomList);
        printFile();
        Program.I().selectServer.onHide(true);
    }
    public void onClickExit()
    {
        hide();
        Program.I().selectServer.onHide(false);
    }
    public override void hide()
    {
        roomPSWLabel.text = "";
        base.hide();

    }

    private void printFile()
    {
        superScrollView.clear();
        superScrollView.toTop();
        if (hideAI)
        {
            listOfRooms.RemoveAll(s => s[11].Contains("AI"));
        }
        if (hideStarted)
        {
            listOfRooms.RemoveAll(s => Convert.ToInt32(s[10]) != 0);
        }
        listOfRooms.TrimExcess();
        listOfRooms = listOfRooms.OrderBy(s => s[3]).ToList();
        foreach (string[] room in listOfRooms)
        {
            superScrollView.add(room[9]);
        }
    }

    string selectedString = string.Empty;

    void onJoin()
    {
        if (roomPSWLabel.text != "")
        {
            onSelected();
        }
        else
        {
            Program.PrintToChat(InterString.Get("请选择房间！！！"));
        }
    }

    void onSelected()
    {
        if (!isShowed)
        {
            return;
        }
        string roomPsw;
        if (selectedString == superScrollView.selectedString)
        {
            roomPsw= listOfRooms.Find(s => s[9] == selectedString)[2];
            JoinRoom(superScrollView.selectedString,roomPsw);
            return;
        }
        selectedString = superScrollView.selectedString;
        roomPsw= listOfRooms.Find(s => s[9] == selectedString)[2];
        if (roomPsw != null)
        {
            roomPSWLabel.text = roomPsw;
        }
        else
        {
            roomPSWLabel.text = "";
        }
    }

    void JoinRoom(string selectedString,string roomPsw)
    {
        string Name = UIHelper.getByName<UIInput>(Program.I().selectServer.gameObject, "name_").value;
        string ipString = UIHelper.getByName<UIInput>(Program.I().selectServer.gameObject, "ip_").value;
        string portString = UIHelper.getByName<UIInput>(Program.I().selectServer.gameObject, "port_").value;
        string pswString = roomPsw;
        string versionString = UIHelper.getByName<UIInput>(Program.I().selectServer.gameObject, "version_").value;
        Program.I().roomList.hide();
        Program.I().selectServer.KF_onlineGame(Name, ipString, portString, versionString, pswString);
    }
}
