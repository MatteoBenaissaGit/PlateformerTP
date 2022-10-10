using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraSwing : MonoBehaviour
{
    [Header("Cooldown")]
    [SerializeField] private bool dutchIsRight = true;
    [SerializeField] private bool cameraCanSwing = true;
    [SerializeField] private float swingEventCooldown = 8f;
    [SerializeField] private float minEventCooldown = 5f;
    [SerializeField] private float maxEventCooldown = 10f;

    [Header("Camera")]
    [SerializeField] private float cameraDutch = 0.0f;
    [SerializeField] private CinemachineVirtualCamera vcam;

    [SerializeField] private struct LensSettings { };
    [SerializeField] private float Dutch;

    private float swingEventAlarm = 0.0f;
    private float swingEventStopAlarm = 0.0f;

    private void Start()
    {
        StartSetup();
    }

    private void StartSetup()
    {
        swingEventAlarm = swingEventCooldown;
        vcam.GetComponent<Cinemachine.CinemachineVirtualCamera>();
        cameraDutch = vcam.m_Lens.Dutch;
    }

    private void Update()
    {
        CameraSwinging();   
    }

    private void SwingEventTimeSetup()
    {
        swingEventCooldown = Random.Range(minEventCooldown, maxEventCooldown);
        swingEventAlarm = swingEventCooldown;
    }

    private void CameraSwinging()
    {
        //alarm countdown
        swingEventAlarm -= Time.deltaTime;

        switch (dutchIsRight)
        {
            case true:
                if (cameraCanSwing)
                {
                    if (swingEventAlarm <= 0)
                    {
                        Debug.Log("Je vais à droite");
                        SwingEventTimeSetup();
                        dutchIsRight = false;
                    }
                    vcam.m_Lens.Dutch -= 0.01f;
                    vcam.m_Lens.Dutch = Mathf.Clamp(vcam.m_Lens.Dutch, -9, 9);
                }
                break;
            case false:
                if (cameraCanSwing)
                {
                    if(swingEventAlarm <= 0)
                    {
                        Debug.Log("Je vais à gauche");
                        SwingEventTimeSetup();
                        dutchIsRight = true;
                    }
                    vcam.m_Lens.Dutch += 0.01f;
                    vcam.m_Lens.Dutch = Mathf.Clamp(vcam.m_Lens.Dutch, -9, 9);
                }
                break;
        }

        if (vcam.m_Lens.Dutch <= 0.01 && vcam.m_Lens.Dutch >= -0.01 && cameraCanSwing)
        {
            var stop = Random.value;
            if (stop < 0.2)
            {
                cameraCanSwing = false;
                swingEventStopAlarm = swingEventCooldown;
                stop = -1; //disable condition
                Debug.Log(swingEventStopAlarm);
            }
        }

        swingEventStopAlarm -= Time.deltaTime;

        if (swingEventStopAlarm <= 0)
        {
            cameraCanSwing = true;
            //Debug.Log(cameraCanSwing);
        }
    }
}
