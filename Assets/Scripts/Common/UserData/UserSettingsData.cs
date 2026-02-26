using System;
using System.IO;
using UnityEngine;

[Serializable]
public class GameSettings
{
    public bool Music_Mute;
    public bool SFX_Mute;
}

public class UserSettingsData : IUserData
{
    public GameSettings Settings { get; set; } = new();

    private const string SETTINGS_FILE = "Settings.json";

    public void SetDefaultData()
    {
        Debug.Log($"{GetType()}::SetDefaultData");

        Settings = new GameSettings
        {
            Music_Mute = false,
            SFX_Mute = false
        };
    }

    public bool LoadData()
    {
        Debug.Log($"{GetType()}::LoadData");

        bool result = false; // 로드 결과 저장용 변수

        try
        {
            string filePath = Path.Combine(UserDataManager.Instance.SaveFolderPath, SETTINGS_FILE);
            string json = File.ReadAllText(filePath);
            Settings = JsonUtility.FromJson<GameSettings>(json);

            result = true; // 로드 성공
        }
        catch (Exception e)
        {
            // 로드 실패 처리
            Debug.Log("Load failed (" + e.Message + ")");
        }

        return result; // 로드 결과 반환
    }

    public bool SaveData()
    {
        Debug.Log($"{GetType()}::SaveData");

        bool result = false; // 저장 결과 저장용 변수

        try
        {
            string json = JsonUtility.ToJson(Settings, true);
            string filePath = Path.Combine(UserDataManager.Instance.SaveFolderPath, SETTINGS_FILE);
            File.WriteAllText(filePath, json);

            result = true; // 저장 성공
        }
        catch (Exception e)
        {
            // 저장 실패 처리
            Debug.Log("Save failed (" + e.Message + ")");
        }

        return result; // 저장 결과 반환
    }
}
