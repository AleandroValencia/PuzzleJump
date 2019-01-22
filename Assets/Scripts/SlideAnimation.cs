using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideAnimation : MonoBehaviour {

    [Header("Animation Curve")]
    [SerializeField] AnimationCurve slideCurve;
    [SerializeField] Vector3 finalPosition;
    [SerializeField] float speed = 0.5f;
    bool movingTowards = true;
    public bool MovingTowards { set { movingTowards = value; } }

    private Vector3 initialPosition;

    private void Awake()
    {
        initialPosition = transform.position;
    }

    private void Start()
    {
        StartCoroutine(MoveTo());
    }

    public IEnumerator MoveTo()
    {
        float i = 0;
        float rate = 1 / speed;
        while (i < slideCurve.length && movingTowards)
        {
            i += speed * Time.deltaTime;
            transform.position = Vector2.Lerp(initialPosition, finalPosition, slideCurve.Evaluate(i));
            yield return 0;
        }
    }

    public IEnumerator MoveTo(Vector3 _pos)
    {
        float i = 0;
        float rate = 1 / speed;
        while (i < slideCurve.length)
        {
            i += speed * Time.deltaTime;
            transform.position = Vector2.Lerp(transform.position, _pos, slideCurve.Evaluate(i));
            yield return 0;
        }
    }

    public IEnumerator MoveBackwards()
    {
        float i = 0;
        float rate = 1 / speed;
        while (i < slideCurve.length && !movingTowards)
        {
            i += speed * Time.deltaTime;
            transform.position = Vector2.Lerp(finalPosition, initialPosition, slideCurve.Evaluate(i));
            yield return 0;
        }
    }
}
