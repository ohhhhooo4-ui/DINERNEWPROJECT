using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFramerate : MonoBehaviour {

    [SerializeField] private int targetFrameRate = 144;

    private void Awake() {
        Application.targetFrameRate = targetFrameRate;
    }

}