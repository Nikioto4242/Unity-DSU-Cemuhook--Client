using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerDataRequester : MonoBehaviour
{
    public static ControllerDataRequester Instance;
    public float RequestIntervalInSeconds = 1f;
    private float timer;
    private bool timerActive;

    private void Start()
    {
        Instance = this;
    }
    // Update is called once per frame
    void Update()
    {
        if (!timerActive)
            return;
        if (timer >= 0)
            timer -= Time.deltaTime;
        if (timer <= 0)
        {
            timer = RequestIntervalInSeconds;
            DSUClient.Instance.SendActualControllerDataRequest(0);
            print("requested");
        }
    }
    public void StartRequesting()
    {
        timerActive = true;
    }
    public void StopRequesting()
    {
        timerActive = false;
    }
    [InspectorButton("OnButtonClicked3")]
    public bool start;
    private void OnButtonClicked3()
    {
        StartRequesting();
    }
    [InspectorButton("OnButtonClicked4")]
    public bool stop;
    private void OnButtonClicked4()
    {
        StopRequesting();
    }
}
