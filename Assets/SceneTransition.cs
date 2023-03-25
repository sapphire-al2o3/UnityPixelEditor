using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [SerializeField]
    string scene;

    public void Run()
    {
        SceneManager.LoadScene(scene);
    }
}
