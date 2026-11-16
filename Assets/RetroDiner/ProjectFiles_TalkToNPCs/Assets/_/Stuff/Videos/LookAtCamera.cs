using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour {


    public enum Method {
        LookAt,
        LookAtInverted,
        CameraForward,
        CameraForwardInverted
    }


    [SerializeField] private Method method;


    private Transform mainCameraTransform;


    private void Awake() {
        mainCameraTransform = Camera.main.transform;
    }

    private void Update() {
        LookAt();
    }

    private void OnEnable() {
        LookAt();
    }

    private void LookAt() {
        switch (method) {
            default:
            case Method.LookAt:
                transform.LookAt(mainCameraTransform.position);
                break;
            case Method.LookAtInverted:
                Vector3 dir = (transform.position - mainCameraTransform.position).normalized;
                transform.LookAt(transform.position + dir);
                break;
            case Method.CameraForward:
                transform.forward = mainCameraTransform.forward;
                break;
            case Method.CameraForwardInverted:
                transform.forward = -mainCameraTransform.forward;
                break;
        }
    }

}
