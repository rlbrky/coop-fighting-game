using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Components;
using UnityEngine.SceneManagement;

public class BootstrapSceneManager : NetworkBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    void LoadScene(string sceneName)
    {
        if (!IsServer)
            return;

        NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    void UnloadScene(Scene loadedScene)
    {
        if (!IsServer)
            return;

        NetworkManager.SceneManager.UnloadScene(loadedScene);
    }
}