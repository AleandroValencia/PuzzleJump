using UnityEngine.SceneManagement;

public class MainMenuManager : Scene
{
    private void Start()
    {
        remainingStones = nodes.Length;
    }

    public override void Jump(NODE_DIRECTION _dir)
    {
        Node previousNode = currentNode;
        currentNode = currentNode.GetLink(_dir);
        StartCoroutine(previousNode.GetComponent<Node>().Scatter(OppositeDirection(_dir)));
        lastDirection = _dir;
        DecrementRemainingStones();
    }

    public override void LevelComplete()
    {
        SceneManager.LoadScene("Zen Mode");
    }

    public override void RestartLevel()
    {
        throw new System.NotImplementedException();
    }

}
