using UnityEngine;

public class FootstepStateBehaviour : StateMachineBehaviour {

    // quantos sons de passo tocamos por cada ciclo completo da animação
    private int stepsPerCycle = 2;

    // guarda a última parte do ciclo em que já tocámos som, para não repetirmos o som todas as frames
    private int lastStepIndex;

    // corre uma única vez no instante em que este estado de animação fica ativo
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        // -1 garante que o primeiro passo do ciclo é sempre detetado como novo, já que currentStepIndex nunca pode começar abaixo de 0
        lastStepIndex = -1;
    }

    // corre todas as frames enquanto este estado de animação está ativo
    // (como o método Update(), mas ligado apenas a este estado específico do Animator)
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        // normalizedTime cresce sempre e não fica preso entre 0 e 1 quando a animação está em loop
        // (0, 0.3, 0.6, 1.0, 1.3, 1.6, 2.0...), cada volta completa soma +1 ao multiplicar por stepsPerCycle e arredonda para baixo
        // transformamos esse valor contínuo num índice que sobe em "degraus" —> sobe 2x por cada ciclo completo, uma vez por cada passada
        int currentStepIndex = Mathf.FloorToInt(stateInfo.normalizedTime * stepsPerCycle);

        // só tocamos o som quando entramos numa parte nova do ciclo (deteção de transição)
        // sem este if, tocaríamos o som em todas as frames, o que geraria ruído constante
        if (currentStepIndex != lastStepIndex) {
            SoundManager.Instance.PlayFootstepSound(animator.gameObject);
            lastStepIndex = currentStepIndex;
        }
    }
}