using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelegateScheduler : MonoBehaviour
{
    public static DelegateScheduler Instance;
    private void Awake() => Instance = this;
    public void Schedule(Action action, float seconds)
    {
        StartCoroutine(coroutine(action, seconds));
    }
    private IEnumerator coroutine(Action action, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        action?.Invoke();
    }
}
