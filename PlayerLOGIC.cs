using System.Diagnostics;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    #region NON Configurable Values
    private Rigidbody2D rigidBody;
    private CircleCollider2D circleCollicer;
    private string state = "Grounded";
    private string debugState = "Grounded";
    Stopwatch timerMain;
    #endregion

    #region Configuarable Values
    [Header("Layer Detection")]
    [SerializeField] private LayerMask layer_SOLID;

    [Header("Speeds")]
    [SerializeField] private float speed_AIR;
    [SerializeField] private float speed_GROUND;
    [SerializeField] private float speed_BOUNCE;

    [Header("Jump")]
    [SerializeField] private float jump_Force;
    [SerializeField] private int jump_Force_MIN;
    [SerializeField] private int jump_Force_MAX;
    
    [Header("Bounce")]
    [SerializeField] private int bounce_Force;
    [SerializeField] private int bounce_Input_Window;
    
    [Header("Debug")]
    [SerializeField] private bool debug;
    #endregion
    
    #region Start & Update Loops
    private void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        circleCollicer = GetComponent<CircleCollider2D>();
        timerMain = new Stopwatch();
    }
    private void Update()
    {
        stateMachine();

        debuger();
    }
    #endregion
    
    #region Functions
    private void debuger()
    {   
        if (Input.GetKey(KeyCode.Escape))
        {
            debug = true;
        }
        if(debug)
        {
            if (debugState != state)
            {
                UnityEngine.Debug.Log("Player is in state: " + state);
            }
            debugState = state;
        }
    }
    private void xMovement(float speed)
    {
        rigidBody.linearVelocityX = Input.GetAxis("Horizontal") * speed;
    }
    private bool mainKEY()
    {
        return Input.GetKey(KeyCode.Space);
    }
    private bool checkColliderOffset(Vector2 Direction, float Length, LayerMask Layer)
    {
        RaycastHit2D rayCastHit = Physics2D.CircleCast(circleCollicer.bounds.center, circleCollicer.radius, Direction, Length, Layer);
        return rayCastHit.collider != null;
    }
    private bool grounded()
    {
        if (checkColliderOffset(Vector2.down, 0.1f, layer_SOLID)) { return true; }
        return false;
    }
    private void airborneState()
    {
        if (rigidBody.linearVelocityY <= 0)
        {
            state = "Fall";
        }
        else
        {
            state = "Lift";
        }
    }
    private float clockRead(bool endClock = true, Stopwatch stopwatch = null)
    {
        if (stopwatch == null)
        {
            stopwatch = timerMain;
        }

        var temp_stopwatch = stopwatch.Elapsed.Milliseconds / 10;
        
        if (endClock)
        {
            stopwatch.Stop();
            stopwatch.Reset();
        }

        if (debug)
        {
            UnityEngine.Debug.Log("Miliseconds / 10 elapsed: " + temp_stopwatch);
        }

        return temp_stopwatch;
    }
    #endregion
    
    private void stateMachine()
    {
        switch (state)
        {
            default:    
                
                UnityEngine.Debug.Log("ERROR: -Unknown State-");    
                airborneState();

            break;

            case "Grounded":

                xMovement(speed_GROUND);

                if (!grounded())
                {
                    airborneState();
                }
                else if (mainKEY())
                {
                    state = "Charge Jump";
                }

            break;
            
            case "Charge Jump":
                
                xMovement(0);

                timerMain.Start();;

                if (!mainKEY())
                {
                    var holdTimer = clockRead();

                    if (holdTimer <= bounce_Input_Window)
                    {
                        state = "Bounce";
                        break;
                    }

                    rigidBody.linearVelocityY = Mathf.Clamp(jump_Force * holdTimer, jump_Force_MIN, jump_Force_MAX);

                    state = "Lift";
                }
            break;
            
            case "Lift":
                
                xMovement(speed_AIR);

                airborneState();

            break;
            
            case "Fall":
                
                xMovement(speed_AIR);

                if (grounded())
                {
                    state = "Land";
                }

            break;

            case "Land":

                state = "Grounded";

            break;
            
            
            case "Bounce":
                
                xMovement(speed_BOUNCE);
                
                if (grounded())
                {
                    rigidBody.linearVelocityY = bounce_Force;
                }
                
                if (!mainKEY())
                {
                    timerMain.Start();
                    
                    if (clockRead(false) > bounce_Input_Window)
                    {
                        airborneState();
                        timerMain.Stop();
                        timerMain.Reset();    
                    }
                }
                else    
                {   
                    timerMain.Stop();
                    timerMain.Reset();   
                }
            break;

            case "":

                //

            break;
            
            /*
            case "Grounded":

                xMovement(speed_GROUND);

                if (grounded() && mainKEY()) 
                { 
                    state = "Charge Jump"; 
                }

            break;
            case "Charge Jump":

                xMovement(0);
                timer += 1;
                if(mainKEY())
                {
                    rigidBody.linearVelocityY = Mathf.Clamp(jump_Force * timer, jump_Force_MIN, jump_Force_MAX);
                    state = "Bounce";
                    timer = 0;
                } 

            break;
            case "Bounce":

                xMovement(speed_AIR);

                if (grounded())
                {
                    if (mainKEY())
                    {
                        rigidBody.linearVelocityY = bounceForce;
                    }
                    else
                    {
                        state = "Ground";
                    }

                }

                break;
            */
            
        }
    }

}