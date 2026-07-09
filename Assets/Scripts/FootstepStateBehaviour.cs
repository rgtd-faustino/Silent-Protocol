using UnityEngine;

public class FootstepStateBehaviour : StateMachineBehaviour {

    private int stepsPerCycle = 2; // quantos sons de passo por cada ciclo da animação

    private int lastStepIndex;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        lastStepIndex = -1;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        // normalizedTime cresce sempre (0, 0.3, 0.6, 1.0, 1.3, 1.6, 2.0...)
        // multiplicando por stepsPerCycle e arredondando para baixo, obtemos um índice que sobe a cada "fatia" do ciclo
        int currentStepIndex = Mathf.FloorToInt(stateInfo.normalizedTime * stepsPerCycle);

        if (currentStepIndex != lastStepIndex) {
            SoundManager.Instance.PlayFootstepSound(animator.gameObject);
            lastStepIndex = currentStepIndex;
        }
    }
}