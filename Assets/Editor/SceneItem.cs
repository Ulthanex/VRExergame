using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SceneItem : Editor
{

    // Use this for initialization
    [MenuItem("Open Scene/Game")]

    public static void OpenGame()
    {
        OpenScene("Game");
    }

    [MenuItem("Open Scene/Menu")]

    public static void OpenMenu()
    {
        OpenScene("Menu");
    }

    static void OpenScene(string name)
    {
        if (EditorApplication.SaveCurrentSceneIfUserWantsTo())
        {
            EditorApplication.OpenScene("Assets/Scenes/" + name + ".unity");
        }
    }
}
