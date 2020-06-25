using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raavanan
{
    public class GridManager : MonoBehaviour
    {
        private Transform mGridParentT;
        private Vector3 mMinpos;

        private Vector3Int[] mGridSizes;
        public float scaleY = 2;
        public float[] scales = { 1, 2 };
        public List<Node[,,]> grids = new List<Node[,,]> ();

        private void Start()
        {
            Initialize();
            GridUnit[] gridUnits = GameObject.FindObjectsOfType<GridUnit>();
            foreach (GridUnit unit in gridUnits)
            {
                Node node = GetNode(unit.startPosition, unit.gridIndex);
                unit.transform.position = node.worldPosition;
            }
        }

        private void Initialize ()
        {
            mGridParentT = new GameObject("Parent").transform;
            GridPosition[] gridPos = GameObject.FindObjectsOfType<GridPosition>();
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minZ = minX;
            float maxZ = maxX;
            float minY = minX;
            float maxY = maxX;
            int i = 0;
            for (;i<gridPos.Length;i++)
            {
                Transform t = gridPos[i].transform;
                minX = (t.position.x < minX) ? t.position.x : minX;
                maxX = (t.position.x > maxX) ? t.position.x : maxX;
                minZ = (t.position.z < minZ) ? t.position.z : minZ;
                maxZ = (t.position.z > maxZ) ? t.position.z : maxZ;
                minY = (t.position.y < minY) ? t.position.y : minY;
                maxY = (t.position.y > maxY) ? t.position.y : maxY;
            }

            mMinpos = Vector3.zero;
            mMinpos.x = minX;
            mMinpos.y = minY;
            mMinpos.z = minZ;

            mGridParentT.position = mMinpos;
            mGridSizes = new Vector3Int[scales.Length];

            for (i = 0; i < scales.Length; i++)
            {
                mGridSizes[i] = new Vector3Int();
                mGridSizes[i].x = Mathf.FloorToInt((maxX - minX) / scales[i]);
                mGridSizes[i].y = Mathf.FloorToInt((maxY - minY) / scaleY);
                mGridSizes[i].z = Mathf.FloorToInt((maxZ - minZ) / scales[i]);
                grids.Add (CreateGrid(mGridSizes[i], scales[i]));
            }
        }

        private Node[,,] CreateGrid (Vector3Int gridSize_, float scaleXZ_)
        {
            Node[,,] grid = new Node[gridSize_.x + 1, gridSize_.y + 1, gridSize_.z + 1];
            for (int x = 0; x < gridSize_.x + 1; x++)
            {
                for (int z = 0; z < gridSize_.z + 1; z++)
                {
                    for (int y = 0; y < gridSize_.y + 1; y++)
                    {
                        Node n = new Node();
                        n.position.x = x;
                        n.position.y = y;
                        n.position.z = z;

                        n.pivotPosition.x = x * scaleXZ_;
                        n.pivotPosition.y = y * scaleY;
                        n.pivotPosition.z = z * scaleXZ_;
                        n.pivotPosition += mMinpos;

                        float scaleDiff = scaleXZ_ / 2;
                        n.worldPosition = n.pivotPosition;
                        n.worldPosition -= new Vector3(scaleDiff, 0, scaleDiff);

                        grid[x, y, z] = n;
                        CreateNode(n, scaleXZ_);

                    }
                }
            }
            return grid;
        }

        private void CreateNode (Node node_, float scale_)
        {
            GameObject GO = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(GO.GetComponent<Collider>());
            GO.transform.localScale = Vector3.one * 0.95f * scale_;
            Vector3 targetPos = node_.pivotPosition;
            targetPos.x += GO.transform.localScale.x / 2;
            targetPos.z += GO.transform.localScale.z / 2;

            GO.transform.position = targetPos;
            GO.transform.eulerAngles = new Vector3(90, 0, 0);
            GO.transform.parent = mGridParentT;
        }

        private Node GetNode (Vector3 worldPosition_, int gridIndex_)
        {
            Vector3Int pos = new Vector3Int();
            pos.x = Mathf.FloorToInt(worldPosition_.x / scales[gridIndex_]);
            pos.y = Mathf.FloorToInt(worldPosition_.y / scaleY);
            pos.z = Mathf.FloorToInt(worldPosition_.z / scales[gridIndex_]);
            return GetNode(pos, gridIndex_);
        }

        private Node GetNode(Vector3Int position_, int gridIndex_)
        {
            Vector3Int size = mGridSizes[gridIndex_];
            if (position_.x < 0 || position_.y < 0 || position_.z < 0 ||
                position_.x > size.x || position_.y > size.y || position_.z > size.z)
                return null;
            return grids[gridIndex_][position_.x, position_.y, position_.z];
        }
    }
}
