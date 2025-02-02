﻿using System;
using System.Collections.Generic;
using ASK.Core;
using ASK.Helpers;
using UnityEngine;

namespace A2DK.Phys {
    [RequireComponent(typeof(Collider2D))]
    public abstract class PhysObj : MonoBehaviour {
        protected BoxCollider2D myCollider { get; private set; }
        public Vector2 velocity { get; protected set; }  = Vector2.zero;
        
        [NonSerialized] public Vector2 NextFrameOffset = Vector2.zero;
        [NonSerialized] private Vector2 MoveRemainder = Vector2.zero;

        public float velocityY {
            get { return velocity.y; }
            protected set { velocity = new Vector2(velocity.x, value); }
        }

        public float velocityX {
            get { return velocity.x; }
            protected set { velocity = new Vector2(value, velocity.y); }
        }

        protected virtual void Start() {
            myCollider = GetComponent<BoxCollider2D>();
            Game.TimeManager.ResetNextFrameOffset += ResetNextFrameOffset;
        }
        
        /**
         * Move velocity * time.
         */
        protected void MoveTick() => Move(velocity * Game.TimeManager.FixedDeltaTime);

        private void ResetNextFrameOffset() {
            NextFrameOffset = Vector2.zero;
        }

        public bool CheckCollisionsAll(Func<PhysObj, Vector2, bool> onCollide)
        {
            //L: lmao
            if (CheckCollisions(Vector2.right, onCollide))
            {
                return true;
            }

            if (CheckCollisions(Vector2.down, onCollide))
            {
                return true;
            }

            if (CheckCollisions(Vector2.left, onCollide))
            {
                return true;
            }

            if (CheckCollisions(Vector2.up, onCollide))
            {
                return true;
            }

            return false;
        }

        public bool IsOverlapping(PhysObj p)
        {
            return CheckCollisions(Vector2.zero, (checkCol, dir) => { return p == checkCol; });
        }

        /// <summary>
        /// See CheckCollisions<T>
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="onCollide"></param>
        /// <returns></returns>
        public PhysObj CheckCollisions(Vector2 direction, Func<PhysObj, Vector2, bool> onCollide) =>
            CheckCollisions<PhysObj>(direction, onCollide);
        
        /// <summary>
        /// Checks the interactable layer for any collisions. Will call onCollide if it hits anything.
        /// </summary>
        /// <param name="direction"><b>MUST</b> be a cardinal direction with a <b>magnitude of one.</b></param>
        /// <param name="onCollide">(<b>physObj</b> collided with, <b>Vector2</b> direction),
        /// returns physObj when collide, otherwise null.</param>
        /// <typeparam name="T">Type of physObj to check against. Must inherit from PhysObj.</typeparam>
        /// <returns></returns>
        public T CheckCollisions<T>(Vector2 direction, Func<T, Vector2, bool> onCollide) where T : PhysObj{
            Vector2 colliderSize = myCollider.size;
            Vector2 sizeMult = colliderSize - Vector2.one;
            List<RaycastHit2D> hits = new List<RaycastHit2D>();
            ContactFilter2D filter = new ContactFilter2D();
            filter.layerMask = LayerMask.GetMask("Interactable", "Ground", "Player");
            filter.useLayerMask = true;
            Physics2D.BoxCast(transform.position, sizeMult, 0, direction, filter, hits, 8f);

            List<T> collideTs = new();
            
            foreach (var hit in hits) {
                if (hit.transform == transform)
                {
                    continue;
                }
                var t = hit.transform.GetComponent<T>();
                if (t != null)
                {
                    collideTs.Add(t);
                }
            }
            
            foreach (var s in collideTs)
            {
                bool proactiveCollision = ProactiveBoxCast(
                    s.transform, 
                    s.NextFrameOffset,
                    sizeMult,
                    1,
                    direction, 
                    filter
                );
                if (proactiveCollision)
                {
                    bool col = onCollide(s, direction);
                    if (col)
                    {
                        return s;
                    }
                }
            }
            
            return null;
        }

        public bool ProactiveBoxCast(Transform checkAgainst, Vector3 nextFrameOffset, Vector2 sizeMult, float dist, Vector2 direction, ContactFilter2D filter) {
            List<RaycastHit2D> hits = new List<RaycastHit2D>();
            int numHits = Physics2D.BoxCast(
                transform.position - nextFrameOffset,
                size: sizeMult, 
                angle: 0, 
                direction: direction, 
                distance: dist,
                results: hits, 
                contactFilter: filter
            );
            foreach (var hit in hits) {
                if (hit.transform == checkAgainst) {
                    return true;
                }
            }
            return false;
        }
        
        private void OnDrawGizmosSelected() {
            Vector2 direction = velocity == Vector2.zero ? Vector2.up: velocity.normalized;
            var col = GetComponent<BoxCollider2D>();
            if (col == null) return;
            Vector2 colliderSize = col.size;
            Vector2 sizeMult = colliderSize - Vector2.one;
            // Vector2 sizeMult = colliderSize;
            BoxDrawer.DrawBoxCast2D(
                origin: (Vector2) transform.position,
                size: sizeMult,
                direction: direction,
                distance: 1,
                angle: 0,
                color: Color.blue
            );
        }

        protected void Move(Vector2 vel) {
            vel += MoveRemainder;
            int moveX = (int) Math.Abs(vel.x);
            if (moveX != 0) {
                Vector2 xDir = new Vector2(vel.x / moveX, 0).normalized;
                MoveGeneral(xDir, moveX, OnCollide);
            }

            int moveY = (int) Math.Abs(vel.y);
            if (moveY != 0) {
                Vector2 yDir = new Vector2(0, vel.y / moveY).normalized;
                MoveGeneral(yDir, moveY, OnCollide);
            }

            Vector2 truncVel = new Vector2((int) vel.x, (int) vel.y);
            MoveRemainder = vel - truncVel;
        }
        public abstract bool MoveGeneral(Vector2 direction, int magnitude, Func<PhysObj, Vector2, bool> onCollide);
        
        public abstract bool Collidable(PhysObj collideWith);
        public virtual bool OnCollide(PhysObj p, Vector2 direction) {
            return Collidable(p);
        }

        /*public virtual bool PlayerCollide(Actor p, Vector2 direction) {
            return OnCollide(p, direction);
        }*/

        public virtual bool IsGround(PhysObj whosAsking) {
            return Collidable(whosAsking);
        }

        //TODO: change this so that it only looks for actors near me
        public static Actor[] AllActors() {
            return FindObjectsOfType<Actor>();
        }
        
        public abstract bool Squish(PhysObj p, Vector2 d);
        
        /**
         * Gets the physObj underneath this PhysObj's feet.
         */
        public PhysObj GetBelowPhysObj() => CheckCollisions(Vector2.down, (p, d) => this.Collidable(p));

        /**
         * Calculates the physics object this PhysObj is riding on.
         */
        public virtual PhysObj RidingOn() => GetBelowPhysObj();

        public int ColliderBottomY() => Convert.ToInt16(transform.position.y + myCollider.offset.y - myCollider.bounds.extents.y);
        
        public int ColliderTopY() => Convert.ToInt16(transform.position.y + myCollider.offset.y + myCollider.bounds.extents.y);
    }
}