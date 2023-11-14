using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class changeScene : MonoBehaviour
{
    public void SceneChange()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void TitleScene()
    {
        SceneManager.LoadScene("Main");
    }
}
