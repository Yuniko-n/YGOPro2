using UnityEngine;
using System;
using System.Data;
using System.IO;
using Mono.Data.Sqlite;

public class AppUpdateLog
{
#if !UNITY_EDITOR && UNITY_ANDROID
    public static string toPath = Application.persistentDataPath;
#else
    public static string toPath = "updates";
#endif
    //客户端版本
    public static string GAME_VERSION = Program.PRO_VERSION() + "-0622";
    public static string GAME_FILE = toPath + "/version.txt";
    public static bool UPDATE_DATA = false;

    //音效版本
    public static string GAME_SOUND_VERSION = "0.3";
    public static string SOUND_FILE = toPath + "/sound.txt";
    public static bool UPDATE_SOUND = false;

    //UI版本
    public static string GAME_UI_VERSION = "0.1";
    public static string UI_FILE = toPath + "/ui.txt";
    public static bool UPDATE_UI = false;

    //卡片数量
    public static int CARDS_NUMBER = 11458;

    public static int CheckCards(string path)
    {
        try
        {
            using (SqliteConnection connection = new SqliteConnection("Data Source=" + path))
            {
                connection.Open();
                using (IDbCommand command = new SqliteCommand("SELECT COUNT(*) FROM datas;", connection))
                {
                    CARDS_NUMBER = Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        catch { }
        return CARDS_NUMBER;
    }

    public static void CheckVersion()
    {
        string game_ver = "";
        string sound_ver = "";
        string ui_ver = "";

        if (File.Exists(GAME_FILE))
        {
            game_ver = File.ReadAllText(GAME_FILE);
        }
        if (game_ver != GAME_VERSION)
        {
            UPDATE_DATA = true;
        }

        if (File.Exists(SOUND_FILE))
        {
            sound_ver = File.ReadAllText(SOUND_FILE);
        }
        if (sound_ver != GAME_SOUND_VERSION)
        {
            UPDATE_SOUND = true;
        }

        if (File.Exists(UI_FILE))
        {
            ui_ver = File.ReadAllText(UI_FILE);
        }
        if (ui_ver != GAME_UI_VERSION)
        {
            UPDATE_UI = true;
        }
    }
}