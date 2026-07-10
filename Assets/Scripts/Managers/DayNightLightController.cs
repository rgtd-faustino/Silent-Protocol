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
    // azimute fixo do sol - 170° coloca o sol ligeiramente a sul, o que fica bem para um cenário urbano de Portugal
    [SerializeField] private float sunAzimuth = 170f;

    // valores padrão das curvas - chamado pelo Unity quando o componente é adicionado ou o botão Reset é premido no inspector -
    // os keyframes foram ajustados empiricamente para simular as horas de luz de um dia de primavera/verão
    private void Reset() {
        intensityCurve = new AnimationCurve(
            new Keyframe(0.00f, 0.00f),  // 00:00
            new Keyframe(0.32f, 0.00f),  // 07:40 ainda escuro
            new Keyframe(0.36f, 0.40f),  // nascente
            new Keyframe(0.50f, 1.20f),  // meio-dia
            new Keyframe(0.75f, 0.85f),  // fim de tarde
            new Keyframe(0.91f, 0.00f),  // pôr do sol
            new Keyframe(1.00f, 0.00f)
        );

        sunElevationCurve = new AnimationCurve(
            new Keyframe(0.00f, -30f),
            new Keyframe(0.36f, 0f),
            new Keyframe(0.50f, 65f),
            new Keyframe(0.75f, 25f),
            new Keyframe(0.91f, -5f),
            new Keyframe(1.00f, -30f)
        );
    }

    // lê o tempo atual do TimeManager, normaliza para [0, 1] e avalia as curvas e gradientes -
    // o % 24f evita problemas se currentMinutes ultrapassar 24h antes do reset do TimeManager
    private void Update() {
        float time = (TimeManager.Instance.GetCurrentTimeInHours() % 24f) / 24f;

        directionalLight.color = lightColorGradient.Evaluate(time);
        directionalLight.intensity = intensityCurve.Evaluate(time);
        directionalLight.transform.rotation = Quaternion.Euler(sunElevationCurve.Evaluate(time), sunAzimuth, 0f);

        RenderSettings.ambientLight = ambientColorGradient.Evaluate(time);
    }
}