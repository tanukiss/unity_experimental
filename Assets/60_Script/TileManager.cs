using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Commons;

public class TileManager : MonoBehaviour {

    public Transform tile;

    public static Dictionary<Point, Tile> TileList = new Dictionary<Point, Tile>();

	// Use this for initialization
	void Start () {
        TileList.Clear();

        //TODO:定義ファイルを作っておき、その定義に従いコストとマップを自動生成する。

        for (int z = 0; z < 20; z++)
        {
            for (int x = 0; x < 20; x++)
            {
                Point point = new Point();
                point.x = x;
                point.z = z;
                Transform obj = Instantiate(tile, new Vector3(x, 0.5f, z), Quaternion.identity) as Transform;

                Tile tileObj = new Tile(point, obj, 1);
                TileList.Add(point, tileObj);
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public static Dictionary<Point, Tile> getTileList()
    {
        return TileList;
    }

}
