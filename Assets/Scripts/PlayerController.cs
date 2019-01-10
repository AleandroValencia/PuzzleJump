using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    enum SOUNDS
    {
        JUMP = 0,
        VICTORY,
        RESTART,
        NUM_SOUNDS
    }

    int gridWidth = 5;
    int gridHeight = 6;
    [SerializeField] Node[] nodes;
    Node currentNode;
    Node startNode;
    NODE_DIRECTION lastDirection = NODE_DIRECTION.MAX_DIRECTION;
    int numStartingStones = 0;
    int remainingStones = 0;
    int levelIndex = 0;

    [SerializeField] float flickAmount = 1.0f;
    [SerializeField] float touchHoldTimer = 1.5f;
    [SerializeField] float doubleTapSensitivity = 0.7f;
    bool touchHold = false;
    float touchTimer = 0.0f;
    float doubleTapTimer = 1.0f;

    [Range(0.0f, 100.0f)] [SerializeField] int randomness = 33;
    [SerializeField] AudioClip[] sounds;
    [SerializeField] Sprite[] sprites;

    private Vector2 touchOrigin = -Vector2.one; //Used to store location of screen touch origin for mobile controls.

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

    void PlaySound(SOUNDS _sound)
    {
        GetComponent<AudioSource>().clip = sounds[(int)_sound];
        GetComponent<AudioSource>().pitch = 1.0f;
        GetComponent<AudioSource>().Play();
    }

    void PlaySoundRandomPitch(SOUNDS _sound)
    {
        GetComponent<AudioSource>().clip = sounds[(int)_sound];
        GetComponent<AudioSource>().pitch = 1.0f + Random.Range(-0.08f, 0.1f);
        GetComponent<AudioSource>().Play();
    }

    /// <summary>
    /// Set links between nodes and keep count of the number of active stones
    /// </summary>
    void SetupNodes()
    {
        numStartingStones = 0;
        remainingStones = 0;
        // TODO: OPTIMIZE
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
                numStartingStones++;
            }
        }
    }

    /// <summary>
    /// Activate all nodes in level and reset their linkes to beginning setting
    /// </summary>
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

    /// <summary>
    /// Link the nodes adjacent to _node to each other before deactivating _node
    /// </summary>
    /// <param name="_node"></param>
    void AdjustLink(Node _node)
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
    /// Move player to the node in the specified _dir
    /// </summary>
    /// <param name="_dir"></param>
    /// <returns></returns>
    bool Jump(NODE_DIRECTION _dir)
    {
        if (currentNode.GetLink(_dir) != null && currentNode.GetLink(_dir).gameObject.activeSelf)
        {
            AdjustLink(currentNode);
            Node previousNode = currentNode;
            currentNode = currentNode.GetLink(_dir);
            transform.SetPositionAndRotation(currentNode.transform.position, transform.rotation);
            previousNode.gameObject.SetActive(false);
            lastDirection = _dir;
            remainingStones--;
            PlaySoundRandomPitch(SOUNDS.JUMP);
            GetComponent<SpriteRenderer>().sprite = sprites[(int)_dir];
            return true;
        }
        return false;
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
    /// Get the opposite direction of passed in _dir
    /// </summary>
    /// <param name="_dir"></param>
    /// <returns></returns>
    NODE_DIRECTION OppositeDirection(NODE_DIRECTION _dir)
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

    /// <summary>
    /// Activate all nodes, deactivate random nodes, create a path from the active nodes
    /// and store it in a list. This list becomes the new path
    /// </summary>
    void GenerateRandomLevel()
    {
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
        bool neighboursExist = currentNode.GetLink(NODE_DIRECTION.UP) != null ||
                                currentNode.GetLink(NODE_DIRECTION.DOWN) != null ||
                                currentNode.GetLink(NODE_DIRECTION.LEFT) != null ||
                                currentNode.GetLink(NODE_DIRECTION.RIGHT) != null;

        while (neighboursExist)
        {
            NODE_DIRECTION randDir = (NODE_DIRECTION)Random.Range((int)NODE_DIRECTION.UP, (int)NODE_DIRECTION.MAX_DIRECTION);
            if (randDir != OppositeDirection(lastDirection))
            {
                if (Jump(randDir))
                {
                    newLevel.Add(currentNode);
                }
            }

            neighboursExist = (currentNode.GetLink(NODE_DIRECTION.UP) != null && lastDirection != OppositeDirection(NODE_DIRECTION.UP)) ||
                                (currentNode.GetLink(NODE_DIRECTION.DOWN) != null && lastDirection != OppositeDirection(NODE_DIRECTION.DOWN)) ||
                                (currentNode.GetLink(NODE_DIRECTION.LEFT) != null && lastDirection != OppositeDirection(NODE_DIRECTION.LEFT)) ||
                                (currentNode.GetLink(NODE_DIRECTION.RIGHT) != null && lastDirection != OppositeDirection(NODE_DIRECTION.RIGHT));
        }

        foreach (Node node in nodes)
        {
            node.Deactivate();
        }
        foreach (Node node in newLevel)
        {
            node.Activate();
        }

        currentNode = startNode;
    }

    /// <summary>
    /// Arrow keys to move, space to restart level
    /// </summary>
    void KeyboardInput()
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
        if (Input.GetKeyDown(KeyCode.Return))
        {
            RestartLevel();
            PlaySound(SOUNDS.RESTART);
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
                GetComponent<SpriteRenderer>().color = Color.blue;
                touchHold = true;
                touchTimer = 0.0f;
            }
            else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
            {
                Vector2 touchEnd = myTouch.position;
                float x = touchEnd.x - touchOrigin.x;
                float y = touchEnd.y - touchOrigin.y;
                touchOrigin.x = -1;
                GetComponent<SpriteRenderer>().color = Color.red;
                touchHold = false;
                bool jumped = false;

                //Check if the difference along the x axis is greater than the difference along the y axis.
                if (Mathf.Abs(x) > Mathf.Abs(y))
                {
                    if (x > flickAmount && lastDirection != NODE_DIRECTION.LEFT)
                    {
                        GetComponent<SpriteRenderer>().color = Color.black;
                        Jump(NODE_DIRECTION.RIGHT);
                        jumped = true;
                    }
                    else if (x < -flickAmount && lastDirection != NODE_DIRECTION.RIGHT)
                    {
                        GetComponent<SpriteRenderer>().color = Color.green;
                        Jump(NODE_DIRECTION.LEFT);
                        jumped = true;
                    }
                }
                else
                {
                    if (y > flickAmount && lastDirection != NODE_DIRECTION.DOWN)
                    {
                        GetComponent<SpriteRenderer>().color = Color.yellow;
                        Jump(NODE_DIRECTION.UP);
                        jumped = true;
                    }
                    else if (y < -flickAmount && lastDirection != NODE_DIRECTION.UP)
                    {
                        GetComponent<SpriteRenderer>().color = Color.grey;
                        Jump(NODE_DIRECTION.DOWN);
                        jumped = true;
                    }
                }

                if (doubleTapTimer < doubleTapSensitivity && !jumped && currentNode != startNode)
                {
                    RestartLevel();
                    PlaySound(SOUNDS.RESTART);
                }
                doubleTapTimer = 0.0f;

            }
        }

        if (touchHold)
        {
            touchTimer += Time.deltaTime;
        }
        else
        {
            doubleTapTimer += Time.deltaTime;
        }

        if (touchTimer > touchHoldTimer)
        {
            //RestartLevel();
            touchTimer = 0.0f;
        }
    }

    // Use this for initialization
    void Start()
    {
        //SelectLevel(levelIndex);
        GenerateRandomLevel();
        SetupNodes();
        transform.SetPositionAndRotation(startNode.transform.position, transform.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        KeyboardInput();
        TouchInput();

        // Level Complete
        if (remainingStones == 1)
        {
            levelIndex++;
            //SelectLevel(levelIndex);
            GenerateRandomLevel();
            RestartLevel();
            PlaySound(SOUNDS.VICTORY);
        }
    }
}
