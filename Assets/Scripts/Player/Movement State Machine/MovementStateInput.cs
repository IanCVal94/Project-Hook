﻿using System.Collections.Generic;
using ASK.Core;
using ASK.Helpers;
using Mechanics;
using UnityEngine;

namespace Player
{
    public class MovementStateInput : PlayerStateInput {
        //Jump
        public GameTimer jumpBufferTimer;
        public bool jumpedFromGround;
        public bool canJumpCut;
        public bool canDoubleJump;

        //Dive
        public bool canDive;

        //Dogo
        public double oldVelocity;
        public GameTimerWindowed ultraTimer;

        public Vector3 diePos;
        
        public void RefillAbilities()
        {
            canDive = true;
            canDoubleJump = true;
        }
    }
}