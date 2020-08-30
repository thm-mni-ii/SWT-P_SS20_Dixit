/*Created by: LifeIsJourney on GitHub (https://github.com/LifeIsJourney/Scripts/blob/master/TiggleUISimple.cs)*/
/* Toggle Prefab based on: https://github.com/Kalxoznik/Unity-Toggle-controller/blob/master/Toggle/Assets/Prefabs/Toggle.prefab */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleController : MonoBehaviour
{
    public bool isOn;

    public GameObject handle;
    private RectTransform handleTransform;
    private float handleSize;
    public RectTransform toggle;
    private float onPosX;
    private float offPosX;
    public float handleOffset;

    public GameObject onIcon;
    public GameObject offIcon;

    public float moveSpeed;
    public float t = 0.0f;
    private bool switching = false;

    public void Awake()
    {
        handleTransform = handle.GetComponent<RectTransform>();
        
        handleSize = handleTransform.sizeDelta.x;
        float toggleSizeX = toggle.sizeDelta.x;
        onPosX = (toggleSizeX / 2) - (handleSize / 2) - handleOffset;
        offPosX = onPosX * -1;
    }
    // Start is called before the first frame update
    void Start()
    {
        if (isOn)
        {
            handleTransform.localPosition = new Vector3(onPosX, 0, 0);
            onIcon.gameObject.SetActive(true);
            offIcon.gameObject.SetActive(false);
        }
        else
        {
            handleTransform.localPosition = new Vector3(offPosX, 0, 0);
            onIcon.gameObject.SetActive(false);
            offIcon.gameObject.SetActive(true);

        }
    }

    // Update is called once per frame
    void Update()
    {
        if (switching)
        {
            StartToggleing(isOn);
        }
    }

    private void StartToggleing(bool tStatus)
    {
        if (!onIcon.activeSelf || !offIcon.activeSelf)
        {
            onIcon.gameObject.SetActive(true);
            offIcon.gameObject.SetActive(false);
        }
        if (tStatus)
        {
            handleTransform.localPosition = SmoothlyMove(handle, onPosX, offPosX);
            onIcon.gameObject.SetActive(false);
            offIcon.gameObject.SetActive(true);
        }
        else
        {
            handleTransform.localPosition = SmoothlyMove(handle, offPosX, onPosX);
            onIcon.gameObject.SetActive(true);
            offIcon.gameObject.SetActive(false);
        }
    }

    private Vector3 SmoothlyMove(GameObject handle, float startPosX, float endPosX)
    {
        Vector3 position = new Vector3(Mathf.Lerp(startPosX, endPosX, t += moveSpeed * Time.deltaTime), 0, 0);
        StopSwitching();
        return position;
    }

	/* edited by: SWT-P_SS_20_Dixit */
    void StopSwitching()
    {
        if (t > 1.0f)
        {
            switching = false;
            t = 0.0f;
            isOn = !isOn;
        }
    }
    public void Switch()
    {
        switching = true;
    }
}