using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneType
{
    EntryScene,
    Main
}

public static class SceneLoader
{
    public static void Load(SceneType scene)
    {
        SceneManager.LoadScene(scene.ToString());
    }
}
