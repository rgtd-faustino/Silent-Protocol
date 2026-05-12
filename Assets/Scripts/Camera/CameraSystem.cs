using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// CameraSystem — Silent Protocol
/// 
/// Architecture:
///   - SurveillanceCamera     : per-camera component (RenderTexture, FOV logic, memory)
///   - CameraSystem           : singleton that manages cycling, suspicion accrual,
///                              and feeds state to CameraViewUI
///
/// Design pillars:
///   1. "Ghost memory"  — each camera remembers the last position the player was
///      spotted. That waypoint slowly drifts the camera's pivot back toward it.
///   2. Signal decay    — overuse degrades the feed visually (CameraViewUI reads
///      CameraSystem.SignalIntegrity [0-1] and applies post-FX).
///   3. Paranoid pacing — suspicion ticks only while the player is ACTIVELY watching,
///      but each session leaves a "residual heat" that makes the next session tick faster.
/// </summary>
public class CameraSystem : MonoBehaviour {
    public static CameraSystem Instance;

    // ─── Inspector ───────────────────────────────────────────────────────────
    [Header("Camera Registry")]
    [Tooltip("All SurveillanceCamera components in the scene. Populate via Inspector.")]
    [SerializeField] private SurveillanceCamera[] allCameras;

    [Header("Suspicion Accrual")]
    [Tooltip("Base suspicion per second while the camera view is open.")]
    [SerializeField] private float baseSuspicionPerSecond = 0.008f;

    [Tooltip("After this many cumulative seconds of camera usage, the rate multiplier starts climbing.")]
    [SerializeField] private float overuseThreshold = 30f;

    [Tooltip("Maximum multiplier applied to the suspicion rate (reached at 3× overuseThreshold).")]
    [SerializeField] private float maxRateMultiplier = 4f;

    [Tooltip("Each session (open → close) adds this residual. Residual decays slowly when camera is closed.")]
    [SerializeField] private float residualPerSession = 0.15f;

    [Tooltip("Each camera switch adds this residual heat.")]
    [SerializeField] private float residualPerSwitch = 0.06f;

    [Tooltip("Rate at which residual heat decays per second when camera is closed.")]
    [SerializeField] private float residualDecayRate = 0.02f;

    [Header("Signal Integrity")]
    [Tooltip("Signal integrity [0-1]. Drops with overuse, recovers when camera is closed.")]
    [SerializeField] private float signalRecoveryRate = 0.05f;

    // ─── Runtime State ───────────────────────────────────────────────────────
    private int currentCameraIndex = 0;
    private bool isActive = false;             // is the player currently watching cameras?
    private float cumulativeWatchSeconds = 0f; // total seconds ever watched this day
    private float residualHeat = 0f;           // session residual, [0-1]
    private float signalIntegrity = 1f;        // public read via property

    /// <summary>Whether the player is currently watching the camera feed.</summary>
    public bool IsActive => isActive;

    /// <summary>Signal quality [0-1]. 1 = clean, 0 = maximum degradation.</summary>
    public float SignalIntegrity => signalIntegrity;

    /// <summary>Residual heat [0-1]. Drives the "paranoia" indicator in the UI.</summary>
    public float ResidualHeat => residualHeat;

    /// <summary>The camera feed the UI should render.</summary>
    public SurveillanceCamera ActiveCamera => (allCameras != null && allCameras.Length > 0)
        ? allCameras[currentCameraIndex]
        : null;

    public int CameraCount => allCameras?.Length ?? 0;
    public int CurrentCameraIndex => currentCameraIndex;

    // ─────────────────────────────────────────────────────────────────────────
    void Awake() {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start() {
        // Initialise each camera
        if (allCameras != null)
            foreach (var cam in allCameras)
                cam?.Initialise();

        // Reset watch time each new day
        GameEvent.OnDayChanged += _ => ResetDailyUsage();
    }

    void OnDestroy() {
        GameEvent.OnDayChanged -= _ => ResetDailyUsage();
    }

    void Update() {
        if (isActive) {
            float dt = Time.deltaTime;
            cumulativeWatchSeconds += dt;

            // Rate multiplier: linear ramp past the threshold
            float overuseFactor = Mathf.Clamp01(
                (cumulativeWatchSeconds - overuseThreshold) / (overuseThreshold * 2f));
            float rateMultiplier = Mathf.Lerp(1f, maxRateMultiplier, overuseFactor)
                                 + residualHeat;          // residual heat adds on top

            // Suspicion
            float tick = baseSuspicionPerSecond * rateMultiplier * dt;
            SuspicionManager.Instance.IncreaseSuspicion(
                Mathf.Clamp(rateMultiplier, 1f, 3f),
                GetInstanceID(),
                SuspicionManager.SuspicionSource.Camera);

            // Signal degrades as overuse grows
            float targetIntegrity = Mathf.Lerp(1f, 0.15f, overuseFactor + residualHeat * 0.4f);
            signalIntegrity = Mathf.MoveTowards(signalIntegrity, targetIntegrity, 0.3f * dt);

        } else {
            // Camera closed — stop suspicion source, recover signal & decay residual
            SuspicionManager.Instance.StopIncreasingSuspicion(GetInstanceID());

            signalIntegrity = Mathf.MoveTowards(signalIntegrity, 1f, signalRecoveryRate * Time.deltaTime);
            residualHeat = Mathf.MoveTowards(residualHeat, 0f, residualDecayRate * Time.deltaTime);
        }

    }

    // ─── Public API ──────────────────────────────────────────────────────────

    /// <summary>Open the camera view. Called by the interactable terminal.</summary>
    public void OpenCameraView() {
        if (isActive) return;
        isActive = true;
        residualHeat = Mathf.Min(1f, residualHeat + residualPerSession);
        CameraViewUI.Instance.Show(this);
    }

    /// <summary>Close the camera view.</summary>
    public void CloseCameraView() {
        if (!isActive) return;
        isActive = false;
        CameraViewUI.Instance.Hide();
        PlayerController.Instance.canMoveRotate = true;
    }

    /// <summary>Cycle to the next camera.</summary>
    public void NextCamera() {
        if (allCameras == null || allCameras.Length == 0) return;
        currentCameraIndex = (currentCameraIndex + 1) % allCameras.Length;
        residualHeat = Mathf.Min(1f, residualHeat + residualPerSwitch);
        CameraViewUI.Instance.OnCameraChanged(allCameras[currentCameraIndex]);
    }

    /// <summary>Cycle to the previous camera.</summary>
    public void PreviousCamera() {
        if (allCameras == null || allCameras.Length == 0) return;
        currentCameraIndex = (currentCameraIndex - 1 + allCameras.Length) % allCameras.Length;
        residualHeat = Mathf.Min(1f, residualHeat + residualPerSwitch);
        CameraViewUI.Instance.OnCameraChanged(allCameras[currentCameraIndex]);
    }

    /// <summary>Jump to a specific camera index.</summary>
    public void SwitchToCamera(int index) {
        if (allCameras == null || index < 0 || index >= allCameras.Length) return;
        currentCameraIndex = index;
        CameraViewUI.Instance.OnCameraChanged(allCameras[currentCameraIndex]);
    }

    private void ResetDailyUsage() {
        cumulativeWatchSeconds = 0f;
        // residual heat persists across days to simulate accumulating paranoia
    }
}