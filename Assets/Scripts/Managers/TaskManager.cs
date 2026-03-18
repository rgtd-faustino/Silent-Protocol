using UnityEngine;

public class TaskManager : MonoBehaviour
{

    public enum TaskDifficulty {
        Small,
        Medium,
        Major
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTaskComplete(TaskDifficulty difficulty, bool doneCorrectly) {
        float multiplier = 0f;

        if (difficulty == TaskDifficulty.Small) {
            multiplier = 0.1f;
        } else if (difficulty == TaskDifficulty.Medium) {
            multiplier = 0.25f;
        } else if (difficulty == TaskDifficulty.Major) {
            multiplier = 0.5f;
        }

        SuspicionManager.Instance.ChangeSuspicionOnTaskComplete(multiplier, doneCorrectly);
    }
}
