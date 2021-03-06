﻿using NTUT.CSIE.GameDev.Game;
using NTUT.CSIE.GameDev.Scene;
using NTUT.CSIE.GameDev.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NTUT.CSIE.GameDev.Component.Map
{
    public class MapGridGenerator : CommonObject
    {
        private const int EMPTY = -1;
        public GameObject gridObject;
        public int col, row;
        public int curRow, curCol;
        public float y;
        private MapGrid[,] _mapGridArray;
        public bool ShowGridText = true;
        [SerializeField]
        protected HouseGenerator _houseGenerator;

        protected override void Awake()
        {
            base.Awake();
            _mapGridArray = new MapGrid[row, col];
            int step = 0;
            int halfStep = 0;
            int mapHeight = 0;
            DeleteChild();
            curCol = curRow = EMPTY;

            for (int c = 0; c < col; c++)
            {
                for (int r = 0; r < row; r++)
                {
                    var grid = Object.Instantiate(gridObject, this.gameObject.transform);
                    var mapGrid = grid.GetComponent<MapGrid>();
                    _mapGridArray[r, c] = mapGrid;

                    if (step == 0)
                    {
                        step = mapGrid.width;
                        halfStep = step >> 1;
                        mapHeight = 10 * step;
                    }

                    mapGrid.SetPosition(r, c);
                    mapGrid._generator = this;
                    mapGrid.Selected = false;
                    grid.name = mapGrid.ToString();
                    int posX = c * step + halfStep, posY = r * step + halfStep;
                    grid.transform.localPosition = new Vector3(posX, y, mapHeight - posY);
                }
            }

            curCol = EMPTY;
            curRow = EMPTY;
        }

        public void SetHighLight(Point p)
        {
            SetHighLight(p.Row, p.Column);
        }

        public void SetHighLight(int r, int c)
        {
            if (this.curCol != EMPTY && this.curRow != EMPTY)
            {
                this[curRow, curCol].Selected = false;
            }

            this.curCol = c;
            this.curRow = r;

            if (r == EMPTY || c == EMPTY) return;

            this[curRow, curCol].Selected = true;
        }

        private void ClearAllSelect()
        {
        }

        private void DeleteChild()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }

        public MapGrid this[int r, int c] => _mapGridArray[r, c];
        public MapGrid this[Point p] => _mapGridArray[p.Row, p.Column];
        public Point CurPoint => new Point(curRow, curCol);
        public HouseInfo CurHouseInfo => this.HouseGenerator[curRow, curCol];
        public HouseGenerator HouseGenerator => _houseGenerator;
    }
}
