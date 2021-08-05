using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    // Interactions
    // Considered hit if it comes into contact with
    // - Fireball
    // - Player
    // Sends out signals when activated:
    // - Turn on/off switch blocks

    // Purpose
    // This script should change the appearance of the button based on its current state
    // It should also turn on/off switch blocks
    public bool isActivated;
    public Sprite buttonUpSprite;
    public Sprite buttonDownSprite;
    public PolygonCollider2D buttonUpCollider;
    public PolygonCollider2D buttonDownCollider;
    public SwitchTileManager switchTileManager;

    void Awake() {
        if (isActivated)
            ToggleButtonDown(true);
        else
            ToggleButtonUp(false);
    }

    public void ToggleButtonDown(bool toggleSwitchTiles) {
        isActivated = true;
        GetComponent<SpriteRenderer>().sprite = buttonDownSprite;
        GetComponent<BoxCollider2D>().enabled = false;
        buttonUpCollider.enabled = false;
        buttonDownCollider.enabled = true;
        if (toggleSwitchTiles && switchTileManager) {
            switchTileManager.ToggleSwitchTiles();
        }
    }

    public void ToggleButtonUp(bool toggleSwitchTiles) {
        isActivated = false;
        GetComponent<SpriteRenderer>().sprite = buttonUpSprite;
        GetComponent<BoxCollider2D>().enabled = true;
        buttonUpCollider.enabled = true;
        buttonDownCollider.enabled = false;
        if (toggleSwitchTiles && switchTileManager) {
            switchTileManager.ToggleSwitchTiles();
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (isActivated == false && other.tag == "hazard") {
            ToggleButtonDown(true);
        }
    }
}
