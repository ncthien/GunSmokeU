using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    private Vector2 position;
    public float height;
    public float width;

    private Transform cachedTransform;

    private void Awake()
    {
        cachedTransform = transform;
    }

    public void Move(Vector2 move)
    {
        move = Game.Instance.GetMove(position, width, height, move);
        SetPosition(position + move);
    }

    public void SetPosition(Vector2 position)
    {
        this.position = position;
        cachedTransform.localPosition = position;
    }

    public Vector2 GetPosition()
    {
        return position;
    }
}
