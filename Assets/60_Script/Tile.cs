using UnityEngine;
using System.Collections;
using Commons;

public class Tile {

    public Point point { get; set; }
    public Transform trans { get; set; }
    public int cost { get; set; }

    public Tile(Point point, Transform trans, int cost) {
        this.point = point;
        this.trans = trans;
        this.cost = cost;
    }

}
