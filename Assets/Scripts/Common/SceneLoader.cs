using Singleton.Component;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneType
{
    Title,
    Lobby,
    CutScene,
    InGame,
}

public class SceneLoader : SingletonComponent<SceneLoader>
{
    #region Singleton
    protected override void AwakeInstance()
    {

    }

    protected override bool InitInstance()
    {
        return true;
    }

    protected override void ReleaseInstance()
    {

    }
    #endregion

    public void LoadScene(SceneType sceneType)
    {
        Debug.Log($"{sceneType} scene loading...");

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneType.ToString());
    }

    public void ReloadScene()
    {
        Debug.Log($"{SceneManager.GetActiveScene().name} scene loading...");

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public AsyncOperation LoadSceneAsync(SceneType sceneType)
    {
        Debug.Log($"{sceneType} scene async loading...");

        Time.timeScale = 1f;
        return SceneManager.LoadSceneAsync(sceneType.ToString());
    }

    public SceneType GetCurrentScene()
    {
        return (SceneType)Enum.Parse(typeof(SceneType), SceneManager.GetActiveScene().name);
    }
}
