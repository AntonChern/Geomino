using UnityEngine;
using UnityEngine.Video;

public class VideoLoop : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    private bool playingForward = true;

    void Start()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
            if (videoPlayer == null)
            {
                Debug.LogError("VideoPlayer component not found!");
                enabled = false;
                return;
            }
        }

        videoPlayer.Prepare();
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void Update()
    {
        if (videoPlayer.isPrepared && !videoPlayer.isPlaying)
        {
            videoPlayer.Play();
        }

        if (!playingForward)
        {
            if (videoPlayer.frame > 0)
            {
                videoPlayer.frame--;
            }
            else
            {
                playingForward = true;
                videoPlayer.Play();
            }
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        if (playingForward)
        {
            playingForward = false;
            videoPlayer.Stop();
        }
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
        }
    }
}