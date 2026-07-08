using UnityEngine;

public class SoundManager : MonoBehaviour {
    public static SoundManager Instance;

    [Header("Music")]
    [SerializeField] private AudioClip gameplayTheme;
    [SerializeField] private AudioClip menuTheme;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip heartbeatPulse;
    [SerializeField] private AudioClip alarmExpulsion;
    [SerializeField] private AudioClip openDoor;
    [SerializeField] private AudioClip closeDoor;
    [SerializeField] private AudioClip singKeyboardSound;
    [SerializeField] private AudioClip typingKeyboard;
    [SerializeField] private AudioClip buzzerWrong2;
    [SerializeField] private AudioClip die;
    [SerializeField] private AudioClip inputCodeNumber;
    [SerializeField] private AudioClip startHackCamera;
    [SerializeField] private AudioClip buzzerCorrect;
    [SerializeField] private AudioClip buzzerWrong;
    [SerializeField] private AudioClip cameraComputer;
    [SerializeField] private AudioClip apanharPapel;
    [SerializeField] private AudioClip footsteps;


    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void PlaySound(AudioSource audio) {
        if (!audio.isPlaying) {
            audio.Play();
        } else {
            audio.Stop();
            audio.Play();
        }
    }

    public void StopSound(AudioSource audio) {
        audio.Stop();
    }

    public void PlaySound(GameObject soundObject) {
        AudioSource audio = soundObject.GetComponent<AudioSource>();
        PlaySound(audio);
    }


    public void PlayFootstepSound(GameObject soundObject) {
        AudioSource audio = soundObject.GetComponent<AudioSource>();
        audio.clip = footsteps;
        PlaySound(audio);
    }

    public void PlayDoorSound(GameObject soundObject, bool openDoorBool) {
        AudioSource audio = soundObject.GetComponent<AudioSource>();
        audio.clip = openDoorBool ? openDoor : closeDoor;
        PlaySound(audio);
    }

}