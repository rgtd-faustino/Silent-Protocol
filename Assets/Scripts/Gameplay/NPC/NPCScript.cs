using System.Collections;
using UnityEngine;

public class NPCScript : MonoBehaviour {

    [SerializeField] private float fovAngle = 90f; // ângulo do field of view em graus
    [SerializeField] private float fovRange = 15f; // alcance máximo de visão

    private Transform playerTransform;

    void Start() {
        playerTransform = NPCManager.Instance.player;
        StartCoroutine(FOVCheckRoutine());
    }

    private IEnumerator FOVCheckRoutine() {
        WaitForSeconds wait = new WaitForSeconds(0.1f);
        while (true) {
            if (PlayerController.Instance.inSusPlace && IsPlayerInFOV()) {
                float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
                int suspicionLevel = GetSuspicionLevelByDistance(distanceToPlayer);

                if (suspicionLevel > 0) {
                    SuspicionManager.Instance.IncreaseSuspicion(suspicionLevel);
                }
            } else {
                // se saiu do FOV, para de aumentar suspeita
                SuspicionManager.Instance.StopIncreasingSuspicion();
            }
            yield return wait;
        }
    }

    private bool IsPlayerInFOV() {
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        float distanceToPlayer = (transform.position - playerTransform.position).magnitude;

        // se está dentro do alcance
        if (distanceToPlayer > fovRange) {
            return false;
        }

        // calcula o ângulo entre o forward do NPC e a direção para o jogador
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        // se está dentro do ângulo do FOV
        return angle <= fovAngle / 2f;
    }

    private int GetSuspicionLevelByDistance(float distance) {
        // divide o alcance em 3 partes
        float oneThird = fovRange / 3f;
        float twoThirds = fovRange * 2f / 3f;

        // se distância < 1/3 do alcance = nível 3
        if (distance < oneThird) {
            return 3;

        } else if (distance < twoThirds) { // se distância entre 1/3 e 2/3 = nível 2
            return 2;

        } else { // se distância entre 2/3 e max = nível 1
            return 1;
        }
    }
}