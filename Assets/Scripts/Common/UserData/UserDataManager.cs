using Singleton.Component;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class UserDataManager : SingletonComponent<UserDataManager>
{
    // 모든 사용자 데이터 인스턴스를 관리하는 리스트
    public List<IUserData> UserDataList { get; private set; } = new List<IUserData>();

    public string SaveFolderPath { get; private set; }
    private const string SAVE_FOLDER = "SaveData";

    #region Singleton
    protected override void AwakeInstance()
    {
        SetupPaths();
        UserDataList.Add(new UserSettingsData());
    }

    protected override bool InitInstance()
    {
        return true;
    }

    protected override void ReleaseInstance()
    {

    }
    #endregion

    private void SetupPaths()
    {
        SaveFolderPath = Path.Combine(Application.persistentDataPath, SAVE_FOLDER);

        if (!Directory.Exists(SaveFolderPath))
            Directory.CreateDirectory(SaveFolderPath);
    }

    // 모든 사용자 데이터를 기본값으로 설정하는 메서드
    public void SetDefaultUserData()
    {
        // 사용자 데이터 리스트 개수만큼 반복
        for (int i = 0; i < UserDataList.Count; i++)
        {
            // 각 사용자 데이터의 기본값 설정
            UserDataList[i].SetDefaultData();
        }
    }

    // 저장된 사용자 데이터를 불러오는 메서드
    public void LoadUserData()
    {
        // 사용자 데이터 리스트 개수만큼 반복
        for (int i = 0; i < UserDataList.Count; i++)
        {
            // 각 사용자 데이터 불러오기
            if (!UserDataList[i].LoadData())
            {
                UserDataList[i].SetDefaultData();
                UserDataList[i].SaveData(); // 기본값으로 설정 후 저장
            }
        }
    }

    // 사용자 데이터를 저장하는 메서드
    public void SaveUserData()
    {
        // 사용자 데이터 리스트 개수만큼 반복
        for (int i = 0; i < UserDataList.Count; i++)
        {
            UserDataList[i].SaveData();
        }
    }

    // 제네릭을 사용하여 특정 타입의 사용자 데이터를 가져오는 메서드
    public T GetUserData<T>() where T : class, IUserData
    {
        // LINQ를 사용하여 해당 타입의 첫 번째 데이터 반환
        return UserDataList.OfType<T>().FirstOrDefault();
    }
}
