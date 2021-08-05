using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    // Has a "trigger zone"
    // public BoxCollider2D doorTriggerZone;

    // Has a "collision box"
    public PolygonCollider2D doorCollider;

    // Has a sprite renderer
    private SpriteRenderer doorSpriteRend;

    public bool isLocked = false;

    public Sprite door_closed;
    public List<Sprite> door_open;

    public bool OpenDoor(bool isOpeningRight, bool unlockDoor)
    {
        bool unlockedDoor = false;

        // Check if door is unlocked
        if (isLocked)
        {
            if (unlockDoor) {
                isLocked = false;
                unlockedDoor = true;
            }
            else
                return false;
        }

        // Disable Door Collider
        doorCollider.enabled = false;
        // Update Sprite
        if (!doorSpriteRend)
            doorSpriteRend = GetComponent<SpriteRenderer>();
        if (door_open.Count != 2)
            Debug.Log("Door is missing sprites");
        else if (isOpeningRight)
            doorSpriteRend.sprite = door_open[0];
        else
            doorSpriteRend.sprite = door_open[1];

        return unlockedDoor;
    }

    public void CloseDoor()
    {
        // Enable Door Collider
        doorCollider.enabled = true;
        // Update Sprite
        if (!doorSpriteRend)
            doorSpriteRend = GetComponent<SpriteRenderer>();
        if (door_closed)
            doorSpriteRend.sprite = door_closed;
        else
            Debug.Log("Missing door closed sprite");
    }
}
