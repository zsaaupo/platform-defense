using UnityEngine;

// wanna be state machine for player animations
public class PlayerAnimator : SpriteAnimator {
    public float runningHorizontalSpeedThreshold;

    public float fallSpeedThreshold;
    public float jumpMidPointSpeedThreshold;
    public float jumpDownSlowSpeedThreshold;

    public ParticleSystem bounceParticles;

    private State state;
    
    private JumpPhase jumpPhase; 

    private string idleAnimName = "Idle";


    public void Refresh(Vector2 velocity, Vector2 input, bool grounded, bool wallSliding, bool jumped, bool wallJumped, bool bounced, bool dead) {

        // change state if needed:

        bool wasIdle = this.state == State.Idle;
        bool wasWallJumping = false;

        if(this.state == State.Dead && !dead) {
            this.state = State.Idle;
        }

        switch(this.state) {
            case State.Idle:
                {
                    if(jumped || (!grounded && Mathf.Abs(velocity.y) > this.fallSpeedThreshold)) {
                        this.state = State.Jump;
                    } else {
                        if(Mathf.Abs(input.x) > 0.4f) {
                            this.state = State.Run;
                        }
                    }
                }
                break;

            case State.Jump:
                {
                    if(grounded) {
                        if(Mathf.Abs(input.x) > 0.4f) {
                            this.state = State.Run;
                        } else {
                            this.state = State.Idle;
                        }
                    } else if(wallSliding && velocity.y < 0) {
                        this.state = State.WallSlide;
                    }
                }
                break;

            case State.Run:
                {
                    if(jumped || (!grounded && Mathf.Abs(velocity.y) > this.fallSpeedThreshold)) {
                        this.state = State.Jump;
                    } else if (wallSliding && velocity.y < 0) {
                        this.state = State.WallSlide;
                    } else if(Mathf.Abs(input.x) <= 0.4f) {
                        this.state = State.Idle;
                    }
                }
                break;

            case State.WallSlide:
                {
                    if(wallJumped) {
                        this.state = State.WallJump;
                    } else {
                        if(!wallSliding) {
                            this.state = State.Jump; // falling down
                        }
                    }
                }
                break;

            case State.WallJump:
                {
                    wasWallJumping = true;
                    this.state = State.Jump;
                }
                break;
        }
        

        if(dead && this.state != State.Dead) {
            this.state = State.Dead;
        }


        // state logic:

        switch(this.state) {
            case State.Idle:
                {
                    if(!wasIdle) {
                        this.idleAnimName = "Idle";
                    } else {
                        if(this.idleAnimName == "IdleCapeWave") {
                            if(this.frame == 0) {
                                this.idleAnimName = "Idle";
                            }
                        } else {
                            if(this.frame == 0 && Random.value < 0.02f) {
                                this.idleAnimName = "IdleCapeWave";
                            }
                        }
                    }
                }
                break;

            case State.Jump:
                {
                    if(Mathf.Abs(velocity.y) < this.jumpMidPointSpeedThreshold) {
                        this.jumpPhase = JumpPhase.MidPoint;
                    } else if(velocity.y > 0) {
                        this.jumpPhase = JumpPhase.Up;
                    } else if(velocity.y > -this.jumpDownSlowSpeedThreshold) {
                        this.jumpPhase = JumpPhase.DownSlow;
                    } else {
                        this.jumpPhase = JumpPhase.DownFast;
                    }
                }
                break;

            case State.Run:
                {
                    
                }
                break;

            case State.WallSlide:
                {
                    
                }
                break;
        }


        // set active animation according to state:
        
        switch(this.state) {
            case State.Idle:
                SetActive(this.idleAnimName);
                break;
            case State.Run:
                SetActive("Run");
                break;
            case State.WallSlide:
                SetActive("Wallslide2");
                break;
            case State.WallJump:
                SetActive("Walljump");
                break;
            case State.Jump:
                switch(this.jumpPhase) {
                    case JumpPhase.Up:
                        SetActive("JumpUp", true, !wasWallJumping); // don't reset timer in case we're coming from wall jump
                        break;
                    case JumpPhase.MidPoint:
                        SetActive("JumpMidPoint");
                        break;
                    case JumpPhase.DownSlow:
                        SetActive("JumpDownSlow");
                        break;
                    case JumpPhase.DownFast:
                        SetActive("JumpDownFast");
                        break;
                }
                break;
        }

        // misc:
        if(bounced) {
            this.bounceParticles.Play();
        }
    }

    private enum State {
        Idle,
        Run,
        WallSlide,
        WallJump,
        Jump,
        Dead,
    }

    private enum JumpPhase {
        Up,
        MidPoint,
        DownSlow,
        DownFast,
    }
}