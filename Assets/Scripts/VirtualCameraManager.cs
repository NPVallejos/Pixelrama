using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class VirtualCameraManager : MonoBehaviour
{
    // Should have a list of virtual cameras
    // Should detect when the player has entered a new area
    // Should turn on and off corresponding virtual cameras

    public List<GameObject> virtualCameraList = null;
    public List<string> levelTags = null;
    public int currentIndex = 0;

    private void ChangeCamera(int targetIndex) {
        // 1. Turn off current virtual camera
        virtualCameraList[currentIndex].SetActive(false);
        // 3. Turn on target virtual Camera
        virtualCameraList[targetIndex].SetActive(true);
        // 3. Set current index to target index
        currentIndex = targetIndex;
    }

    void OnTriggerEnter2D(Collider2D collision) {
        // if (collision.tag == "level01") {
        //     ChangeCamera(0);
        // }
        // if (collision.tag == "level02") {
        //     ChangeCamera(1);
        // }
        // else if (collision.tag == "level03") {
        //     ChangeCamera(2);
        // }
        for (int i = 0; i < levelTags.Count; ++i) {
            if (collision.tag == levelTags[i]) {
                ChangeCamera(i);
            }
        }
    }
}
