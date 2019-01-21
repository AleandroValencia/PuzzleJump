using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationScript : MonoBehaviour {

    [SerializeField] float idleAlarm = 3.0f;

    Animator animator;
    float idleTimer = 0.0f;

    // Use this for initialization
    void Start () {
        animator = GetComponent<Animator>();
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
