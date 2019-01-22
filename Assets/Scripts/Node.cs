using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NODE_DIRECTION
{
    UP = 0,
    DOWN,
    LEFT,
    RIGHT,
    MAX_DIRECTION
}

public class Node : MonoBehaviour
{
    // Up, Down, Left, Right
    public Node[] nodeLink = new Node[(int)NODE_DIRECTION.MAX_DIRECTION] { null, null, null, null };
    public bool activeAtStart = false;
    [SerializeField] Sprite[] sprites;
    [SerializeField] float scatterSpeed = 0.2f;
    [SerializeField] float scatterAcceleration = 1.0f;
    Vector3 startPos;
    SpriteRenderer renderer;
    Animator animator;
    bool scattering = false;
    bool moving = false;
    int spriteIndex = 0;

    public bool StartedActive() { return activeAtStart; }
    public bool Moving { get { return moving; } }
    public Vector3 StartPosition { get { return startPos; } }

    private void Awake()
    {
        if (gameObject.activeSelf)
        {
            activeAtStart = true;
        }
        renderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        spriteIndex = Random.Range(0, 3);
        renderer.sprite = sprites[spriteIndex];
        startPos = transform.position;
    }

    public void SetLink(NODE_DIRECTION _dir, Node _nodeLink)
    {
        nodeLink[(int)_dir] = _nodeLink;
    }

    public Node GetLink(NODE_DIRECTION _dir)
    {
        return nodeLink[(int)_dir];
    }

    public void StopScattering()
    {
        scattering = false;
    }

    public void ResetPosition()
    {
        transform.position = startPos;
        transform.up = new Vector3(0.0f, 1.0f);
    }

    public IEnumerator FlyToPosition()
    {
        moving = true;
        animator.SetBool("Flying", true);
        while(transform.position != startPos)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPos, 0.2f);
            yield return null;
        }
        animator.SetBool("Flying", false);
        transform.up = new Vector3(0.0f, 1.0f);
        moving = true;
    }

    private bool RandomBool()
    {
        if (Random.Range(0, 2) == 0)
        {
            return true;
        }
        return false;
    }

    public void Squish()
    {
        GetComponent<Animator>().Play("Squish");
    }

    /// <summary>
    /// Node is NOT active for this level
    /// </summary>
    public void Deactivate()
    {
        activeAtStart = false;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Assign random sprite and set active for this level
    /// </summary>
    public void Activate()
    {
        spriteIndex = Random.Range(0, sprites.Length);
        renderer.sprite = sprites[spriteIndex];
        activeAtStart = true;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Bug flees when player leaves the bug
    /// </summary>
    /// <param name="_dir"></param>
    /// <returns></returns>
    public IEnumerator Scatter(NODE_DIRECTION _dir)
    {
        float acceleration = Random.Range(-scatterAcceleration, scatterAcceleration);
        Vector3 direction = Vector3.zero;
        switch (_dir)
        {
            case NODE_DIRECTION.UP:
                direction.y += scatterSpeed;
                break;
            case NODE_DIRECTION.DOWN:
                direction.y -= scatterSpeed;
                break;
            case NODE_DIRECTION.LEFT:
                direction.x -= scatterSpeed;
                break;
            case NODE_DIRECTION.RIGHT:
                direction.x += scatterSpeed;
                break;
            case NODE_DIRECTION.MAX_DIRECTION:
                break;
            default:
                break;
        }
        scattering = true;
        animator.SetBool("Flying", true);

        while (scattering && Mathf.Abs(transform.position.y) < Camera.main.orthographicSize && Mathf.Abs(transform.position.x) < Camera.main.orthographicSize * (10.0f / 16.0f))
        {
            transform.up = direction;
            transform.position += direction;
            if (RandomBool())
                direction.x += acceleration * Time.deltaTime;
            if (RandomBool())
                direction.y += acceleration * Time.deltaTime;
            yield return null;
        }

        //transform.position = startPos;
        if (scattering)
            gameObject.SetActive(false);
        animator.SetBool("Flying", false);
        renderer.sprite = sprites[spriteIndex];
    }
}
