using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : Scene
{
    PlayerController player;
    SoundManager sfx;
    [Range(0.0f, 100.0f)] [SerializeField] int randomness = 33;
    int numStartingStones = 0;

    // Pre-generated levels
    int[,] levels = new int[,] {    {2,0,0,0,0,1,1,1,1,0,1,0,0,1,0,1,1,1,1,0,0,1,0,0,0,0,0,0,0,0 },
                                    {0,1,1,0,0,1,1,0,2,0,1,1,0,0,0,1,1,0,0,0,1,0,1,0,0,0,0,0,0,0 },
                                    {0,2,0,0,0,0,1,1,1,0,1,0,0,1,0,1,0,0,1,0,0,1,1,1,0,0,1,0,0,0 },
                                    {0,0,0,2,0,0,1,1,1,0,0,1,1,1,0,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0 },
                                    {0,1,1,0,0,1,0,0,1,0,1,0,0,1,0,1,1,1,1,2,1,0,0,0,0,0,0,0,0,0 },
                                    {0,1,1,1,0,0,1,1,1,0,1,1,0,1,2,0,1,1,1,0,0,0,0,0,0,0,0,0,0,0 },
                                    {2,0,0,0,0,1,1,1,1,0,1,0,0,1,0,1,0,0,1,0,1,1,1,1,1,0,0,0,0,0 },
                                    {0,1,1,1,0,0,1,0,1,0,2,1,0,1,1,0,1,0,1,0,0,1,1,1,0,0,0,0,0,0 },
                                    {2,0,0,0,0,1,1,0,0,0,1,1,1,0,0,1,1,1,0,0,1,1,0,0,0,1,0,0,0,0 } };


    void Start()
    {
        //SelectLevel(levelIndex);
        player = GetComponent<PlayerController>();
        sfx = GetComponent<SoundManager>();
        GenerateRandomLevel();
        SetupNodes();
        StartCoroutine(player.JumpToPosition(startNode.StartPosition));
    }

    public override void Jump(NODE_DIRECTION _dir)
    {
        AdjustLink(currentNode);
        Node previousNode = currentNode;
        currentNode = currentNode.GetLink(_dir);
        StartCoroutine(previousNode.GetComponent<Node>().Scatter(OppositeDirection(_dir)));
        lastDirection = _dir;
        DecrementRemainingStones();
    }

    public override IEnumerator LevelComplete()
    {
        sfx.PlaySound(SoundManager.SOUNDS.VICTORY);
        GenerateRandomLevel();
        RestartLevel();
        yield return null;
    }

    /// <summary>
    /// Load level _index in the array of pre-generated levels
    /// </summary>
    /// <param name="_index"></param>
    void SelectLevel(int _index)
    {
        for (int i = 0; i < 30; ++i)
        {
            if (levels[_index, i] == 0)
            {
                nodes[i].Deactivate();
            }
            else if (levels[_index, i] == 1)
            {
                nodes[i].Activate();
            }
            else if (levels[_index, i] == 2)
            {
                nodes[i].Activate();
                startNode = nodes[i];
                currentNode = startNode;
            }
        }
    }

    /// <summary>
    /// Move player to the node in the specified _dir
    /// </summary>
    /// <param name="_dir"></param>
    /// <returns></returns>
    bool AIJump(NODE_DIRECTION _dir)
    {
        if (currentNode.GetLink(_dir) != null && currentNode.GetLink(_dir).gameObject.activeSelf)
        {
            AdjustLink(currentNode);
            Node previousNode = currentNode;
            currentNode = currentNode.GetLink(_dir);
            previousNode.gameObject.SetActive(false);
            lastDirection = _dir;
            remainingStones--;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Set links between nodes and keep count of the number of active stones
    /// </summary>
    void SetupNodes()
    {
        numStartingStones = 0;
        remainingStones = 0;
        for (int i = 0; i < nodes.Length; ++i)
        {
            if (nodes[i].StartedActive())
            {
                // set vertical links
                for (int j = i - gridWidth; j >= 0; j -= gridWidth)
                {
                    if (nodes[j].gameObject.activeSelf)
                    {
                        nodes[i].SetLink(NODE_DIRECTION.UP, nodes[j]);
                        nodes[j].SetLink(NODE_DIRECTION.DOWN, nodes[i]);
                        break;
                    }
                }
                // Set horizontal links
                for (int j = i - 1; j >= (i / gridWidth) * gridWidth; --j)
                {
                    if (nodes[j].gameObject.activeSelf)
                    {
                        nodes[i].SetLink(NODE_DIRECTION.LEFT, nodes[j]);
                        nodes[j].SetLink(NODE_DIRECTION.RIGHT, nodes[i]);
                        break;
                    }
                }
                remainingStones++;
                numStartingStones++;
            }
        }
    }

    /// <summary>
    /// Link the nodes adjacent to _node to each other before deactivating _node
    /// </summary>
    /// <param name="_node"></param>
    public void AdjustLink(Node _node)
    {
        // Link the nodes to the up with the down of current node  (and left and right)
        for (int i = 0; i < 4; i += 2)
        {
            if (_node.GetLink((NODE_DIRECTION)i) != null)
                _node.GetLink((NODE_DIRECTION)i).SetLink((NODE_DIRECTION)i + 1, _node.GetLink((NODE_DIRECTION)i + 1));
            if (_node.GetLink((NODE_DIRECTION)i + 1) != null)
                _node.GetLink((NODE_DIRECTION)i + 1).SetLink((NODE_DIRECTION)i, _node.GetLink((NODE_DIRECTION)i));
        }
    }

    /// <summary>
    /// Activate all nodes, deactivate random nodes, create a path from the active nodes
    /// and store it in a list. This list becomes the new level
    /// </summary>
    public void GenerateRandomLevel()
    {
        //startNode = currentNode;
        startNode = nodes[Random.Range(0, nodes.Length)];
        foreach (Node node in nodes)
        {
            node.Activate();
        }
        SetupNodes();
        foreach (Node node in nodes)
        {
            // Turn off random stones
            if (Random.Range(0, 100) < randomness && node != startNode)
            {
                AdjustLink(node);
                node.Deactivate();
            }
        }

        List<Node> newLevel = new List<Node>();
        currentNode = startNode;
        newLevel.Add(currentNode);
        // Check that the current node has at least one neighbour
        bool neighboursExist = currentNode.GetLink(NODE_DIRECTION.UP) != null ||
                                currentNode.GetLink(NODE_DIRECTION.DOWN) != null ||
                                currentNode.GetLink(NODE_DIRECTION.LEFT) != null ||
                                currentNode.GetLink(NODE_DIRECTION.RIGHT) != null;

        while (neighboursExist)
        {
            NODE_DIRECTION randDir = (NODE_DIRECTION)Random.Range((int)NODE_DIRECTION.UP, (int)NODE_DIRECTION.MAX_DIRECTION);
            // Check that random direction is not backwards
            if (randDir != OppositeDirection(lastDirection))
            {
                if (AIJump(randDir))
                {
                    newLevel.Add(currentNode);
                }
            }

            // Check that current node has at least one neighbour that is NOT backwards from the last direction
            neighboursExist = (currentNode.GetLink(NODE_DIRECTION.UP) != null && lastDirection != OppositeDirection(NODE_DIRECTION.UP)) ||
                                (currentNode.GetLink(NODE_DIRECTION.DOWN) != null && lastDirection != OppositeDirection(NODE_DIRECTION.DOWN)) ||
                                (currentNode.GetLink(NODE_DIRECTION.LEFT) != null && lastDirection != OppositeDirection(NODE_DIRECTION.LEFT)) ||
                                (currentNode.GetLink(NODE_DIRECTION.RIGHT) != null && lastDirection != OppositeDirection(NODE_DIRECTION.RIGHT));
        }

        // Turn off all nodes that aren't in the new level and weren't randomly turned off
        foreach (Node node in nodes)
        {
            node.Deactivate();
        }
        // Turn on all nodes in the new level
        foreach (Node node in newLevel)
        {
            node.Activate();
        }

        currentNode = startNode;

        // Check that there's at least six bugs in the level
        if (newLevel.Count < 6)
        {
            GenerateRandomLevel();
        }
        //player.ResetPlayerPosition(startNode.transform.position);
    }

    /// <summary>
    /// Activate all nodes in level and reset their links to beginning setting
    /// </summary>
    public override void RestartLevel()
    {
        foreach (Node node in nodes)
        {
            node.StopScattering();
            if (node.activeAtStart)
            {
                node.gameObject.SetActive(true);
                //node.ResetPosition();
                StartCoroutine(node.FlyToPosition());
            }
        }
        SetupNodes();   // reset links
        //transform.SetPositionAndRotation(startNode.transform.position, transform.rotation);
        StartCoroutine(player.JumpToPosition(startNode.StartPosition));
        currentNode = startNode;
        lastDirection = NODE_DIRECTION.MAX_DIRECTION;
    }
}
