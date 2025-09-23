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
                enabled = false; // Disable script if no VideoPlayer
                return;
            }
        }

        videoPlayer.Prepare(); // Prepare the video for playback
        videoPlayer.loopPointReached += OnVideoEnd; // Subscribe to end event
    }

    void Update()
    {
        if (videoPlayer.isPrepared && !videoPlayer.isPlaying)
        {
            videoPlayer.Play(); // Start playing once prepared
        }

        if (!playingForward) // If playing in reverse
        {
            // Manually decrement the frame or time for reverse playback
            // Using frame is generally smoother for reverse playback than time
            if (videoPlayer.frame > 0)
            {
                videoPlayer.frame--;
            }
            else
            {
                // Reached the beginning, switch to forward
                playingForward = true;
                videoPlayer.Play(); // Start playing forward
            }
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        // This event fires when the video reaches its end (in forward playback)
        if (playingForward)
        {
            playingForward = false;
            videoPlayer.Stop();
            // No need to explicitly Play() here, Update() will handle reverse
        }
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd; // Unsubscribe to prevent memory leaks
        }
    }
}