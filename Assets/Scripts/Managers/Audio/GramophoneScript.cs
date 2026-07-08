using System.Collections;
using UnityEngine;

public class GramophoneScript : MonoBehaviour {
    private IEnumerator coroutine;

    void Start()
    {
        coroutine = PlaySoundPeriodically();
        StartCoroutine(coroutine);
    }

    private IEnumerator PlaySoundPeriodically() {
        while (true) {
            yield return new WaitForSeconds(60f);
            SoundManager.Instance.PlaySound(gameObject);
        }
    }
}
