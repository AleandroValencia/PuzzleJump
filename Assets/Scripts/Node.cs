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
    Vector3 startPos;
    SpriteRenderer renderer;

    public bool StartedActive() { return activeAtStart; }
    public void SetLink(NODE_DIRECTION _dir, Node _nodeLink)
    {
        nodeLink[(int)_dir] = _nodeLink;
    }

    public Node GetLink(NODE_DIRECTION _dir)
    {
        return nodeLink[(int)_dir];
    }

    public void Deactivate()
    {
        activeAtStart = false;
        gameObject.SetActive(false);
    }

    public void Activate()
    {
        int rand = Random.Range(0, 3);
        renderer.sprite = sprites[rand];
        activeAtStart = true;
        gameObject.SetActive(true);
    }

    public IEnumerator Scatter(NODE_DIRECTION _dir)
    {
        Vector3 direction = Vector3.zero;
        switch (_dir)
        {
            case NODE_DIRECTION.UP:
                direction.y+= scatterSpeed;
                break;
            case NODE_DIRECTION.DOWN:
                direction.y-= scatterSpeed;
                break;
            case NODE_DIRECTION.LEFT:
                direction.x-= scatterSpeed;
                break;
            case NODE_DIRECTION.RIGHT:
                direction.x+= scatterSpeed;
                break;
            case NODE_DIRECTION.MAX_DIRECTION:
                break;
            default:
                break;
        }
        
        while (Mathf.Abs(transform.position.y) < Camera.main.orthographicSize && Mathf.Abs(transform.position.x) < Camera.main.orthographicSize * (10.0f/16.0f))
        {
            transform.position += direction;
            yield return null;
        }

        transform.position = startPos;
        gameObject.SetActive(false);
    }

    //public void Scatter(NODE_DIRECTION _dir)
    //{

    //}

    private void Awake()
    {
        if (gameObject.activeSelf)
        {
            activeAtStart = true;
        }
        renderer = GetComponent<SpriteRenderer>();
        int rand = Random.Range(0, 3);
        renderer.sprite = sprites[rand];
        startPos = transform.position;
    }

    public void Squish()
    {
        GetComponent<Animator>().Play("Squish");
    }
}
