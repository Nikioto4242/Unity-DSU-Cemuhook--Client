using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DSUClient;

public class dsuDataVisualizer : MonoBehaviour
{
    public bool Round = true;

    public byte Slot = 0;

    public float nX = 0;
    public float nY = 0;
    public float nZ = 0;

    public ulong totalMicroSec;//update only with accelerometer (but not gyro only) changes
    public float accelXG;
    public float accelYG;
    public float accelZG;

    // Gyro
    public float angVelPitch;
    public float angVelYaw;
    public float angVelRoll;

    private Vector3 initPos;
    private bool nInit;

    // Start is called before the first frame update
    void Start()
    {
        initPos = transform.position;


    }

    // Update is called once per frame
    private void Update()
    {
        var slot = ControllerData.controllerSlots[Slot];
        if (slot == null)
            return;
        var data = slot.controllerData;


        totalMicroSec = data.totalMicroSec;
        accelXG = data.accelXG;
        accelYG = data.accelYG;
        accelZG = data.accelZG;
        angVelPitch = data.angVelPitch;
        angVelYaw = data.angVelYaw;
        angVelRoll = data.angVelRoll;

        if (!nInit)
        {
            Reset();
        }

        var finx = accelXG - nX;
        //var finy = (float)Math.Round(accelYG - nY, 1);
        var finy = 0f;
        var finz = accelZG - nZ;

        if (Round)
        {
            finx = (float)Math.Round(finx, 1);
            finy = (float)Math.Round(finy, 1);
            finz = (float)Math.Round(finz, 1);
        }

        var a = new Vector3(finx, finy, finz);
        transform.position += a;

        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            Reset();
        }
    }

    private void Reset()
    {
        transform.position = initPos;
        nX = accelXG;
        nY = accelYG;
        nZ = accelZG;
        nInit = true;
    }
}
