using UnityEngine.SceneManagement;

public class MainMenuManager : Scene
{
    public SlideAnimation[] menuItems;
    AnimationScript animator;
    SoundManager sfx;

    private void Start()
    {
        animator = GetComponent<AnimationScript>();
        sfx = GetComponent<SoundManager>();
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

    public override System.Collections.IEnumerator LevelComplete()
    {
        animator.Shoryuken();
        StartCoroutine(animator.JumpOffscreen());
        foreach (SlideAnimation item in menuItems)
        {
            item.MovingTowards = false;
            if (item.tag == "arrow")
                StartCoroutine(item.MoveTo(new UnityEngine.Vector3(0.0f, -8.0f)));
            else
                StartCoroutine(item.MoveBackwards());
        }
        sfx.PlaySound(SoundManager.SOUNDS.VICTORY);
        StartCoroutine(currentNode.Scatter(NODE_DIRECTION.DOWN));
        DecrementRemainingStones();
        while (animator.OnScreen)
        {
            yield return null;
        }
        SceneManager.LoadScene("Zen Mode");
    }

    public override void RestartLevel()
    {
        throw new System.NotImplementedException();
    }

}
