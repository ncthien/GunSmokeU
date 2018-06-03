using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    private Map map;

	void Start ()
    {
        map = new Map();
        map.Load(1);
	}
	
	void Update ()
    {
		
	}
}
