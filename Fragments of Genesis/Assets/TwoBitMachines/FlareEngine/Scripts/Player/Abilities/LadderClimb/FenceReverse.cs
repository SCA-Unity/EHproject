using TwoBitMachines.FlareEngine.Interactables;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.ThePlayer
{
        public class FenceReverse
        {
                [System.NonSerialized] private SpriteRenderer ladderRenderer;
                [System.NonSerialized] private SpriteRenderer characterRenderer;
                [System.NonSerialized] private int characterSortingLayer;
                [System.NonSerialized] private int characterOrderLayer;

                [System.NonSerialized] private float edgeCounter;
                [System.NonSerialized] private bool initialized;
                [System.NonSerialized] private bool isFlippingUp;
                [System.NonSerialized] private bool isFlippingDown;
                [System.NonSerialized] private bool isFlippingLeft;
                [System.NonSerialized] private bool isFlippingRight;

                public bool isFlippingX => isFlippingLeft || isFlippingRight;
                public bool isFlippingY => isFlippingUp || isFlippingDown;
                public bool isFlipping => isFlippingX || isFlippingY;

                public void Reset (AbilityManager player)
                {
                        if (initialized)
                        {
                                characterRenderer.sortingLayerID = characterSortingLayer;
                                characterRenderer.sortingOrder = characterOrderLayer - 1;
                        }
                        StopFlip ( );
                }

                public void StopFlip ( )
                {
                        isFlippingUp = false;
                        isFlippingDown = false;
                        isFlippingLeft = false;
                        isFlippingRight = false;
                }

                public void Flip (AbilityManager player, LadderClimb ladderClimb, TwoBitMachines.FlareEngine.Interactables.Ladder ladder, ref Vector2 velocity)
                {
                        if (!ladder.canFlip || Time.deltaTime == 0)
                        {
                                return;
                        }
                        if (player.world.touchingASurface)
                        {
                                Reset (player);
                        }

                        if (ladder.canFlipX)
                        {
                                IsFlippingX (player, ladderClimb, ladder.reverseTime, ladder.ladder, ref velocity);
                        }
                        if (ladder.canFlipY)
                        {
                                IsFlippingY (player, ladderClimb, ladder.reverseTime, ladder.ladder, ref velocity);
                        }
                }

                private void IsFlippingX (AbilityManager player, LadderClimb ladderClimb, float reverseTime, LadderInstance ladder, ref Vector2 velocity)
                {
                        Vector2 playerCenter = ladderClimb.playerCenter;
                        Vector2 playerCenterOriginal = playerCenter;
                        playerCenter += velocity * Time.deltaTime;

                        if (!isFlippingX && (ladderClimb.playerBottom + 0.1f) >= ladder.top)
                        {
                                return;
                        }

                        if (!isFlippingLeft && !isFlippingY && playerCenter.x <= (ladder.left + 0.15f))
                        {
                                isFlippingLeft = true;
                                edgeCounter = 0;
                        }
                        if (!isFlippingRight && !isFlippingY && playerCenter.x >= (ladder.right - 0.15f))
                        {
                                isFlippingRight = true;
                                edgeCounter = 0;
                        }

                        if (isFlippingX && !isFlippingY)
                        {
                                if (isFlippingLeft)
                                {
                                        HoldPositionLeft (ladder, playerCenterOriginal, ref velocity);
                                }

                                if (isFlippingRight)
                                {
                                        HoldPositionRight (ladder, playerCenterOriginal, ref velocity);
                                }

                                if (Clock.Timer (ref edgeCounter, reverseTime))
                                {
                                        ladderClimb.hasFlipped = !ladderClimb.hasFlipped;
                                        SetLayerOrder (player, ladder, ladderClimb.hasFlipped);
                                        float direction = isFlippingLeft ? 1 : -1;
                                        player.world.transform.position += new Vector3 (direction * player.world.box.sizeX * 0.5f, 0, 0);
                                        isFlippingLeft = false;
                                        isFlippingRight = false;
                                }
                                else
                                {
                                        player.signals.Set ("flippingX");
                                        velocity.y = 0;
                                        if (Mathf.Abs (velocity.x) < 0.001f)
                                        {
                                                velocity.x = 0;
                                        }
                                }
                        }
                }

                private void IsFlippingY (AbilityManager player, LadderClimb ladderClimb, float reverseTime, LadderInstance ladder, ref Vector2 velocity)
                {
                        Vector2 playerCenter = ladderClimb.playerCenter;
                        Vector2 playerCenterOriginal = playerCenter;
                        playerCenter += velocity * Time.deltaTime;

                        if (!isFlippingUp && !isFlippingX && velocity.y > 0 && playerCenter.y >= (ladder.top - 0.15f))
                        {
                                isFlippingUp = true;
                                edgeCounter = 0;
                        }
                        if (!isFlippingDown && !isFlippingX && playerCenter.y <= (ladder.bottom + 0.15f))
                        {
                                isFlippingDown = true;
                                edgeCounter = 0;
                        }

                        if (isFlippingY && !isFlippingX)
                        {
                                if (isFlippingUp)
                                {
                                        HoldPositionUp (ladder, playerCenterOriginal, ref velocity);
                                }

                                if (isFlippingDown)
                                {
                                        HoldPositionDown (ladder, playerCenterOriginal, ref velocity);
                                }

                                if (Clock.Timer (ref edgeCounter, reverseTime))
                                {
                                        ladderClimb.hasFlipped = !ladderClimb.hasFlipped;
                                        SetLayerOrder (player, ladder, ladderClimb.hasFlipped);
                                        float direction = isFlippingUp ? -1 : 1;
                                        player.world.transform.position += new Vector3 (0, direction * player.world.box.sizeY * 0.25f, 0);
                                        isFlippingUp = false;
                                        isFlippingDown = false;
                                }
                                else
                                {
                                        player.signals.Set ("flippingY");
                                        velocity.x = 0;
                                        if (Mathf.Abs (velocity.y) < 0.001f)
                                        {
                                                velocity.y = 0;
                                        }
                                }
                        }
                }

                private void HoldPositionUp (LadderInstance ladder, Vector2 playerCenter, ref Vector2 velocity)
                {
                        velocity.y = (ladder.top - 0.15f - playerCenter.y) / Time.deltaTime;
                }

                private void HoldPositionLeft (LadderInstance ladder, Vector2 playerCenter, ref Vector2 velocity)
                {
                        velocity.x = (ladder.left + 0.15f - playerCenter.x) / Time.deltaTime;
                }

                private void HoldPositionRight (LadderInstance ladder, Vector2 playerCenter, ref Vector2 velocity)
                {
                        velocity.x = (ladder.right - 0.15f - playerCenter.x) / Time.deltaTime;
                }

                private void HoldPositionDown (LadderInstance ladder, Vector2 playerCenter, ref Vector2 velocity)
                {
                        velocity.y = (ladder.bottom + 0.15f - playerCenter.y) / Time.deltaTime;
                }

                public void SetLayerOrder (AbilityManager player, LadderInstance ladder, bool reversed)
                {
                        if (characterRenderer == null)
                        {
                                characterRenderer = player.world.transform.GetComponent<SpriteRenderer> ( );
                                if (characterRenderer != null)
                                {
                                        characterSortingLayer = characterRenderer.sortingLayerID;
                                        characterOrderLayer = characterRenderer.sortingOrder;
                                        initialized = true;
                                }
                        }
                        if (ladderRenderer == null && ladder.target != null)
                        {
                                ladderRenderer = ladder.target.GetComponent<SpriteRenderer> ( );
                        }
                        if (ladderRenderer == null || characterRenderer == null)
                        {
                                return;
                        }

                        if (reversed)
                        {
                                characterRenderer.sortingLayerID = ladderRenderer.sortingLayerID;
                                characterRenderer.sortingOrder = ladderRenderer.sortingOrder - 1;
                        }
                        else
                        {
                                characterRenderer.sortingLayerID = characterSortingLayer;
                                characterRenderer.sortingOrder = characterOrderLayer - 1;
                        }

                }

        }
}