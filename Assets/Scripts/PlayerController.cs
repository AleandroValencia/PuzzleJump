using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField] int gridWidth = 5;
    [SerializeField] int gridHeight = 6;
    [SerializeField] Node[] nodes;
    [SerializeField] Node currentNode;
    Node startNode;
    NODE_DIRECTION lastDirection = NODE_DIRECTION.MAX_DIRECTION;
    public int startingStones = 0;
    public int remainingStones = 0;

    private Vector2 touchOrigin = -Vector2.one; //Used to store location of screen touch origin for mobile controls.


    void SetupNodes()
    {
        startingStones = 0;
        remainingStones = 0;
        for (int i = 0; i < nodes.Length; ++i)
        {
            if (nodes[i].StartedActive())
            {
                // set up link
                for (int j = i - gridWidth; j >= 0; j -= gridWidth)
                {
                    if (nodes[j].gameObject.activeSelf)
                    {
                        nodes[i].SetLink(NODE_DIRECTION.UP, nodes[j]);
                        break;
                    }
                }
                // set down link
                for (int j = i + gridWidth; j < gridHeight * gridWidth; j += gridWidth)
                {
                    if (nodes[j].gameObject.activeSelf)
                    {
                        nodes[i].SetLink(NODE_DIRECTION.DOWN, nodes[j]);
                        break;
                    }
                }
                // Set left link
                for (int j = i - 1; j >= (i / gridWidth) * gridWidth; --j)
                {
                    if (nodes[j].gameObject.activeSelf)
                    {
                        nodes[i].SetLink(NODE_DIRECTION.LEFT, nodes[j]);
                        break;
                    }
                }
                // set right link
                for (int j = i + 1; j < (i / gridWidth + 1) * gridWidth; ++j)
                {
                    if (nodes[j].gameObject.activeSelf)
                    {
                        nodes[i].SetLink(NODE_DIRECTION.RIGHT, nodes[j]);
                        break;
                    }
                }
                remainingStones++;
                startingStones++;
            }
        }
    }

    void RestartLevel()
    {
        foreach (Node node in nodes)
        {
            if (node.activeAtStart)
            {
                node.gameObject.SetActive(true);
            }
        }
        SetupNodes();   // reset links
        transform.SetPositionAndRotation(startNode.transform.position, transform.rotation);
        currentNode = startNode;
        lastDirection = NODE_DIRECTION.MAX_DIRECTION;
    }

    void AdjustLink()
    {
        // Link the nodes to the up with the down of current node  (and left and right)
        for (int i = 0; i < 4; i += 2)
        {
            if (currentNode.GetLink((NODE_DIRECTION)i) != null)
                currentNode.GetLink((NODE_DIRECTION)i).SetLink((NODE_DIRECTION)i + 1, currentNode.GetLink((NODE_DIRECTION)i + 1));
            if (currentNode.GetLink((NODE_DIRECTION)i + 1) != null)
                currentNode.GetLink((NODE_DIRECTION)i + 1).SetLink((NODE_DIRECTION)i, currentNode.GetLink((NODE_DIRECTION)i));
        }
    }

    void Jump(NODE_DIRECTION _dir)
    {
        if (currentNode.GetLink(_dir) != null && currentNode.GetLink(_dir).gameObject.activeSelf)
        {
            AdjustLink();
            Node previousNode = currentNode;
            currentNode = currentNode.GetLink(_dir);
            transform.SetPositionAndRotation(currentNode.transform.position, transform.rotation);
            previousNode.gameObject.SetActive(false);
            lastDirection = _dir;
            remainingStones--;
        }
    }

    // Use this for initialization
    void Start()
    {
        SetupNodes();
        startNode = currentNode;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) && lastDirection != NODE_DIRECTION.DOWN)
        {
            Jump(NODE_DIRECTION.UP);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) && lastDirection != NODE_DIRECTION.UP)
        {
            Jump(NODE_DIRECTION.DOWN);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) && lastDirection != NODE_DIRECTION.RIGHT)
        {
            Jump(NODE_DIRECTION.LEFT);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) && lastDirection != NODE_DIRECTION.LEFT)
        {
            Jump(NODE_DIRECTION.RIGHT);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            RestartLevel();
        }

        if (Input.touchCount > 0)
        {
            Touch myTouch = Input.touches[0];

            if (myTouch.phase == TouchPhase.Began)
            {
                touchOrigin = myTouch.position;
            }
            else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
            {
                Vector2 touchEnd = myTouch.position;
                float x = touchEnd.x - touchOrigin.x;
                float y = touchEnd.y - touchOrigin.y;
                touchOrigin.x = -1;

                //Check if the difference along the x axis is greater than the difference along the y axis.
                if (Mathf.Abs(x) > Mathf.Abs(y))
                {
                    if (x > 0 && lastDirection != NODE_DIRECTION.LEFT)
                    {
                        Jump(NODE_DIRECTION.RIGHT);
                    }
                    else if (x < 0 && lastDirection != NODE_DIRECTION.RIGHT)
                    {
                        Jump(NODE_DIRECTION.LEFT);
                    }
                }
                else
                {
                    if (y > 0 && lastDirection != NODE_DIRECTION.DOWN)
                    {
                        Jump(NODE_DIRECTION.RIGHT);
                    }
                    else if (y < 0 && lastDirection != NODE_DIRECTION.UP)
                    {
                        Jump(NODE_DIRECTION.LEFT);
                    }
                }
            }
        }

        if (remainingStones == 1)
        {
            print("you win");
        }
    }
}
