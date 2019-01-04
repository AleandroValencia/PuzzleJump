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

public class Node : MonoBehaviour {

    // Up, Down, Left, Right
    public Node[] nodeLink = new Node[(int)NODE_DIRECTION.MAX_DIRECTION] { null, null, null, null };
    public bool activeAtStart = false;
    [SerializeField] int index;

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
        activeAtStart = true;
        gameObject.SetActive(true);
    }

    private void Awake()
    {
        if (gameObject.activeSelf)
        {
            activeAtStart = true;
        }
    }
}
