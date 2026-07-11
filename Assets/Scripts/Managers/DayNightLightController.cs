using UnityEngine;

public class DayNightLightController : MonoBehaviour {

    [Header("Referência")]
    [SerializeField] private Light directionalLight;

    [Header("Cor ao longo do dia")]
    [SerializeField] private Gradient lightColorGradient;
    [SerializeField] private Gradient ambientColorGradient;

    [Header("Intensidade (x = hora normalizada 0-1, y = intensidade)")]
    [SerializeField] private AnimationCurve intensityCurve;

    [Header("Elevação do sol (x = hora normalizada, y = graus -90 a 90)")]
    [SerializeField] private AnimationCurve sunElevationCurve;

    // o azimute é a direção horizontal para onde o sol aponta, é medida em graus à volta do eixo Y(0°-360°)
    // a 170° coloca o sol ligeiramente a sul
    [SerializeField] private float sunAzimuth = 170f;


    // lê o tempo atual do TimeManager, normaliza para [0, 1] e avalia as curvas e gradientes
    // o % 24f evita problemas se currentMinutes ultrapassar 24h antes do reset do TimeManager
    private void Update() {
        float time = (TimeManager.Instance.GetCurrentTimeInHours() % 24f) / 24f;

        directionalLight.color = lightColorGradient.Evaluate(time);
        directionalLight.intensity = intensityCurve.Evaluate(time);
        directionalLight.transform.rotation = Quaternion.Euler(sunElevationCurve.Evaluate(time), sunAzimuth, 0f);

        RenderSettings.ambientLight = ambientColorGradient.Evaluate(time);
    }
}