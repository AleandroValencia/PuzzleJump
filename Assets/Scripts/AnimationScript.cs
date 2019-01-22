using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationScript : MonoBehaviour {

    [SerializeField] float idleAlarm = 3.0f;
    [SerializeField] float jumpSpeed = 3.0f;
    [SerializeField] float jumpAcceleration = 0.1f;
    [SerializeField] float shoryukenHeight = 2.0f;
    [SerializeField] float shoryukenSpeed = 1.0f;

    Animator animator;
    float idleTimer = 0.0f;
    bool onScreen = true;
    bool shoryuken = false;
    public bool OnScreen { get { return onScreen; } }
    public bool IsShoryuken { get { return shoryuken; } }

    // Use this for initialization
    void Start () {
        animator = GetComponent<Animator>();
	}
	
    public IEnumerator ShoryukenBounce()
    {
        shoryuken = true;
        Shoryuken();
        float yPos = transform.position.y;
        while(transform.position.y < yPos + shoryukenHeight)
        {
            transform.position += new Vector3(0.0f, shoryukenSpeed * Time.deltaTime);
            yield return null;
        }
        while(transform.position.y > yPos)
        {
            transform.position -= new Vector3(0.0f, shoryukenSpeed * Time.deltaTime);
            yield return null;
        }
        StopShoryuken();
        shoryuken = false;
    }

    public IEnumerator JumpOffscreen()
    {
        onScreen = Mathf.Abs(transform.position.y - 1.0f) < Camera.main.orthographicSize && Mathf.Abs(transform.position.x) < Camera.main.orthographicSize * (10.0f / 16.0f);
        while(onScreen)
        {
            transform.position += new Vector3(0.0f, jumpSpeed * Time.deltaTime);
            onScreen = Mathf.Abs(transform.position.y - 1.0f) < Camera.main.orthographicSize && Mathf.Abs(transform.position.x) < Camera.main.orthographicSize * (10.0f / 16.0f);
            jumpSpeed += jumpAcceleration;
            yield return null;
        }
    }

    public void StopShoryuken()
    {
        animator.SetBool("Shoryuken", false);
    }

    public void Shoryuken()
    {
        animator.SetBool("Shoryuken", true);
    }

    public void SetJumpPose(NODE_DIRECTION _dir)
    {
        animator.SetFloat("Idle_Pos", (float)_dir);
    }

    public void SetWaiting(bool _waiting)
    {
        animator.SetBool("Waiting", _waiting);
    }

    public bool IsWaiting()
    {
        return animator.GetBool("Waiting");
    }

    public void Jump(bool _jump)
    {
        animator.SetBool("Jumping", _jump);
    }

    public void ResetIdleTimer()
    {
        idleTimer = 0.0f;
    }

	// Update is called once per frame
	void Update ()
    {
        idleTimer += Time.deltaTime;
        if (idleTimer > idleAlarm)
        {
            // play idle anim
            animator.SetBool("Waiting", !animator.GetBool("Waiting"));
            idleTimer = 0.0f;
        }
    }
}
