using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using YGOSharp.OCGWrapper.Enums;

public class selectDeck : WindowServantSP
{

    UIselectableList superScrollView = null;
    UIInput searchInput = null;
    UIDeckPanel deckPanel = null;

    string sort = "sortByTimeDeck";


    cardPicLoader[] quickCards = new cardPicLoader[200];

    public GameObject gameObjectEditCode;
    UIInput inputEditCode;
    UITextList editTextList;
    string editPath;

    public override void initialize()
    {
        float f = Screen.height / 700f;
        gameObjectEditCode = create
            (
            Program.I().editDeckCode,
            Program.camera_main_2d.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height * 2f, 0)),
            new Vector3(0, 0, 0),
            false,
            Program.ui_main_2d,
            true,
            new Vector3(f, f, f)
            );
            
        UIHelper.InterGameObject(gameObjectEditCode);
        UIHelper.registEvent(gameObjectEditCode, "exit_", editExit);
        UIHelper.registEvent(gameObjectEditCode, "save_", editSave);
        UIHelper.registEvent(gameObjectEditCode, "clear_", editClear);
        UIHelper.registEvent(gameObjectEditCode, "copy_", editCopy);
        UIHelper.registEvent(gameObjectEditCode, "paste_", editPaste);
        UIHelper.getByName<UISprite>(gameObjectEditCode, "input_").enabled = false;//暂定
        inputEditCode = UIHelper.getByName<UIInput>(gameObjectEditCode, "input_");
        editTextList = UIHelper.getByName<UITextList>(gameObjectEditCode, "code_");

        createWindow(Program.I().remaster_deckManager);
        deckPanel = gameObject.GetComponentInChildren<UIDeckPanel>();
        UIHelper.registEvent(gameObject, "exit_", onClickExit);
        superScrollView = gameObject.GetComponentInChildren<UIselectableList>();
        superScrollView.selectedAction = onSelected;
        UIHelper.registEvent(gameObject, "sort_", onSort);
        setSortLable();
        UIHelper.registEvent(gameObject, "edit_", onEdit);
        UIHelper.registEvent(gameObject, "new_", onNew);
        UIHelper.registEvent(gameObject, "dispose_", onDispose);
        UIHelper.registEvent(gameObject, "copy_", onCopy);
        UIHelper.registEvent(gameObject, "rename_", onRename);
        UIHelper.registEvent(gameObject, "code_", onCode);
        searchInput = UIHelper.getByName<UIInput>(gameObject, "search_");
        superScrollView.install();
        for (int i = 0; i < quickCards.Length; i++)
        {
            quickCards[i] = deckPanel.createCard();
            quickCards[i].relayer(i);
        }
        SetActiveFalse();

    }

    void editExit()
    {
        gameObjectEditCode.SetActive(false);
        iTween.MoveTo(gameObjectEditCode, Program.camera_main_2d.ScreenToWorldPoint(
            new Vector3(Screen.width + 600, Screen.height / 2, 600)), 1.2f);
    }
    
    void editSave()
    {
        if (inputEditCode.value.Contains("ygo://deck?"))
        {
            Program.PrintToChat(InterString.Get("发现YGOMobile卡组代码，自动转换为YGOPro卡组代码。"));
            string code = inputEditCode.value;
            code = code.Replace("ygo://deck?", "#created by ...");
            code = code.Replace("main=", "\'#main\'");
            code = code.Replace("&extra=", "\'#extra\'");
            code = code.Replace("&side=", "\'!side\'");
            code = code.Replace("'", "\n");

            var lines = code.Replace("\r", "").Split("\n");
            List<string> codeList = new List<string>();
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Substring(lines[i].Length - 2, 1) == "*")
                {
                    int l = Convert.ToInt32(lines[i].Substring(lines[i].Length - 1, 1));
                    for(int s = 0; s < l; s++)
                    {
                        codeList.Add(lines[i].Substring(0, lines[i].Length - 2));
                    }
                }
                else
                {
                    codeList.Add(lines[i]);
                }
            }

            inputEditCode.value = string.Join("\n", codeList.ToArray());
        }
        else if (!inputEditCode.value.Contains("#created by"))
        {
            Program.PrintToChat(InterString.Get("无法识别的卡组代码"));
            return;
        }
        File.WriteAllText(editPath, inputEditCode.value);
        Program.PrintToChat(InterString.Get("已保存：" + editPath));
        editLoad();
    }

    void editClear()
    {
        inputEditCode.value = "";
        editTextList.Clear();
    }

    void editCopy()
    {
        GUIUtility.systemCopyBuffer = inputEditCode.value;
        Program.PrintToChat(InterString.Get("已复制到剪贴板"));
    }

    void editPaste()
    {
        inputEditCode.value = GUIUtility.systemCopyBuffer;
        editLoad();
        Program.PrintToChat(InterString.Get("已从剪贴板导入"));
    }

    void editLoad()
    {
        editTextList.Clear();
        string[] lines = inputEditCode.value.Replace("\r", "").Split("\n");
        for (int i = 0; i < lines.Length; i++)
        {
            editTextList.Add(lines[i]);
        }
    }

    void onSearch()
    {
        printFile();
        superScrollView.toTop();
    }

    void onEdit()
    {
        if (!superScrollView.Selected())
        {
            return;
        }
        if (!isShowed)
        {
            return;
        }
        KF_editDeck(superScrollView.selectedString);
    }

    string preString = "";

    public override void preFrameFunction()
    {
        base.preFrameFunction();
        Menu.checkCommend();
        if (searchInput.value != preString)
        {
            preString = searchInput.value;
            onSearch();
        }
    }

    public void KF_editDeck(string deckName)
    {
        string path = "deck/" + deckName + ".ydk";
        if (File.Exists(path))
        {
            Config.Set("deckInUse", deckName);
            ((DeckManager)Program.I().deckManager).shiftCondition(DeckManager.Condition.editDeck);
            Program.I().shiftToServant(Program.I().deckManager);
            ((DeckManager)Program.I().deckManager).loadDeckFromYDK(path);
            ((CardDescription)Program.I().cardDescription).setTitle(deckName);
            ((DeckManager)Program.I().deckManager).setGoodLooking();
            ((DeckManager)Program.I().deckManager).returnAction =
                () =>
                {
                    if (((DeckManager)Program.I().deckManager).deckDirty)
                    {
                    RMSshow_yesOrNoOrCancle(
                          "deckManager_returnAction"
                        , InterString.Get("要保存卡组的变更吗？")
                        , new messageSystemValue { hint = "yes", value = "yes" }
                        , new messageSystemValue { hint = "no", value = "no" }
                        , new messageSystemValue { hint = "cancle", value = "cancle" }
                        );
                    }
                    else {
                        Program.I().shiftToServant(Program.I().selectDeck);
                    }
                };
        }
    }

    public override void ES_RMS(string hashCode, List<messageSystemValue> result)
    {
        base.ES_RMS(hashCode, result);
        if (hashCode == "deckManager_returnAction")
        {
            if (result[0].value == "yes")
            {
                if (Program.I().deckManager.onSave())
                {
                    Program.I().shiftToServant(Program.I().selectDeck);
                }
            }
            if (result[0].value == "no")
            {
                Program.I().shiftToServant(Program.I().selectDeck);
            }
        }
        if (hashCode == "onNew")
        {
            try
            {
                File.Create("deck/" + result[0].value + ".ydk").Close();
                RMSshow_none(InterString.Get("「[?]」创建完毕。", result[0].value));
                superScrollView.selectedString = result[0].value;
                printFile();
            }
            catch (Exception)
            {
                RMSshow_none(InterString.Get("非法输入！请检查输入的文件名。"));
            }
        }
        if (hashCode == "onDispose")
        {
            if (result[0].value == "yes")
            {
                try
                {
                    File.Delete("deck/" + superScrollView.selectedString + ".ydk");
                    RMSshow_none(InterString.Get("「[?]」删除完毕。", superScrollView.selectedString));
                    printFile();
                }
                catch (Exception)
                {
                    RMSshow_none(InterString.Get("非法删除！"));
                }
            }
        }
        if (hashCode == "onCopy")
        {
            try
            {
                File.Copy("deck/" + superScrollView.selectedString + ".ydk", "deck/" + result[0].value + ".ydk");
                RMSshow_none(InterString.Get("「[?]」复制完毕。", superScrollView.selectedString));
                superScrollView.selectedString = result[0].value;
                printFile();
            }
            catch (Exception)
            {
                RMSshow_none(InterString.Get("非法输入！请检查输入的文件名。"));
            }
        }
        if (hashCode == "onRename")
        {
            try
            {
                File.Move("deck/" + superScrollView.selectedString + ".ydk", "deck/" + result[0].value + ".ydk");
                RMSshow_none(InterString.Get("「[?]」重命名完毕。", superScrollView.selectedString));
                superScrollView.selectedString = result[0].value;
                printFile();
            }
            catch (Exception)
            {
                RMSshow_none(InterString.Get("非法输入！请检查输入的文件名。"));
            }
        }
    }

    void onNew()
    {
        RMSshow_input("onNew", InterString.Get("请输入要创建的卡组名"), UIHelper.getTimeString());
    }

    void onDispose()
    {
        if (!superScrollView.Selected())
        {
            return;
        }
        string path = "deck/" + superScrollView.selectedString + ".ydk";
        if (File.Exists(path))
        {
            RMSshow_yesOrNo(
                          "onDispose"
                        , InterString.Get("确认删除「[?]」吗？", superScrollView.selectedString)
                        , new messageSystemValue { hint = "yes", value = "yes" }
                        , new messageSystemValue { hint = "no", value = "no" }
                        );
        }
    }

    void onCopy()
    {
        if (!superScrollView.Selected())
        {
            return;
        }
        string path = "deck/" + superScrollView.selectedString + ".ydk";
        if (File.Exists(path))
        {
            string newname = InterString.Get("[?]的副本", superScrollView.selectedString);
            string newnamer = newname;
            int i = 1;
            while (File.Exists("deck/" + newnamer + ".ydk"))
            {
                newnamer = newname + i.ToString();
                i++;
            }
            RMSshow_input("onCopy", InterString.Get("请输入复制后的卡组名"), newnamer);
        }
    }

    void onRename()
    {
        if (!superScrollView.Selected())
        {
            return;
        }
        string path = "deck/" + superScrollView.selectedString + ".ydk";
        if (File.Exists(path))
        {
            RMSshow_input("onRename", InterString.Get("新的卡组名"), superScrollView.selectedString);
        }
    }

    void onCode()
    {
        if (!superScrollView.Selected())
        {
            return;
        }
        editPath = "deck/" + superScrollView.selectedString + ".ydk";

        gameObjectEditCode.SetActive(true);

        inputEditCode.value = File.ReadAllText(editPath);
        editLoad();
        iTween.MoveTo(gameObjectEditCode, Program.camera_main_2d.ScreenToWorldPoint(
            new Vector3(Screen.width / 2, Screen.height / 2, 0)), 1.2f);
    }

    private void setSortLable()
    {
        if (Config.Get(sort,"1") == "1")
        {
            UIHelper.trySetLableText(gameObject, "sort_", InterString.Get("时间排序"));
        }
        else
        {
            UIHelper.trySetLableText(gameObject, "sort_", InterString.Get("名称排序"));
        }
    }

    private void onSort()
    {
        if (Config.Get(sort,"1") == "1")
        {
            Config.Set(sort, "0");
        }
        else
        {
            Config.Set(sort, "1");
        }
        setSortLable();
        printFile();
    }


    string deckSelected = "";
    void onSelected()
    {
        if (deckSelected == superScrollView.selectedString)
        {
            onEdit();
        }
        deckSelected = superScrollView.selectedString;
        printSelected();
    }

    private void printSelected()
    {
        GameTextureManager.clearUnloaded();
        YGOSharp.Deck deck;
        DeckManager.FromYDKtoCodedDeck("deck/" + deckSelected + ".ydk", out deck);
        int mainAll = 0;
        int mainMonster = 0;
        int mainSpell = 0;
        int mainTrap = 0;
        int sideAll = 0;
        int sideMonster = 0;
        int sideSpell = 0;
        int sideTrap = 0;
        int extraAll = 0;
        int extraFusion = 0;
        int extraLink = 0;
        int extraSync = 0;
        int extraXyz = 0;
        int currentIndex = 0;

        int[] hangshu = UIHelper.get_decklieshuArray(deck.Main.Count);
        foreach (var item in deck.Main)
        {
            mainAll++;
            YGOSharp.Card c = YGOSharp.CardsManager.Get(item);
            if ((c.Type & (UInt32)CardType.Monster) > 0)
            {
                mainMonster++;
            }
            if ((c.Type & (UInt32)CardType.Spell) > 0)
            {
                mainSpell++;
            }
            if ((c.Type & (UInt32)CardType.Trap) > 0)
            {
                mainTrap++;
            }
            quickCards[currentIndex].reCode(item);
            Vector2 v = UIHelper.get_hang_lieArry(mainAll - 1, hangshu);
            quickCards[currentIndex].transform.localPosition = new Vector3
                (
                -176.3f + UIHelper.get_left_right_indexZuo(0, 352f, (int)v.y, hangshu[(int)v.x],10)
                ,
                161.6f - v.x * 60f
                ,
                0
                );
            if (currentIndex <= 198)
            {
                currentIndex++;
            }
        }
        foreach (var item in deck.Side)
        {
            sideAll++;
            YGOSharp.Card c = YGOSharp.CardsManager.Get(item);
            if ((c.Type & (UInt32)CardType.Monster) > 0)
            {
                sideMonster++;
            }
            if ((c.Type & (UInt32)CardType.Spell) > 0)
            {
                sideSpell++;
            }
            if ((c.Type & (UInt32)CardType.Trap) > 0)
            {
                sideTrap++;
            }
            quickCards[currentIndex].reCode(item);
            quickCards[currentIndex].transform.localPosition = new Vector3
                (
                -176.3f + UIHelper.get_left_right_indexZuo(0, 352f, sideAll - 1, deck.Side.Count,10)
                ,
                -181.1f
                ,
                0
                );
            if (currentIndex <= 198)
            {
                currentIndex++;
            }
        }
        foreach (var item in deck.Extra)
        {
            extraAll++;
            YGOSharp.Card c = YGOSharp.CardsManager.Get(item);
            if ((c.Type & (UInt32)CardType.Fusion) > 0)
            {
                extraFusion++;
            }
            if ((c.Type & (UInt32)CardType.Synchro) > 0)
            {
                extraSync++;
            }
            if ((c.Type & (UInt32)CardType.Xyz) > 0)
            {
                extraXyz++;
            }
            if ((c.Type & (UInt32)CardType.Link) > 0)
            {
                extraLink++;
            }
            quickCards[currentIndex].reCode(item);
            quickCards[currentIndex].transform.localPosition = new Vector3
                (
                -176.3f + UIHelper.get_left_right_indexZuo(0, 352f, extraAll - 1, deck.Extra.Count, 10)
                ,
                -99.199f
                ,
                0
                );
            if (currentIndex <= 198)
            {
                currentIndex++;
            }
        }
        while (true)
        {
            quickCards[currentIndex].clear();
            if (currentIndex <= 198)
            {
                currentIndex++;
            }
            else
            {
                break;
            }
        }
        deckPanel.leftMain.text = GameStringHelper._zhukazu + mainAll;
        deckPanel.leftExtra.text = GameStringHelper._ewaikazu + extraAll;
        deckPanel.leftSide.text = GameStringHelper._fukazu + sideAll;
        deckPanel.rightMain.text = GameStringHelper._guaishou + mainMonster + " "+ GameStringHelper._mofa + mainSpell + " " + GameStringHelper._xianjing + mainTrap;
        deckPanel.rightExtra.text = GameStringHelper._ronghe + extraFusion + " " + GameStringHelper._tongtiao + extraSync + " " + GameStringHelper._chaoliang + extraXyz + " " + GameStringHelper._lianjie + extraLink;
        deckPanel.rightSide.text = GameStringHelper._guaishou + sideMonster + " " + GameStringHelper._mofa + sideSpell + " " + GameStringHelper._xianjing + sideTrap;
    }

    public override void show()
    {
        base.show();
        printFile();
        superScrollView.toTop();
        superScrollView.selectedString = Config.Get("deckInUse", "miaowu");
        printSelected();
        Program.charge();
    }

    public override void hide()
    {
        if (isShowed)
        {
            if (superScrollView.Selected())
            {
                Config.Set("deckInUse", superScrollView.selectedString);
            }
        }
        base.hide();    
    }

    void printFile()
    {
        string deckInUse = Config.Get("deckInUse","miaowu");
        superScrollView.clear();
        FileInfo[] fileInfos = (new DirectoryInfo("deck")).GetFiles();
        if (Config.Get(sort,"1") == "1")
        {
            Array.Sort(fileInfos, UIHelper.CompareTime);
        }
        else
        {
            Array.Sort(fileInfos, UIHelper.CompareName);
        }
        for (int i = 0; i < fileInfos.Length; i++)
        {
            if (fileInfos[i].Name.Length > 4)
            {
                if (fileInfos[i].Name.Substring(fileInfos[i].Name.Length - 4, 4) == ".ydk")
                {
                    if (fileInfos[i].Name.Substring(0, fileInfos[i].Name.Length - 4) == deckInUse)
                    {
                        if (searchInput.value == "" || Regex.Replace(fileInfos[i].Name, searchInput.value, "miaowu", RegexOptions.IgnoreCase) != fileInfos[i].Name)
                        {
                            superScrollView.add(fileInfos[i].Name.Substring(0, fileInfos[i].Name.Length - 4));
                        }
                    }
                }
            }
        }
        for (int i = 0; i < fileInfos.Length; i++)
        {
            if (fileInfos[i].Name.Length > 4)
            {
                if (fileInfos[i].Name.Substring(fileInfos[i].Name.Length - 4, 4) == ".ydk")
                {
                    if (fileInfos[i].Name.Substring(0, fileInfos[i].Name.Length - 4) != deckInUse)
                    {
                        if (searchInput.value == "" || Regex.Replace(fileInfos[i].Name, searchInput.value, "miaowu", RegexOptions.IgnoreCase) != fileInfos[i].Name)
                        {
                            superScrollView.add(fileInfos[i].Name.Substring(0, fileInfos[i].Name.Length - 4));
                        }
                    }
                }
            }
        }
        if (superScrollView.Selected() == false)
        {
            superScrollView.selectTop();
        }
    }

    void onClickExit()
    {
        editExit();
        Program.I().shiftToServant(Program.I().menu);
    }

}
