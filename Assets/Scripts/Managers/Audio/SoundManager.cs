using UnityEngine;

public class SoundManager : MonoBehaviour {
    public static SoundManager Instance;

    [Header("Audio Sources")]
    public AudioSource audioSource2D;
    // fonte separada para sons das cameras de seguranca ao ligar o computador, assim não interfere com o feedback de gameplay normal
    public AudioSource audioSource2DCameras;
    public AudioSource gameplayMusicSource;
    public AudioSource menuMusicSource;
    // fonte dedicada ao heartbeat para podermos controlar o volume independentemente em SuspicionManager.UpdateHeartbeat
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
    public AudioClip buzzerWrong2; // versão alternativa de erro que é usada no camera hack fail porque o som é diferente
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

    // 3D -> sons posicionados no mundo, ligados a AudioSources nos próprios GameObjects
    public AudioClip openDoor;
    public AudioClip closeDoor;
    public AudioClip footsteps;
    public AudioClip typingKeyboard;

    // indexado pelo SuspicionState (0=None, 1=Attention, 2=Investigation, 3=Expulsion)
    // o SuspicionManager.UpdateHeartbeat usa este array para mapear o estado atual ao volume certo
    [HideInInspector] public float[] heartbeatVolumeSteps = { 0.25f, 0.5f, 0.75f, 1f };

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // parar antes de tocar evita sobreposição de clips na mesma fonte 2D
    public void PlaySound(AudioSource audio, AudioClip clip) {
        audio.clip = clip;
        if (audio.isPlaying)
            audio.Stop();
        audio.Play();
    }

    // pausa o menu para não sobrepor com a musica do jogo
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