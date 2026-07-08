using UnityEngine;

public class SoundManager : MonoBehaviour {
    public static SoundManager Instance;

    [Header("Audio Sources")]
    public AudioSource audioSource2D;
    public AudioSource audioSourceTheme;
    public AudioSource heartbeatSource;

    [Header("Music")]
    public AudioClip gameplayTheme;
    public AudioClip menuTheme;

    [Header("Sound Effects")]
    // 2D
    public AudioClip apanharPapel;
    public AudioClip heartbeatPulse;
    public AudioClip buzzerCorrect;
    public AudioClip buzzerWrong;
    public AudioClip buzzerWrong2;
    public AudioClip die;
    public AudioClip alarmExpulsion;
    public AudioClip cameraComputer;
    public AudioClip singKeyboardSound;
    public AudioClip startHackCamera;
    public AudioClip inputCodeNumber;

    // 3D
    public AudioClip openDoor;
    public AudioClip closeDoor;
    public AudioClip footsteps;
    public AudioClip typingKeyboard;

    [HideInInspector] public float[] heartbeatVolumeSteps = { 0.25f, 0.5f, 0.75f, 1f }; // None, Attention, Investigation, Expulsion

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void PlaySound(AudioSource audio, AudioClip clip) {
        audio.clip = clip;

        if (audio.isPlaying) 
            audio.Stop();

        audio.Play();
    }

    public void PlayMusic(AudioClip clip) {
        if (audioSourceTheme.clip == clip && audioSourceTheme.isPlaying) 
            return; // já está a tocar, não reinicia

        audioSourceTheme.clip = clip;
        audioSourceTheme.loop = true;
        audioSourceTheme.Play();
    }

    public void PlayFootstepSound(GameObject soundObject) {
        AudioSource audio = soundObject.GetComponent<AudioSource>();
        PlaySound(audio, footsteps);
    }

    public void PlayDoorSound(GameObject soundObject, bool openDoorBool) {
        AudioSource audio = soundObject.GetComponent<AudioSource>();
        PlaySound(audio, openDoorBool ? openDoor : closeDoor);
    }

}