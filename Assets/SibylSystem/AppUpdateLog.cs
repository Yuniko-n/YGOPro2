using UnityEngine;
using System;
using System.Data;
using System.IO;
using Mono.Data.Sqlite;

public class AppUpdateLog
{
    //客户端版本
    public static string GAME_VERSION = Program.PRO_VERSION();

    //卡片数量
    public static int CARDS_NUMBER = 10947;

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
}