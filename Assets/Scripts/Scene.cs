using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Scene : MonoBehaviour
{
    [SerializeField] protected Node[] nodes;
    [SerializeField] protected int gridWidth = 5;
    [SerializeField] protected int gridHeight = 6;
    [SerializeField] protected Node startNode;
    protected int remainingStones = 0;
    protected Node currentNode;
    protected NODE_DIRECTION lastDirection = NODE_DIRECTION.MAX_DIRECTION;

    public Node StartNode { get { return startNode; } }
    public int RemainingStones { get { return remainingStones; } }
    public Node CurrentNode { get { return currentNode; } set { currentNode = value; } }
    public NODE_DIRECTION LastDirection { get { return lastDirection; } set { lastDirection = value; } }

    public Vector3 StartNodePosition()
    {
        if (startNode != null)
            return startNode.transform.position;
        return Vector3.zero;
    }
    public void DecrementRemainingStones() { remainingStones--; }

    private void Awake()
    {
        currentNode = startNode;
    }

    public abstract void Jump(NODE_DIRECTION _dir);
    public abstract void RestartLevel();
    public abstract void LevelComplete();

    /// <summary>
    /// Get the opposite direction of passed in _dir
    /// </summary>
    /// <param name="_dir"></param>
    /// <returns></returns>
    public NODE_DIRECTION OppositeDirection(NODE_DIRECTION _dir)
    {
        switch (_dir)
        {
            case NODE_DIRECTION.UP:
                return NODE_DIRECTION.DOWN;
            case NODE_DIRECTION.DOWN:
                return NODE_DIRECTION.UP;
            case NODE_DIRECTION.LEFT:
                return NODE_DIRECTION.RIGHT;
            case NODE_DIRECTION.RIGHT:
                return NODE_DIRECTION.LEFT;
            default:
                break;
        }
        return NODE_DIRECTION.MAX_DIRECTION;
    }
}
