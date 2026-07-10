using UnityEngine;

public class SoundManager : MonoBehaviour {
    public static SoundManager Instance;

    [Header("Audio Sources")]
    public AudioSource audioSource2D;
    public AudioSource audioSource2DCameras;
    public AudioSource gameplayMusicSource;
    public AudioSource menuMusicSource;
    public AudioSource heartbeatSource;

    [Header("Music")]
    public AudioClip gameplayTheme;
    public AudioClip menuTheme;

    [Header("Sound Effects")]
    // 2D
    public AudioClip apanharPapel;
    public AudioClip heartbeatPulse;
    public AudioClip buttonClick;
    public AudioClip buzzerCorrect;
    public AudioClip buzzerWrong;
    public AudioClip buzzerWrong2;
    public AudioClip die;
    public AudioClip alarmExpulsion;
    public AudioClip cameraComputer;
    public AudioClip singKeyboardSound;
    public AudioClip startHackCamera;
    public AudioClip inputCodeNumber;
    public AudioClip cardReaderSuccess;
    public AudioClip cameraSwitch;
    public AudioClip flashlightToggleOnOff;
    public AudioClip taskAppeared;
    public AudioClip travelDingElevator;
    public AudioClip intelPickup;

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

    public void PlayGameplayMusic() {
        menuMusicSource.Pause();

        gameplayMusicSource.clip = gameplayTheme;

        if (!gameplayMusicSource.isPlaying)
            gameplayMusicSource.Play();
    }

    public void PlayMenuMusic() {
        gameplayMusicSource.Pause();

        menuMusicSource.clip = menuTheme;

        if (!menuMusicSource.isPlaying)
            menuMusicSource.Play();
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