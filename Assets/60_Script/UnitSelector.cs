using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Commons;
using System;

public class UnitSelector : MonoBehaviour {

    public Transform self;

    UnitSelectStatus mySelectStatus = UnitSelectStatus.Neutral;
    Point myPoint;
    int moveCost = 4;
    Transform selectedUnit;
    

    List<Point> selectedPoints = new List<Point>();
    Dictionary<Point, Tile> tileList = new Dictionary<Point, Tile>();

    float _moveStartTime;
    Vector3 _moveDst;
    Vector3 _moveSrc;
    float _time = 1.0f;

    // Use this for initialization
    void Start () {
        myPoint.x = (int)Math.Round(self.position.x, MidpointRounding.AwayFromZero);
        myPoint.z = (int)Math.Round(self.position.z, MidpointRounding.AwayFromZero);
    }
	
	// Update is called once per frame
	void Update () {

        // 移動中は移動処理以外何もできない
        if(mySelectStatus == UnitSelectStatus.Moving)
        {
            var diff = Time.time - _moveStartTime;
            // 目的地に着いた
            if (diff > _time)
            {
                myPoint.x = (int)Math.Round(_moveDst.x, MidpointRounding.AwayFromZero);
                myPoint.z = (int)Math.Round(_moveDst.z, MidpointRounding.AwayFromZero);
                mySelectStatus = UnitSelectStatus.Neutral;

            }
            else
            {
                var rate = diff / _time;
                selectedUnit.position = Vector3.Lerp(_moveSrc, _moveDst, rate);
                return;
            }
        }

        // マップクリック時
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();

            switch (mySelectStatus)
            {
                case UnitSelectStatus.Neutral:
                    // ユニット選択
                    if (Physics.Raycast(ray, out hit, 1000000, 1 << 8))
                    {
                        this.mySelectStatus = UnitSelectStatus.Selected;
                        selectedUnit = hit.transform;

                        viewMovableTiles();
                    }
                    break;
                case UnitSelectStatus.Selected:

                    if (Physics.Raycast(ray, out hit, 1000000, 1 << 8))
                    {
                        //TODO:プレイヤーを選択したのでその場で待機するかどうかのメニューを出す
                        Debug.Log("プレイヤーのメニューを表示する");
                    }else {
                        bool selectedClear = false;
                        if (Physics.Raycast(ray, out hit, 1000000, 1 << 9))
                        {
                            // そのタイルの座標へ移動する
                            if (hit.collider.GetComponent<Renderer>().enabled)
                            {
                                mySelectStatus = UnitSelectStatus.Moving;

                                Vector3 movedPos = hit.transform.position;
                                movedPos.y = 1.5f;

                                _moveStartTime = Time.time;
                                _moveSrc = selectedUnit.position;
                                _moveDst = movedPos;

                                this.clearMovableTile(false);
                            } else
                            {
                                selectedClear = true;
                            }
                        }
                        else
                        {
                            // ユニット・床以外がクリックされた
                            selectedClear = true;
                        }

                        if(selectedClear)
                        {
                            clearMovableTile(true);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }

    private void clearMovableTile(bool isResetUnitStatus)
    {
        if (isResetUnitStatus)
        {
            this.mySelectStatus = UnitSelectStatus.Neutral;
            selectedUnit = null;
        }

        // タイルの選択状態をクリア
        Dictionary<Point, Tile> tileList = TileManager.getTileList();
        foreach (Point point in selectedPoints)
        {
            if (tileList.ContainsKey(point))
            {
                Transform tile = tileList[point].trans.FindChild("Plane");
                tile.GetComponent<Renderer>().enabled = false;
            }
        }
    }

    #region ユニットの移動範囲関連
    /// <summary>
    /// 移動範囲を表示する
    /// </summary>
    private void viewMovableTiles()
    {
        this.tileList = TileManager.getTileList();
        selectedPoints.Clear();

        int leftCost = moveCost;
        
        myPoint.x = (int)Math.Round(self.position.x, MidpointRounding.AwayFromZero);
        myPoint.z = (int)Math.Round(self.position.z, MidpointRounding.AwayFromZero);
        this.searchDirection(myPoint, Direction.North, leftCost);

        leftCost = moveCost;
        this.searchDirection(myPoint, Direction.East, leftCost);

        leftCost = moveCost;
        this.searchDirection(myPoint, Direction.South , leftCost);

        leftCost = moveCost;
        this.searchDirection(myPoint, Direction.West, leftCost);

        foreach (Point point in selectedPoints)
        {
            if(tileList.ContainsKey(point))
            {
                Transform tile = tileList[point].trans.FindChild("Plane");
                tile.GetComponent<Renderer>().enabled = true;
            }
        }
    }

    /// <summary>
    /// 移動範囲の探索
    /// </summary>
    /// <param name="point"></param>
    /// <param name="dir"></param>
    /// <param name="leftCost"></param>
    private void searchDirection(Point point, Direction dir, int leftCost)
    {
        Point wkPoint;
        switch (dir)
        {
            case Direction.North:
                wkPoint = point;
                wkPoint.z++;
                if(this.searchTile(wkPoint, leftCost))
                {
                    this.searchDirection(wkPoint, Direction.North, leftCost - this.tileList[wkPoint].cost);
                }

                if(leftCost == 0) { return; }

                wkPoint = point;
                wkPoint.x++;
                if (this.searchTile(wkPoint, leftCost))
                {
                    this.searchDirection(wkPoint, Direction.East, leftCost - this.tileList[wkPoint].cost);
                }

                if (leftCost == 0) { return; }

                wkPoint = point;
                wkPoint.x--;
                if (this.searchTile(wkPoint, leftCost))
                {
                    this.searchDirection(wkPoint, Direction.West, leftCost - this.tileList[wkPoint].cost);
                }
                break;

            case Direction.East:

                wkPoint = point;
                wkPoint.x++;
                if (this.searchTile(wkPoint, leftCost))
                {
                    this.searchDirection(wkPoint, Direction.East, leftCost - this.tileList[wkPoint].cost);
                }

                if (leftCost == 0) { return; }

                wkPoint = point;
                wkPoint.z--;
                if (this.searchTile(wkPoint, leftCost))
                {
                    this.searchDirection(wkPoint, Direction.South, leftCost - this.tileList[wkPoint].cost);
                }

                if (leftCost == 0) { return; }

                wkPoint = point;
                wkPoint.z++;
                if (this.searchTile(wkPoint, leftCost))
                {
                    this.searchDirection(point, Direction.North, leftCost - this.tileList[wkPoint].cost);
                }

                break;
            case Direction.South:
                wkPoint = point;
                wkPoint.z--;
                if (this.searchTile(wkPoint, leftCost))
                {
                    this.searchDirection(wkPoint, Direction.South, leftCost - this.tileList[wkPoint].cost);
                }

                if (leftCost == 0) { return; }

                wkPoint = point;
                wkPoint.x--;
                if (this.searchTile(wkPoint, leftCost))
                {
                    this.searchDirection(wkPoint, Direction.West, leftCost - this.tileList[wkPoint].cost);
                }

                if (leftCost == 0) { return; }

                wkPoint = point;
                wkPoint.x++;
                if (this.searchTile(wkPoint, leftCost))
                {
                    this.searchDirection(wkPoint, Direction.East, leftCost - this.tileList[wkPoint].cost);
                }
                break;
            case Direction.West:
                wkPoint = point;
                wkPoint.x--;
                if (this.searchTile(wkPoint, leftCost))
                {
                    this.searchDirection(wkPoint, Direction.West, leftCost - this.tileList[wkPoint].cost);
                }

                if (leftCost == 0) { return; }

                wkPoint = point;
                wkPoint.z++;
                if (this.searchTile(wkPoint, leftCost))
                {
                    this.searchDirection(wkPoint, Direction.North, leftCost - this.tileList[wkPoint].cost);
                }

                if (leftCost == 0) { return; }

                wkPoint = point;
                wkPoint.z--;
                if (this.searchTile(wkPoint, leftCost))
                {
                    this.searchDirection(wkPoint, Direction.South, leftCost - this.tileList[wkPoint].cost);
                }
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 指定座標に移動可能かチェック
    /// </summary>
    /// <param name="point"></param>
    /// <param name="leftCost"></param>
    /// <returns></returns>
    private bool searchTile(Point point, int leftCost)
    {
        if(point.x < 0 || point.z < 0) { return false; }

        if (this.tileList.ContainsKey(point) && leftCost - this.tileList[point].cost >= 0)
        {

                if (!selectedPoints.Contains(point))
                {
                    selectedPoints.Add(point);
                }
                return true;
        } else
        {
            return false;
        }

    }
    
    #endregion

}
