using UnityEngine;
using World;

public class MusicPlayer : MonoBehaviour
{
    public AudioSource music;
    public AudioClip[] tracks;

    private int curTrackNum = 0;
    private float timeLeft;

    // Play the music
    bool play;
    // Detect when you use the toggle, ensures music isn’t played multiple times
    bool toggleChange;

    void Start()
    {
        // Fetch the AudioSource from the GameObject
        music = GetComponent<AudioSource>();
        music.clip = tracks[0];
        music.Play();
        timeLeft = music.clip.length;
        // Ensure the toggle is set to true for the music to play at start-up
        play = true;
        toggleChange = false;
    }

    // Go to the next song
    public void Next()
    {
        music.Stop();

        curTrackNum += 1;
        curTrackNum %= tracks.Length;
        music.clip = tracks[curTrackNum];

        music.Play();
        timeLeft = music.clip.length;
    }

    void Update()
    {
        timeLeft -= Time.deltaTime;

        // Check to see if you just set the toggle to positive
        if (play == false && toggleChange == true)
        {
            // Play the audio you attach to the AudioSource component
            music.Play();
            // Ensure audio doesn’t play more than once
            toggleChange = false;
            play = true;
        }
        // Check if you just set the toggle to false
        if (play == true && toggleChange == true)
        {
            // Stop the audio
            music.Stop();
            // Ensure audio doesn’t play more than once
            toggleChange = false;
            play = false;
        }

        // Go to next song if the current song ended
        if (play == true && toggleChange == false && !music.isPlaying && timeLeft < 0.1) {
            Next();
        }
    }

    public void toggleMusic() {
        toggleChange = true;
    }
}