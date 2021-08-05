using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Switch tiles in the game scene:
// =================================================
// SwitchTileManager (Parent GameObject):
//      1. SwitchTilesOffGroup (Child 1 GameObject):
//          a. Transparent Tile (GameObject)
//          b. Transparent Tile (GameObject)
//          c. etc.
//      2. SwitchTilesOnGroup (Child 2 GameObject)
//          a. Actual Tile (GameObject)
//          b. Actual Tile (GameObject)
//          c. etc.
// =================================================
// Transparent Tile
// - Sprite Renderer (Component)
// - Layer = default
// =================================================
// Regular Tile
// - Sprite Renderer (Component)
// - BoxCollider2D (Component)
// - Layer = Obstacle

public class SwitchTileManager : MonoBehaviour
{
    // Manage toggling between SwitchTilesOffGroup and SwitchTilesOnGroup
    public GameObject switchTilesOffGroup;
    public GameObject switchTilesOnGroup;
    public bool isActivated = false;

    void Start() {
        if (switchTilesOffGroup && switchTilesOnGroup) {
            if (isActivated) {
                switchTilesOnGroup.SetActive(true);
                switchTilesOffGroup.SetActive(false);
            }
            else {
                switchTilesOnGroup.SetActive(false);
                switchTilesOffGroup.SetActive(true);
            }
        }
    }

    public void ToggleSwitchTiles() {
        isActivated = !isActivated;
        if (isActivated == true) {
            ActivateSwitchTiles();
        }
        else {
            DeactivateSwitchTiles();
        }
    }

    private void ActivateSwitchTiles() {
        if (switchTilesOffGroup && switchTilesOnGroup) {
            switchTilesOffGroup.SetActive(false);
            switchTilesOnGroup.SetActive(true);
        }
    }

    private void DeactivateSwitchTiles() {
        if (switchTilesOffGroup && switchTilesOnGroup) {
            switchTilesOnGroup.SetActive(false);
            switchTilesOffGroup.SetActive(true);
        }
    }

}
