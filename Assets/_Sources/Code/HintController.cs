using System.Collections;
using System.Collections.Generic;
using Sources.Managers;
using UnityEngine;

public class HintController : MonoBehaviour
{
    [SerializeField] private HintUI ui;
    [SerializeField] private List<HintData> hints;

    private int index;
    private Coroutine routine;
    private InputManager input;

    private void Awake()
    {
        input = InputManager.Instance;
    }

    private void Start()
    {
        ShowNext();
    }

    private void ShowNext()
    {
        if (index >= hints.Count)
        {
            ui.Hide();
            return;
        }

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(ShowRoutine(hints[index]));
    }

    private IEnumerator ShowRoutine(HintData hint)
    {
        ui.Show(hint.text);

        float t = 0f;
        while (t < hint.duration)
        {
            if (CheckCondition(hint.condition))
                break;

            t += Time.deltaTime;
            yield return null;
        }

        ui.Hide();
        yield return new WaitForSeconds(0.35f);

        index++;
        ShowNext();
    }

    private bool CheckCondition(HintCondition condition)
    {
        switch (condition)
        {
            case HintCondition.PlayerMoved:
                return Mathf.Abs(input.Horizontal) > 0.1f ||
                       Mathf.Abs(input.Vertical) > 0.1f;

            case HintCondition.PlayerInteracted:
                return input.ConsumeInteract();

            case HintCondition.ItemPicked:
                return false;
        }

        return false;
    }
}
