using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SurveillanceCamera : MonoBehaviour
{
    [Header("Identificação")]
    public string cameraLabel = "CAM-01";
    public int floor = 1;

    [Header("Render Texture")]
    public RenderTexture renderTexture;

    [SerializeField] private Transform pivotTransform; // para rodar a camara
    [SerializeField] private float panRange = 40f; // o quão é que a câmara pode rodar desde o ângulo original
    [SerializeField] private float panDuration = 12f; // quanto tempo demora a fazer um ângulo inteiro
    [SerializeField] public bool panEnabled = true; // se for falso a câmara não roda

    private Camera cam;
    private float baseYRotation; // ângulo original
    private float panPhase = 0f; // serve como "fase" para o movimento de rodar seguindo o seno para girar suavemente
    private bool _initialised = false;

    public void Initialise()
    {
        cam = GetComponent<Camera>();

        if (renderTexture != null)
            cam.targetTexture = renderTexture;

        if (pivotTransform == null)
            pivotTransform = transform;

        baseYRotation = pivotTransform.eulerAngles.y;
        _initialised = true;
    }

    void Update()
    {
        if (!_initialised) return;
        UpdatePan();
    }

    private void UpdatePan() {
        if (!panEnabled) {
            // snap de volta ao centro se o pan for desligado em runtime
            Vector3 rest = pivotTransform.eulerAngles;
            rest.y = Mathf.LerpAngle(rest.y, baseYRotation, Time.deltaTime * 2f);
            pivotTransform.eulerAngles = rest;
            return;
        }

        // panPhase começa a 0 -> sin(0) = 0 -> arranca sempre do centro
        panPhase += Time.deltaTime / panDuration;
        float sine = Mathf.Sin(panPhase * Mathf.PI * 2f);
        float targetY = baseYRotation + sine * panRange;
        Vector3 e = pivotTransform.eulerAngles;
        e.y = Mathf.LerpAngle(e.y, targetY, Time.deltaTime * 2f);
        pivotTransform.eulerAngles = e;
    }

}
