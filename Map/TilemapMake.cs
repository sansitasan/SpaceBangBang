using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SpaceBangBang
{
    public class TilemapMake : MonoBehaviour
    {
        [Header("Tile")]
        [SerializeField]
        private Tile ceiling;
        [SerializeField]
        private List<Tile> walls;
        [SerializeField]
        private Tile edge;
        [SerializeField]
        private Tile wallnground;
        [SerializeField]
        private Tile ground;
        [SerializeField]
        private Tile edgeground;

        [Header("TileMap")]
        [SerializeField]
        private Tilemap groundtilemap;
        [SerializeField]
        private Tilemap walltilemap;
        [SerializeField]
        private Tilemap objecttilemap;

        [SerializeField]
        private Grid grid;
        // Start is called before the first frame update
        void Start()
        {
            grid.cellSize = new Vector3(4, 4, 0);
            //CreateMap(30, 15);
        }

        public void CreateMap(int x, int y)
        {
            CreateWall(x, y);
            CreateGround(x, y);
        }

        public void CreateGround(int x, int y)
        {
            int i, j;
            Matrix4x4 matrix;

            //�Ϲ� �׶���
            for (i = -y + 4; i < y - 3; ++i)
            {
                for (j = -x + 4; j < x - 3; ++j)
                {
                    groundtilemap.SetTile(new Vector3Int(j, i, 0), ground);
                }
            }

            //�� ��ó �׶��� �׸��� - ����, �»�� �𼭸�
            for (i = -x + 4; i < x - 3; i++)
            {
                groundtilemap.SetTile(new Vector3Int(i, y - 3, 0), wallnground);
            }
            groundtilemap.SetTile(new Vector3Int(-x + 3, y - 3, 0), edgeground);

            //�� ��ó �׶��� �׸��� - �Ʒ���, ���ϴ� �𼭸�
            matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -180f), Vector3.one);
            for (i = -x + 4; i < x - 3; i++)
            {
                groundtilemap.SetTile(new Vector3Int(i, -y + 3, 0), wallnground);
                groundtilemap.SetTransformMatrix(new Vector3Int(i, -y + 3, 0), matrix);
            }
            groundtilemap.SetTile(new Vector3Int(x - 3, -y + 3, 0), edgeground);
            groundtilemap.SetTransformMatrix(new Vector3Int(x - 3, -y + 3, 0), matrix);

            //�� ��ó �׶��� �׸��� - ������, ���� �𼭸�
            matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -90f), Vector3.one);
            for (i = -y + 4; i < y - 3; i++)
            {
                groundtilemap.SetTile(new Vector3Int(x - 3, i, 0), wallnground);
                groundtilemap.SetTransformMatrix(new Vector3Int(x - 3, i, 0), matrix);
            }
            groundtilemap.SetTile(new Vector3Int(x - 3, y - 3, 0), edgeground);
            groundtilemap.SetTransformMatrix(new Vector3Int(x - 3, y - 3, 0), matrix);

            //�� ��ó �׶��� �׸��� - ����, ���ϴ� �𼭸�
            matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -270f), Vector3.one);
            for (i = -y + 4; i < y - 3; i++)
            {
                groundtilemap.SetTile(new Vector3Int(-x + 3, i, 0), wallnground);
                groundtilemap.SetTransformMatrix(new Vector3Int(-x + 3, i, 0), matrix);
            }
            groundtilemap.SetTile(new Vector3Int(-x + 3, -y + 3, 0), edgeground);
            groundtilemap.SetTransformMatrix(new Vector3Int(-x + 3, -y + 3, 0), matrix);
        }

        public void CreateWall(int x, int y)
        {
            int i, j;

            //�� ���ʸ� �׸���
            for (i = -x + 1; i < x; ++i)
                for (j = y - 2; j < y; ++j)
                {
                    walltilemap.SetTile(new Vector3Int(i, j, 0), RandomTile(walls));
                }

            for (i = -x + 1; i < x; ++i)
            {
                walltilemap.SetTile(new Vector3Int(i, y, 0), ceiling);
            }

            //Ÿ�� �����ڸ� �׸���
            walltilemap.SetTile(new Vector3Int(-x, -y, 0), edge);
            walltilemap.SetTile(new Vector3Int(x, -y, 0), edge);
            walltilemap.SetTile(new Vector3Int(-x, y, 0), edge);
            walltilemap.SetTile(new Vector3Int(x, y, 0), edge);

            //�� ������ �� �׸���
            Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -90f), Vector3.one);
            for (i = -y + 1; i < y; ++i)
            {
                walltilemap.SetTile(new Vector3Int(x, i, 0), ceiling);
                walltilemap.SetTransformMatrix(new Vector3Int(x, i, 0), matrix);
            }

            for (i = -y + 3; i < y - 2; ++i)
                for (j = x - 2; j < x; ++j)
                {
                    walltilemap.SetTile(new Vector3Int(j, i, 0), RandomTile(walls));
                    walltilemap.SetTransformMatrix(new Vector3Int(j, i, 0), matrix);
                }

            //�� ���ʸ� �׸���
            matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -270f), Vector3.one);
            for (i = -y + 1; i < y; ++i)
            {
                walltilemap.SetTile(new Vector3Int(-x, i, 0), ceiling);
                walltilemap.SetTransformMatrix(new Vector3Int(-x, i, 0), matrix);
            }

            for (i = -y + 3; i < y - 2; ++i)
                for (j = -x + 1; j < -x + 3; ++j)
                {
                    walltilemap.SetTile(new Vector3Int(j, i, 0), RandomTile(walls));
                    walltilemap.SetTransformMatrix(new Vector3Int(j, i, 0), matrix);
                }

            //�� �Ʒ��� �׸���
            matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -180f), Vector3.one);
            for (i = -x + 1; i < x; ++i)
                for (j = -y + 1; j < -y + 3; ++j)
                {
                    walltilemap.SetTile(new Vector3Int(i, j, 0), RandomTile(walls));
                    walltilemap.SetTransformMatrix(new Vector3Int(i, j, 0), matrix);
                }

            for (i = -x + 1; i < x; ++i)
            {
                walltilemap.SetTile(new Vector3Int(i, -y, 0), ceiling);
                walltilemap.SetTransformMatrix(new Vector3Int(i, -y, 0), matrix);
            }
        }

        private Tile RandomTile(List<Tile> tile)
        {
            return tile[Random.Range(0, tile.Count)];
        }
    }
}