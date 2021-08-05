using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    // MOVEMENT
    // The fireball should follow a set path
    // It goes from start to end
    // It can have arbitrary number of subpaths
    [Header("Waypoint Options")]
    public int startingWaypoint = 0;
    public List<Vector2> waypoints;
    [Space(10)]
    [Header("Movement Options")]
    public float movementSpeed = 1.0f;
    public bool allowLooping = true;

    private int count = 0;
    private bool isLooping = false;
    private InterpolatedTransform m_interpolatedTransform;

    void Start() {
        m_interpolatedTransform = GetComponent<InterpolatedTransform>();
        m_interpolatedTransform.enabled = false;
        if (startingWaypoint > 0 && startingWaypoint < waypoints.Count) {
            count = startingWaypoint;
        }
        if (waypoints.Count > 0) {
            transform.position = waypoints[count];
            count++;
        }
        m_interpolatedTransform.enabled = true;
    }

    void FixedUpdate() {
        // Handle Waypoint Movement
        if (waypoints.Count != 0) {
            // We want this to be done waypoints.Count number of times
            if (!isLooping && count < waypoints.Count) {
                transform.position = Vector2.MoveTowards(transform.position, waypoints[count], movementSpeed * Time.deltaTime);
            }
            else if (isLooping && count >= 0) {
                transform.position = Vector2.MoveTowards(transform.position, waypoints[count], movementSpeed * Time.deltaTime);
            }
            // Increment/decrement count once fireball reaches waypoints[count]
            if (transform.position.x == waypoints[count].x
            && transform.position.y == waypoints[count].y) {
                if (isLooping) {
                    if (count == 0)
                    isLooping = false;
                    else
                    count--;
                }
                else if (allowLooping && count == waypoints.Count - 1) {
                    isLooping = true;
                }
                else if (count < waypoints.Count - 1){
                    count++;
                }
                else {
                    // Reset fireball position to start & reset count
                    count = 0;
                    m_interpolatedTransform.enabled = false;
                    transform.position = waypoints[count];
                    m_interpolatedTransform.enabled = true;
                    ++count;
                }
            }
        }
    }


    // INTERACTIONS
    // It should kill the player on contact
    // It should activate a button on contact
}
