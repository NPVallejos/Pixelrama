using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Controller2D : RaycastController {

    [Header("Struct Objects")]
    public CollisionInfo collisions;

    public float tileX = 1;
    public float tileY = 1;

    public bool ignoreVerticalCollision = false;
    public bool cornerCorrectionEnabled = false;

    public override void Start() {
        base.Start();
        collisions.facingRight = true;
    }

    public void SetFacingRight(bool toggle) {
        collisions.facingRight = toggle;
    }

    public void Move(Vector2 moveAmount, bool isDashing, bool playerInputDown = false, bool ignoreLeftWallDetectionRays = false, bool ignoreRightWallDetectionRays = false, bool standingOnPlatform = false) {
        UpdateRaycastOrigins();
        // if (moveAmount.x > 0) { collisions.facingRight = true; }
        // else if (moveAmount.x < 0) { collisions.facingRight = false; }

        collisions.ResetWallBools();
        WallDetectionRays(ignoreLeftWallDetectionRays, ignoreRightWallDetectionRays);

        // Theres got to be a better way to refactor these two if blocks
        if (moveAmount.x != 0) {
            collisions.ResetHorizontalBools();
            HorizontalCollision(ref moveAmount);
        }

        // Two-way platform will work if this returns true
        ignoreVerticalCollision = false;
        if (playerInputDown) {
            CheckForPlatform(ref moveAmount);
        }

        if (moveAmount.y != 0) {
            collisions.ResetVerticalBools();
            if (ignoreVerticalCollision == false)
                VerticalCollision(ref moveAmount, playerInputDown);
        }

        if (isDashing && moveAmount.y == 0) {
            CastDownwardRaysWhileDashingHorizontally();
        }

        if (moveAmount.x < 0)
            DiagonalCollisionLeft(ref moveAmount);
        else if (moveAmount.x > 0)
            DiagonalCollisionRight(ref moveAmount);

        transform.Translate(moveAmount);

        if (standingOnPlatform)
            collisions.below = true;
    }

    /*
    1. Inside this function, perform two seperate 2D boxcasts
        A. The first boxcast checks on the platformCollision layer
        B. The second boxcast checks on the obstacleCollision layer
    2. The player will ignore vertical casts if (A == true && B == false)
    */
    void CheckForPlatform(ref Vector2 moveAmount) {

        Vector2 boxOrigin = new Vector2(transform.position.x + boxCollider.offset.x, transform.position.y + boxCollider.offset.y);
        Vector2 direction = new Vector2(0, Mathf.Sign(moveAmount.y));
        Vector2 size = new Vector2(boxCollider.size.x + skinWidth * -2, boxCollider.size.y);

        RaycastHit2D hitPlatform = Physics2D.BoxCast(boxOrigin, size, 0.0f, direction, 0, platformMask);
        RaycastHit2D hitObstacle = Physics2D.BoxCast(boxOrigin, size, 0.0f, direction, 0, collisionMask);

        if (hitPlatform && hitObstacle == false)
        {
            ignoreVerticalCollision = true;
        }
    }

    public void CastDownwardRaysWhileDashingHorizontally() {
        collisions.ResetVerticalBools();
        Vector2 dummy = Vector2.up * -skinWidth;
        VerticalCollision(ref dummy);
    }

    public void WallDetectionRays(bool ignoreLeftRays, bool ignoreRightRays) {
        if (ignoreLeftRays)
            FireRaysRight();
        else if (ignoreRightRays)
            FireRaysLeft();
        else
            FireRaysHorizontallyInBothDirections();
    }

    private void FireRaysHorizontallyInBothDirections() {
        for (int i = 0; i < horizontalRayCount; i++) {
            Vector2 rayOriginLeft  = raycastOrigins.bottomLeft;
            Vector2 rayOriginRight = raycastOrigins.bottomRight;
            rayOriginLeft  += Vector2.up * (horizontalRaySpacing * i);
            rayOriginRight += Vector2.up * (horizontalRaySpacing * i);

            RaycastHit2D hitLeft  = Physics2D.Raycast(rayOriginLeft, -Vector2.right, wallDetectionRayLength, collisionMask);
            RaycastHit2D hitRight = Physics2D.Raycast(rayOriginRight, Vector2.right, wallDetectionRayLength, collisionMask);

            Debug.DrawRay(rayOriginLeft, -Vector2.right * wallDetectionRayLength, Color.red);
            Debug.DrawRay(rayOriginRight, Vector2.right * wallDetectionRayLength, Color.red);

            if (hitLeft)
                    collisions.wallIsLeft = true;

            if (hitRight)
                    collisions.wallIsRight = true;
        }
    }

    private void FireRaysRight() {
        for (int i = 0; i < horizontalRayCount; i++) {
            Vector2 rayOriginRight = raycastOrigins.bottomRight;
            rayOriginRight += Vector2.up * (horizontalRaySpacing * i);

            RaycastHit2D hitRight = Physics2D.Raycast(rayOriginRight, Vector2.right, wallDetectionRayLength, collisionMask);

            Debug.DrawRay(rayOriginRight, Vector2.right * wallDetectionRayLength, Color.red);

            if (hitRight)
                    collisions.wallIsRight = true;
        }
    }

    private void FireRaysLeft() {
        for (int i = 0; i < horizontalRayCount; i++) {
            Vector2 rayOriginLeft  = raycastOrigins.bottomLeft;
            rayOriginLeft  += Vector2.up * (horizontalRaySpacing * i);

            RaycastHit2D hitLeft  = Physics2D.Raycast(rayOriginLeft, -Vector2.right, wallDetectionRayLength, collisionMask);

            Debug.DrawRay(rayOriginLeft, -Vector2.right * wallDetectionRayLength, Color.red);

            if (hitLeft)
                    collisions.wallIsLeft = true;
        }
    }

    void HorizontalCollision(ref Vector2 moveAmount) {
        float directionX = Mathf.Sign(moveAmount.x);               // If we are moving left, directionX = -1, else directionX = 1
        float rayLength = Mathf.Abs(moveAmount.x) + skinWidth;    // force moveAmount.x to be positive

        for (int i = 0; i < horizontalRayCount; i++) {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;        // If we are moving left then we want our ray to start in the bottom left corner, else we are moving right and we set ray origin to bottom right
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
            // If our raycast hits something then the first thing we want to do is set our x moveAmount to the amount we have to move to get from our current position to the point at which the ray intersected with an obstacle; essentially the ray distance
            if (hit) {
                moveAmount.x = (hit.distance - skinWidth) * directionX;
                rayLength = hit.distance; // we change our ray length to the distance between our raycast origin and the object
                collisions.left  = (directionX == -1); // if we are moving left then directionX = -1
                collisions.right = (directionX == 1); // ^^

                collisions.wallIsLeft = collisions.left;
                collisions.wallIsRight = collisions.right;
            }
            //Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.white);
        }
    }

    public void VerticalCollision(ref Vector2 moveAmount, bool playerInputDown = false) {
		float directionY = Mathf.Sign (moveAmount.y);
		float rayLength = Mathf.Abs (moveAmount.y) + skinWidth;

        // Var's for corner correction
        bool hitLeftCorner = false;
        bool hitRightCorner = false;
        bool hitMiddleRays = false;
        float hitPointX = 0f;
        float middleRayHitPointX = 0f;
        float prevMoveAmountY = moveAmount.y;

		for (int i = 0; i < verticalRayCount; i += 1) {
			Vector2 rayOrigin = (directionY == -1)?raycastOrigins.bottomLeft:raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);
			RaycastHit2D hit;
            if (directionY > 0 || playerInputDown)
                hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
            else {
                hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, platformMask | collisionMask);
            }

			//Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength,Color.red);

			if (hit) {
				moveAmount.y = (hit.distance - skinWidth) * directionY;
				rayLength = hit.distance;

				collisions.below = directionY == -1;
				collisions.above = directionY == 1;

                // Determine if first two rays/last two rays/middle rays hit
                // This determines if corner correction occurs
                // WARNING: THIS ASSUMES THAT THERE ARE 5 VERTICAL RAYS BEING CASTED
                // MAY BE ERROR PRONE TO HIGHER AMOUNTS
                if (directionY > 0 && cornerCorrectionEnabled) {
                    if (i <= 1 && !hitRightCorner) {
                        hitRightCorner = true;
                        hitPointX = hit.point.x;
                    }
                    if (i > 1 && i < verticalRayCount - 2) {
                        hitMiddleRays = true;
                        middleRayHitPointX = hit.point.x;
                    }
                    if (i >= verticalRayCount - 2) {
                        hitLeftCorner = true;
                        hitPointX = hit.point.x;
                    }
                }
			}
		}
        // To prevent corner correction with middle ray add the following:
        // if (hitLeftCorner != hitRightCorner && !hitMiddleRays)
        // Account for hitting corner of tile
        if (hitLeftCorner != hitRightCorner) {
            //Debug.LogFormat("hitLeftCorner = {0}, hitRightCorner = {1}, hitMid={2}", hitLeftCorner, hitRightCorner, hitMiddleRays);

            float cornerPointX = 0f;
            if (hitMiddleRays) cornerPointX = ComputeCornerX(middleRayHitPointX);
            else cornerPointX = ComputeCornerX(hitPointX);

            if (moveAmount.x == 0 ||
                (hitRightCorner && moveAmount.x > 0) ||
                (hitLeftCorner && moveAmount.x < 0)) {

                float distanceToCornerFromHitPoint = cornerPointX - hitPointX;

                float newPositionXSign = Mathf.Sign(distanceToCornerFromHitPoint);
                float newPlayerPositionX = Mathf.Abs(distanceToCornerFromHitPoint) + skinWidth;

                //Debug.LogFormat("cornerPointX: {0}\nhitPointX: {1}\ndistanceToCorner: {2}\nnewPlayerPosX: {3}", cornerPointX, hitPointX, distanceToCornerFromHitPoint, (transform.position.x + (newPositionXSign * newPlayerPositionX)));

                transform.position = new Vector2(transform.position.x + (newPositionXSign * newPlayerPositionX), transform.position.y);
                collisions.above = false;
                moveAmount.y = prevMoveAmountY;
            }
        }
	}

    float ComputeCornerX(float point) {
        float sign = Mathf.Sign(point);
        float halfTileX = tileX * 0.5f;
        float offset = Mathf.Abs(point) + halfTileX;
        float offsetRounded = (int)offset;
        if (offset > offsetRounded + tileX)
            return sign * (offsetRounded + tileX);
        else
            return sign * offsetRounded;
    }

    void DiagonalCollisionLeft(ref Vector2 moveAmount)
    {
        Vector2 rayOrigin = Vector2.zero;
        if (moveAmount.x < 0 && moveAmount.y < 0)
        {
            rayOrigin = raycastOrigins.bottomLeft;
        }
        else if (moveAmount.x < 0 && moveAmount.y > 0)
        {
            rayOrigin = raycastOrigins.topLeft;
        }

        float slope = Mathf.Abs(moveAmount.x) / Mathf.Abs(moveAmount.y);
        float diagonalSkinWidth;
        if (slope <= 1)
        {
            diagonalSkinWidth = new Vector2(skinWidth * slope, skinWidth).magnitude;
        }
        else
        {
            diagonalSkinWidth = new Vector2(skinWidth, (1 / slope) * skinWidth).magnitude;
        }

        float rayLength = moveAmount.magnitude + diagonalSkinWidth;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, moveAmount, rayLength, collisionMask);

        //Debug.DrawRay(rayOrigin, moveAmount.normalized * rayLength, Color.white);
        if (hit)
        {
            moveAmount = moveAmount.normalized * (hit.distance - diagonalSkinWidth);
        }
    }

    void DiagonalCollisionRight(ref Vector2 moveAmount)
    {
        Vector2 rayOrigin = Vector2.zero;
        if (moveAmount.x > 0 && moveAmount.y < 0)
        {
            rayOrigin = raycastOrigins.bottomRight;
        }
        else if (moveAmount.x > 0 && moveAmount.y > 0)
        {
            rayOrigin = raycastOrigins.topRight;
        }

        float slope = Mathf.Abs(moveAmount.x) / Mathf.Abs(moveAmount.y);
        float diagonalSkinWidth;
        if (slope <= 1)
        {
            diagonalSkinWidth = new Vector2(skinWidth * slope, skinWidth).magnitude;
        }
        else
        {
            diagonalSkinWidth = new Vector2(skinWidth, (1 / slope) * skinWidth).magnitude;
        }

        float rayLength = moveAmount.magnitude + diagonalSkinWidth;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, moveAmount, rayLength, collisionMask);

        //Debug.DrawRay(rayOrigin, moveAmount.normalized * rayLength, Color.white);
        if (hit)
        {
            moveAmount = moveAmount.normalized * (hit.distance - diagonalSkinWidth);
        }
    }

    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;

        public bool facingRight;
        public bool wallIsLeft;
        public bool wallIsRight;

        public void ResetHorizontalBools() => left = right = false;
        public void ResetVerticalBools() => above = below = false;
        public void ResetWallBools() => wallIsLeft = wallIsRight = false;
    }
}


/*
Important code:

public LayerMask collisionMask;
public LayerMask platformMask;

public void Move(Vector3 velocity, bool playerInputDown = false)
{
    UpdateRaycastOrigins();
    collisions.Reset();         // We will set our collisions in HorizontalCollision and VerticalCollision so we can reset this now

    ignoreVerticalCollision = false;

    // Here we handle horizontal and vertical collisions
    // We only want to handle horizontal and vertical collisions if we are moving horizontally or vertically (i.e. velocity.x != 0 and velocity.y != 0 respectively)
    if (velocity.x != 0)
        HorizontalCollision(ref velocity);

    // ***************************************
    // THIS IS WHERE TWO-WAY PLATFORMS WORK
    // CheckForPlatform() function will set 'ignoreVerticalCollision' to TRUE and will disable VerticalCollision() call for one frame
    // Platforms collider NEEDS TO BE AN EDGE COLLIDER for this to work
    if (playerInputDown)
        CheckForPlatform(ref velocity);
    if (velocity.y != 0 && ignoreVerticalCollision == false)
        VerticalCollision(ref velocity); // We are passing in a reference to our velocity
    // ******************************************

    if (velocity.x < 0 && velocity.y != 0)
        DiagonalCollisionLeft(ref velocity);
    else if (velocity.x > 0 && velocity.y != 0)
        DiagonalCollisionRight(ref velocity);

    transform.Translate(velocity);
}

void CheckForPlatform(ref Vector3 velocity)
{
    // 1. Inside this function, perform two seperate 2D boxcasts
    //     A. The first boxcast checks on the platformCollision layer
    //     B. The second boxcast checks on the obstacleCollision layer
    // 2. The player will ignore vertical casts if (A == true && B == false)

    Vector2 boxOrigin = new Vector2(transform.position.x + boxCollider.offset.x, transform.position.y + boxCollider.offset.y);
    Vector2 direction = new Vector2(0, Mathf.Sign(velocity.y));
    Vector2 size = new Vector2(boxCollider.size.x + skinWidth * -2, boxCollider.size.y);

    RaycastHit2D hitPlatform = Physics2D.BoxCast(boxOrigin, size, 0.0f, direction, 0, platformMask);
    RaycastHit2D hitObstacle = Physics2D.BoxCast(boxOrigin, size, 0.0f, direction, 0, collisionMask);

    if (hitPlatform && hitObstacle == false)
    {
        ignoreVerticalCollision = true;
    }
}
*/
