using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;


public class IntroVideoPlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;           

    private bool videoPlayed;         

    private void Start()
    {
        videoPlayer.loopPointReached += OnVideoEnd;
    }
    private void Update()
    {
        if (videoPlayer.isPlaying && !videoPlayed && Input.GetKeyDown(KeyCode.Space))
        {
            videoPlayed = true;
            StopVideoAndLoadMainScene();
        }
    }

    private void StopVideoAndLoadMainScene()
    {
        videoPlayer.Stop();              
        SceneManager.LoadScene("Main");    
    }

    private void OnVideoEnd(VideoPlayer player)
    {
        if (!videoPlayed)
        {
            videoPlayed = true;
            StopVideoAndLoadMainScene();
        }
    }
}
