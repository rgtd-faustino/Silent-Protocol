using UnityEngine;

public class BedScript : InteractableObject {
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnSleepConfirmed(float hours) {
        TimeManager.Instance.TrySleep(hours);
    }

    public override void Interact() {
        ///if (TimeManager.Instance.isNight) {
            UIManager.Instance.OpenSleepView(this);

        //} else {
        //    Debug.Log("Se calhar só posso dormir quando for de noite...");
        //}
    }
}
