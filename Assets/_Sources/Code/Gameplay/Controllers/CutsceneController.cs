using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class CutsceneController : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;

    private void Awake()
    {
        director.stopped += OnTimelineStopped;
    }

    private void Start()
    {
        director.Play();
    }

    private void OnTimelineStopped(PlayableDirector obj)
    {
        director.stopped -= OnTimelineStopped;
        SceneManager.LoadScene("Game");
    }
}
