using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour {

    [Header("Components")]
    [HideInInspector]
    public BoxCollider2D boxCollider;

    [Header("Struct Objects")]
    public RaycastOrigins raycastOrigins;

    [Header("Collision Mask")]
    public LayerMask collisionMask;

    [Header("Platform Mask")]
    public LayerMask platformMask;

    [Header("Ray Information")]
    public int horizontalRayCount = 4;
    public int verticalRayCount = 5; // must be odd for corner correction to work correctly
    public int diagonalRayCount = 4;
    [Range(skinWidth * 2.5f, 0.5f)] public float wallDetectionRayLength = skinWidth * 2.5f;

    [Header("Other Variables")]
    [HideInInspector]
    public const float skinWidth = .015f;
    [HideInInspector]
    public float horizontalRaySpacing;
    [HideInInspector]
    public float verticalRaySpacing;

    public virtual void Start() {
        boxCollider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    public void UpdateRaycastOrigins() {
        Bounds bounds = boxCollider.bounds;
        // So that the bounds are shrunken inward:
        bounds.Expand(skinWidth * -2); // Causes the origin of our rays to be slightly INSIDE of the players box boxCollider which is good because (?)

        // So if you think of a box, this will represent the raycast origin:
        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);    // from the bottom left vertex
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y); // from the bottom right vertex
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);       // from the top left vertex
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);      // from the top right vertex
    }

    public void CalculateRaySpacing() {
        Bounds bounds = boxCollider.bounds;
        // So that the bounds are shrunken inward:
        bounds.Expand(skinWidth * -2); // Causes the origin of our rays to be slightly INSIDE of the players box boxCollider which is good because (?)

        // This will make sure that at least 2 rays are being fired:
        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);  // horizontally
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);      // vertically
        diagonalRayCount = Mathf.Clamp(diagonalRayCount, 2, int.MaxValue);

        // This will evenly distribute rays:
        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);        // horizontally by dividing the BoxCollider2D's y axis by the total number of horizontal rays being fired - 1
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);            // vertically by dividing the BoxCollider2D's x-axis by the total number of vertical rays being fired - 1
    }

    public struct RaycastOrigins {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }
}
