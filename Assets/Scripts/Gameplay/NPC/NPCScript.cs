using System.Collections;
using UnityEngine;

public class NPCScript : MonoBehaviour {

    // o tipo influencia como o NPC reage ao estado global de suspeita
    // Ex: apenas Guards entram no modo Investigate, os colegas ignoram
    public enum NPCType { Colleague, Boss, Receptionist, Guard, Visitor }

    // Idle     — parado, sem fazer nada de especial
    // Patrol   — a percorrer os patrolPoints em sequência
    // Attention — parado, a olhar à volta, mais alerta
    // Investigate — a mover-se para o último sítio onde viu o jogador
    // Chase    — a perseguir ativamente o jogador (leva a Game Over)
    public enum NPCState { Idle, Patrol, Attention, Investigate, Chase }

    [SerializeField] private NPCType npcType = NPCType.Colleague;

    // fovAngle: ângulo do cone de visão em graus (ex: 90 = 45° para cada lado)
    // fovRange: distância máxima a que o NPC consegue ver
    [SerializeField] private float fovAngle = 90f;
    [SerializeField] private float fovRange = 15f;

    // pontos de patrulha pela cena, o NPC percorre-os em sequência ciclicamente
    [SerializeField] private Transform[] patrolPoints;

    private NPCState currentState = NPCState.Idle;
    private Transform playerTransform;
    private int currentPatrolIndex = 0;

    // flag de se este NPC deveria estar a patrulhar (independentemente do estado atual de suspeita).
    // usado para repor o comportamento correto quando o estado de suspeita baixa, alguns NPCs patrulham, outros ficam em Idle.
    private bool isPatrolling = true;


    void Start() {
        playerTransform = NPCManager.Instance.player;
        // registo no NPCManager para receber broadcasts de estado
        NPCManager.Instance.RegisterNPC(this);

        // a verificação do FOV corre como Coroutine para não sobrecarregar o Update com deteção visual a 60fps
        StartCoroutine(FOVCheckRoutine());

        SetState(NPCState.Idle);
    }

    void OnDestroy() {
        // garantir que o NPCManager não tenta notificar um NPC que já não existe (causaria NullReferenceException)
        NPCManager.Instance.UnregisterNPC(this);
    }


    // SetState é o único ponto de entrada para mudar de estado
    private void SetState(NPCState newState) {
        if (currentState == newState) 
            return;

        currentState = newState;

        switch (newState) {
            case NPCState.Idle: EnterIdle(); 
                break;

            case NPCState.Patrol: EnterPatrol(); 
                break;

            case NPCState.Attention: EnterAttention(); 
                break;

            case NPCState.Investigate: EnterInvestigate(); 
                break;

            case NPCState.Chase: EnterChase(); 
                break;
        }
    }

    private void EnterIdle() {

    }

    private void EnterPatrol() {

    }

    private void EnterAttention() {

    }

    private void EnterInvestigate() {

    }

    private void EnterChase() {

    }


    // corre em paralelo com o Update, independentemente do frame rate
    // WaitForSeconds(0.1f) significa que verifica 10 vezes por segundo, suficiente para ser responsivo sem ser caro em termos de CPU
    //
    // a condição PlayerController.Instance.inSusPlace verifica se o jogador está numa zona marcada como suspeita
    // fora dessas zonas, mesmo que o NPC veja o jogador, não gera suspeita —> o jogador tem o direito de circular
    private IEnumerator FOVCheckRoutine() {
        WaitForSeconds wait = new WaitForSeconds(0.1f);
        while (true) {
            if (PlayerController.Instance.inSusPlace && IsPlayerInFOV()) {
                float dist = Vector3.Distance(transform.position, playerTransform.position);
                float level = GetSuspicionLevelByDistance(dist);
                if (level > 0)
                    SuspicionManager.Instance.IncreaseSuspicion(level, SuspicionManager.SuspicionSource.NPCSight);
            } else {
                // se o jogador saiu do FOV ou da zona suspeita, pára de alimentar a subida da suspeita —> o decay corre sozinho no SuspicionManager
                SuspicionManager.Instance.StopIncreasingSuspicion();
            }
            yield return wait;
        }
    }

    // verifica se o jogador está dentro do cone de visão deste NPC
    // dois critérios: distância (fovRange) e ângulo (fovAngle)
    private bool IsPlayerInFOV() {
        Vector3 dir = (playerTransform.position - transform.position).normalized;
        float dist = Vector3.Distance(transform.position, playerTransform.position);

        if (dist > fovRange) return false;
        return Vector3.Angle(transform.forward, dir) <= fovAngle / 2f;
    }

    // quanto mais perto, maior o nível de suspeita (1, 1.5 ou 2)
    // estes valores são usados pelo SuspicionManager como multiplicador da velocidade de subida da barra
    private float GetSuspicionLevelByDistance(float distance) {
        float third = fovRange / 3f;
        if (distance < third) return 2f;
        if (distance < third * 2f) return 1.5f;
        return 1f;
    }


    // chamado pelo NPCManager quando o estado global de suspeita muda, cada NPC decide a sua própria reação com base no seu tipo
    // guardas são mais proativos — entram em Investigate na fase de investigação enquanto colegas e outros só reagem a Attention e Expulsion
    public void OnGlobalSuspicionChanged(SuspicionManager.SuspicionState state) {
        switch (state) {
            case SuspicionManager.SuspicionState.None:
                // tudo voltou ao normal —> retoma patrulha se for o caso
                SetState(isPatrolling ? NPCState.Patrol : NPCState.Idle);
                break;
            case SuspicionManager.SuspicionState.Attention:
                SetState(NPCState.Attention);
                break;
            case SuspicionManager.SuspicionState.Investigation:
                // só guardas investigam ativamente —> os outros ficam em Attention
                if (npcType == NPCType.Guard) SetState(NPCState.Investigate);
                break;
            case SuspicionManager.SuspicionState.Expulsion:
                SetState(NPCState.Chase);
                break;
        }
    }

    // para o NPCManager alterar o modo de patrulha individualmente ou em grupo (via SetAllPatrolling)
    public void SetPatrolling(bool patrolling) {
        isPatrolling = patrolling;
        if (patrolling && currentState == NPCState.Idle)
            SetState(NPCState.Patrol);
        else if (!patrolling)
            SetState(NPCState.Idle);
    }
}