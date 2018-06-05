using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public Camera gameCamera;
    public Player player;

    private float minCamY;
    private float maxCamY;
    private float camY;

    private float mapWidth;
    private float mapHeight;

    private float viewWidth;
    private float viewHeight;

    private Map map;

    public static Game Instance = null;

    private void Awake()
    {
        Instance = this;
    }

    void Start ()
    {
        map = new Map();
        map.Load(1);

        float screenRatio = (float)Screen.width / Screen.height;

        mapWidth = map.GetRealWidth();
        mapHeight = map.GetRealHeight();

        viewWidth = mapWidth;
        viewHeight = viewWidth / screenRatio;

        gameCamera.orthographicSize = minCamY = viewHeight / 2.0f;
        maxCamY = mapHeight - minCamY;

        Vector3 camPos = gameCamera.transform.localPosition;
        camPos.x = viewWidth / 2.0f;
        camPos.y = camY = minCamY;
        gameCamera.transform.localPosition = camPos;

        player.SetPosition(new Vector2(camPos.x, 0.0f));
    }

    private void MoveCameraY(float dist)
    {
        SetCameraY(camY + dist);
    }

    private void SetCameraY(float y)
    {
        camY = Mathf.Clamp(y, minCamY, maxCamY);
        Vector3 camPos = gameCamera.transform.localPosition;
        camPos.y = camY;
        gameCamera.transform.localPosition = camPos;
    }

    public Vector2 GetMove(Vector2 position, float width, float height, Vector2 move)
    {
        return map.GetMove(position, width, height, move);
    }

    void Update ()
    {
        float dt = Time.deltaTime;

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        if (x != 0.0f || y != 0.0f)
        {
            Vector2 move = new Vector2(x, y) * Constants.MOVE_SPEED * dt;
            player.Move(move);

            SetCameraY(player.GetPosition().y + viewHeight / 2.0f);
        }
	}
}
