using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float jumpSpeed = 1.0f;
    [SerializeField] float flickAmount = 1.0f;
    [SerializeField] float touchHoldTimer = 1.5f;
    [SerializeField] float doubleTapSensitivity = 0.7f;

    Scene levelManager;
    SoundManager sfx;
    AnimationScript animationController;
    bool resetting = false;
    bool jumping = false;
    bool touchHold = false;
    float touchTimer = 0.0f;
    float doubleTapTimer = 1.0f;
    float distanceFromNextRock = 0.0f;
    float scaleAmount = 0.1f;

    private Vector2 touchOrigin = -Vector2.one; //Used to store location of screen touch origin for mobile controls.

    public void ResetPlayerPosition(Vector3 _pos)
    {
        transform.SetPositionAndRotation(_pos, transform.rotation);
    }

    public IEnumerator JumpToPosition(Vector3 _pos)
    {
        resetting = true;
        jumping = true;
        distanceFromNextRock = Vector2.Distance(transform.position, _pos);
        while (transform.position != _pos)
        {
            transform.position = Vector3.MoveTowards(transform.position, _pos, 0.1f);
            // squash/stretch over jump
            if (Vector2.Distance(transform.position, _pos) > distanceFromNextRock / 2.0f)
            {
                transform.localScale += new Vector3(scaleAmount, scaleAmount);
            }
            else
            {
                if (transform.localScale.x > 1.0f)
                    transform.localScale -= new Vector3(scaleAmount, scaleAmount);
            }
            yield return null;
        }
        jumping = true;
        resetting = false;
    }

    /// <summary>
    /// Move player to the node in the specified _dir
    /// </summary>
    /// <param name="_dir"></param>
    /// <returns></returns>
    bool Jump(NODE_DIRECTION _dir)
    {
        if (levelManager.CurrentNode.GetLink(_dir) != null && levelManager.CurrentNode.GetLink(_dir).gameObject.activeSelf)
        {
            levelManager.Jump(_dir);

            sfx.PlaySoundRandomPitch(SoundManager.SOUNDS.JUMP);
            jumping = true;
            animationController.SetJumpPose(_dir);
            animationController.SetWaiting(false);
            animationController.ResetIdleTimer();

            distanceFromNextRock = Vector2.Distance(transform.position, levelManager.CurrentNode.transform.position);
            return true;
        }
        return false;
    }


    /// <summary>
    /// Arrow keys to move, enter to restart level
    /// </summary>
    void KeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) && levelManager.LastDirection != NODE_DIRECTION.DOWN)
        {
            Jump(NODE_DIRECTION.UP);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) && levelManager.LastDirection != NODE_DIRECTION.UP)
        {
            Jump(NODE_DIRECTION.DOWN);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) && levelManager.LastDirection != NODE_DIRECTION.RIGHT)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            Jump(NODE_DIRECTION.LEFT);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) && levelManager.LastDirection != NODE_DIRECTION.LEFT)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            Jump(NODE_DIRECTION.RIGHT);
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            levelManager.RestartLevel();
            sfx.PlaySound(SoundManager.SOUNDS.RESTART);
        }
    }

    /// <summary>
    /// Increment timers based on input
    /// </summary>
    void TouchTimers()
    {
        if (touchHold)
        {
            // Increment hold timer
            touchTimer += Time.deltaTime;
        }
        else
        {
            // Increment not holding timer
            doubleTapTimer += Time.deltaTime;
        }

        // Touch held for more than hold timer
        if (touchTimer > touchHoldTimer)
        {
            // Do stuff...
            touchTimer = 0.0f;
        }
    }

    /// <summary>
    /// swipe in direction to move, double tap to restart level (also hold available)
    /// </summary>
    void TouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch myTouch = Input.touches[0];

            if (myTouch.phase == TouchPhase.Began)
            {
                touchOrigin = myTouch.position;
                touchHold = true;
                touchTimer = 0.0f;
                animationController.ResetIdleTimer();
            }
            else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
            {
                Vector2 touchEnd = myTouch.position;
                float x = touchEnd.x - touchOrigin.x;
                float y = touchEnd.y - touchOrigin.y;
                touchOrigin.x = -1;
                touchHold = false;
                bool jumped = false;

                //Check if the difference along the x axis is greater than the difference along the y axis.
                if (Mathf.Abs(x) > Mathf.Abs(y))
                {
                    if (x > flickAmount && levelManager.LastDirection != NODE_DIRECTION.LEFT)
                    {
                        GetComponent<SpriteRenderer>().flipX = false;
                        Jump(NODE_DIRECTION.RIGHT);
                        jumped = true;
                    }
                    else if (x < -flickAmount && levelManager.LastDirection != NODE_DIRECTION.RIGHT)
                    {
                        GetComponent<SpriteRenderer>().flipX = true;
                        Jump(NODE_DIRECTION.LEFT);
                        jumped = true;
                    }
                }
                else
                {
                    if (y > flickAmount && levelManager.LastDirection != NODE_DIRECTION.DOWN)
                    {
                        Jump(NODE_DIRECTION.UP);
                        jumped = true;
                    }
                    else if (y < -flickAmount && levelManager.LastDirection != NODE_DIRECTION.UP)
                    {
                        Jump(NODE_DIRECTION.DOWN);
                        jumped = true;
                    }
                }

                // DoubleTap
                if (doubleTapTimer < doubleTapSensitivity && !jumped && levelManager.CurrentNode != levelManager.StartNode)
                {
                    levelManager.RestartLevel();
                    sfx.PlaySound(SoundManager.SOUNDS.RESTART);
                }
                doubleTapTimer = 0.0f;
            }
        }

        TouchTimers();
    }

    // Use this for initialization
    void Start()
    {
        sfx = GetComponent<SoundManager>();
        animationController = GetComponent<AnimationScript>();
        levelManager = GetComponent<Scene>();
        if (levelManager.StartNodePosition() != null)
            transform.SetPositionAndRotation(levelManager.StartNodePosition(), transform.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        if (!jumping)
        {
            // Level Complete
            if (levelManager.RemainingStones == 1)
            {
                StartCoroutine(levelManager.LevelComplete());
            }
            animationController.Jump(false);
            KeyboardInput();
            TouchInput();
        }
        else
        {
            animationController.Jump(true);
            if (!resetting)
            {
                transform.position = Vector3.MoveTowards(transform.position, levelManager.CurrentNode.transform.position, jumpSpeed);
                if (transform.position == levelManager.CurrentNode.transform.position)
                {
                    jumping = false;
                    levelManager.CurrentNode.GetComponent<Node>().Squish();
                }

                // squash/stretch over jump
                if (Vector2.Distance(transform.position, levelManager.CurrentNode.transform.position) > distanceFromNextRock / 2.0f)
                {
                    transform.localScale += new Vector3(scaleAmount, scaleAmount);
                }
                else
                {
                    if (transform.localScale.x > 1.0f)
                        transform.localScale -= new Vector3(scaleAmount, scaleAmount);
                }
            }
        }
    }
}
