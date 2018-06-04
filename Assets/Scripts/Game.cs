using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public Camera gameCamera;

    private float minCamY;
    private float maxCamY;
    private float camY;
        
    private Map map;

	void Start ()
    {
        map = new Map();
        map.Load(1);

        float screenRatio = (float)Screen.width / Screen.height;

        float viewWidth = map.GetRealWidth();
        float viewHeight = viewWidth / screenRatio;

        gameCamera.orthographicSize = minCamY = viewHeight / 2.0f;
        maxCamY = map.GetRealHeight() - minCamY;

        Vector3 camPos = gameCamera.transform.localPosition;
        camPos.x = viewWidth / 2.0f;
        camPos.y = camY = minCamY;
        gameCamera.transform.localPosition = camPos;
    }

    private void MoveCamera(float dist)
    {
        camY = Mathf.Clamp(camY + dist, minCamY, maxCamY);
        Vector3 camPos = gameCamera.transform.localPosition;
        camPos.y = camY;
        gameCamera.transform.localPosition = camPos;
    }
	
	void Update ()
    {
        float dt = Time.deltaTime;
        float move = Input.GetAxis("Vertical") * Constants.MOVE_SPEED * dt;
        MoveCamera(move);
	}
}
