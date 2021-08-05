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
    public GameObject switchTiles;

    void Awake() {
        if (isActivated)
            ToggleButtonDown(true);
        else
            ToggleButtonUp(true);
    }

    public void ToggleButtonDown(bool shouldToggleSwitchTiles) {
        isActivated = true;
        GetComponent<SpriteRenderer>().sprite = buttonDownSprite;
        GetComponent<BoxCollider2D>().enabled = false;
        buttonUpCollider.enabled = false;
        buttonDownCollider.enabled = true;
        if (switchTiles != null && shouldToggleSwitchTiles) {
            switchTiles.GetComponent<SwitchTileManager>().ToggleSwitchTiles();
        }
    }

    public void ToggleButtonUp(bool shouldToggleSwitchTiles) {
        isActivated = false;
        GetComponent<SpriteRenderer>().sprite = buttonUpSprite;
        GetComponent<BoxCollider2D>().enabled = true;
        buttonUpCollider.enabled = true;
        buttonDownCollider.enabled = false;
        if (switchTiles != null && shouldToggleSwitchTiles) {
            switchTiles.GetComponent<SwitchTileManager>().ToggleSwitchTiles();
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (isActivated == false && other.tag == "hazard") {
            ToggleButtonDown(true);
        }
    }
}
