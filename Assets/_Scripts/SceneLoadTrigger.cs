using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadTrigger : MonoBehaviour
{
    [SerializeField] string whichScene;
    [SerializeField] bool shouldSaveStats;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == PlayerController.Instance.gameObject)
        {
            if (shouldSaveStats)
            {
                // Save current stats before transitioning
                PlayerStats.Instance.SaveStats();
            }
            else
            {
                // Reset player for death/restart scenarios
                PlayerController.Instance.ResetPlayer();
            }

            SceneManager.LoadScene(whichScene);
        }
    }
}
