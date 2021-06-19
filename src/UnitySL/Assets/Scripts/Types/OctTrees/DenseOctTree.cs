using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Types.OctTrees
{
    public class DenseOctTree<T> where T : IPositionable
    {
        public DenseOctTree(Vector3 minPosition, Vector3 maxPosition, int depth)
        {
            Space = new Cell<T>(minPosition, maxPosition);

            for (int i = 0; i < depth; i++)
            {
                Space.Subdivide();
            }
        }

        public Cell<T> Space { get; protected set; }

        public void Add(T item)
        {
            Space.Add(item);
        }
        public void Remove(T item)
        {
            Space.Remove(item);
        }

        public List<T> FindNearby(Vector3 pos, float radius)
        {
            List<T> items = new List<T>();
            foreach (Cell<T> cell in Space.GetCells(pos, radius))
            {
                if (cell == null)
                {
                    continue;
                }
                items.AddRange(cell.Items.Where(item => Vector3.Distance(pos, item.Position) < radius));
            }
            return items;
        }
    }

    public class Cell<T> where T : IPositionable
    {
        public Cell (Vector3 minPosition, Vector3 maxPosition)
        {
            MinPosition = minPosition;
            MaxPosition = maxPosition;
            MidPosition = MinPosition + (MaxPosition - MinPosition) / 2f;
        }

        public Vector3 MinPosition { get; protected set; }
        public Vector3 MidPosition { get; protected set; }
        public Vector3 MaxPosition { get; protected set; }

        public List<T> Items = new List<T>();

        public enum CellId
        {
            BottomSouthWest = 0,
            BottomSouthEast = 1,
            BottomNorthEast = 2,
            BottomNorthWest = 3,
            TopSouthWest    = 4,
            TopSouthEast    = 5,
            TopNorthEast    = 6,
            TopNorthWest    = 7
        }

        public bool HasSubdivisions => Subdivisions[0] != null;
        public Cell<T>[] Subdivisions { get; protected set; } = new Cell<T>[8];

        public void Subdivide()
        {
            if (!HasSubdivisions)
            {
                Vector3 p000 = new Vector3(MinPosition.x, MinPosition.y, MinPosition.z);
                Vector3 p100 = new Vector3(MidPosition.x, MinPosition.y, MinPosition.z);
                Vector3 p001 = new Vector3(MinPosition.x, MinPosition.y, MidPosition.z);
                Vector3 p101 = new Vector3(MidPosition.x, MinPosition.y, MidPosition.z);
                Vector3 p010 = new Vector3(MinPosition.x, MidPosition.y, MinPosition.z);
                Vector3 p110 = new Vector3(MidPosition.x, MidPosition.y, MinPosition.z);
                Vector3 p011 = new Vector3(MinPosition.x, MidPosition.y, MidPosition.z);
                Vector3 p111 = new Vector3(MidPosition.x, MidPosition.y, MidPosition.z);
                Vector3 p112 = new Vector3(MidPosition.x, MidPosition.y, MaxPosition.z);
                Vector3 p211 = new Vector3(MaxPosition.x, MidPosition.y, MidPosition.z);
                Vector3 p212 = new Vector3(MaxPosition.x, MidPosition.y, MaxPosition.z);
                Vector3 p121 = new Vector3(MidPosition.x, MaxPosition.y, MidPosition.z);
                Vector3 p122 = new Vector3(MidPosition.x, MaxPosition.y, MaxPosition.z);
                Vector3 p221 = new Vector3(MaxPosition.x, MaxPosition.y, MidPosition.z);
                Vector3 p222 = new Vector3(MaxPosition.x, MaxPosition.y, MaxPosition.z);

                Subdivisions[0] = new Cell<T>(p000, p111);
                Subdivisions[1] = new Cell<T>(p100, p211);
                Subdivisions[2] = new Cell<T>(p101, p212);
                Subdivisions[3] = new Cell<T>(p001, p112);

                Subdivisions[4] = new Cell<T>(p010, p121);
                Subdivisions[5] = new Cell<T>(p110, p221);
                Subdivisions[6] = new Cell<T>(p111, p222);
                Subdivisions[7] = new Cell<T>(p011, p122);
                return;
            }

            foreach (var cell in Subdivisions)
            {
                cell.Subdivide();
            }
        }

        public Cell<T> GetCell(CellId id)
        {
            return Subdivisions[(int)id];
        }

        public Cell<T> GetCell(Vector3 pos)
        {
            if (!HasSubdivisions)
            {
                return this;
            }
            foreach (var cell in Subdivisions)
            {
                if (!cell.Surrounds(pos))
                {
                    continue;
                }
                return cell.GetCell(pos);
            }
            return null;
        }

        /// <summary>
        /// Returns an IEnumerable of cells within a radius from a point.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="radius"></param>
        /// <returns>If the cell is on the edge, the enumeration will contain null values for invalid cells.</returns>
        public IEnumerable<Cell<T>> GetCells(Vector3 pos, float radius)
        {
            Cell<T> centreCell = GetCell(pos);
            Vector3 cellSize = centreCell.MaxPosition - centreCell.MinPosition;
            Vector3Int numCells = new Vector3Int(Mathf.CeilToInt(radius / cellSize.x) + 1,
                                                 Mathf.CeilToInt(radius / cellSize.y) + 1,
                                                 Mathf.CeilToInt(radius / cellSize.z) + 1);
            Cell<T>[] cells = new Cell<T>[numCells.x * numCells.y * numCells.z];
            Vector3 startPos = centreCell.MidPosition;
            startPos.x -= cellSize.x * (numCells.x / 2f + pos.x < startPos.x ? 1 : 0);
            startPos.y -= cellSize.y * (numCells.y / 2f + pos.y < startPos.y ? 1 : 0);
            startPos.z -= cellSize.z * (numCells.z / 2f + pos.z < startPos.z ? 1 : 0);

            Vector3 p = startPos;
            int index = 0;
            p.z = startPos.z;
            for (int k = 0; k < numCells.z; k++)
            {
                p.y = startPos.y;
                for (int j = 0; j < numCells.y; j++)
                {
                    p.x = startPos.x;
                    for (int i = 0; i < numCells.x; i++)
                    {
                        cells[index++] = GetCell(p);
                        p.x += cellSize.x;
                    }
                    p.y += cellSize.y;
                }
                p.z += cellSize.z;
            }
            return cells;
        }

        public void Add(T item)
        {
            if (!HasSubdivisions)
            {
                //Debug.Log($"Added item at {item.Position} to {this}");
                Items.Add(item);
                return;
            }
            foreach (var cell in Subdivisions)
            {
                if (!cell.Surrounds(item))
                {
                    continue;
                }

                cell.Add(item);
                return;
            }
            throw new Exception($"Tried to add an object that is outside of this oct tree. Position={item.Position}");
        }

        public void Remove(T item)
        {
            if (!HasSubdivisions)
            {
                Items.Remove(item);
                return;
            }
            foreach (var cell in Subdivisions)
            {
                if (cell.Surrounds(item))
                {
                    cell.Remove(item);
                    return;
                }
            }
        }

        private bool Surrounds(T item)
        {
            return Surrounds(item.Position);
        }
        private bool Surrounds(Vector3 pos)
        {
            return    pos.x >= MinPosition.x && pos.x < MaxPosition.x 
                   && pos.y >= MinPosition.y && pos.y < MaxPosition.y
                   && pos.z >= MinPosition.z && pos.z < MaxPosition.z;
        }

        public override string ToString()
        {
            return $"Cell {{MinPosition={MinPosition}, MidPosition={MidPosition}, MaxPosition={MaxPosition}}}";
        }
    }

    public interface IPositionable
    {
        Vector3 Position { get; }
    }
}
