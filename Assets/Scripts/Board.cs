using System.Data;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// ����� ����������� ������ � ��������� �����
/// </summary>
public class Board : MonoBehaviour
{
    // ������� �����
    public Tilemap tilemap { get; private set; }

    // �������� ������
    public Piece activePiece { get; private set; }

    // ���������� � �������
    public TetrominoData[] tetrominoes;

    // ������� ������ ����� 20�10
    public Vector2Int boardSize = new Vector2Int(10, 20);

    // ����� ��������� ������ �� �����
    public Vector3Int spawnPosition = new Vector3Int(-1, 8, 0);

    // ����� ��������� ��������� ������
    public Vector3Int nextPieceSpawnPosition = new Vector3Int(7, 8, 0);

    // ������ ��������� ������
    public int nextTetrominos = -1;

    // ��������� ������� �� �����
    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-boardSize.x / 2, -boardSize.y / 2);
            return new RectInt(position, boardSize);
        }
    }

    private void Awake()
    {
        // ���������� �������� ���������� �� �����
        tilemap = GetComponentInChildren<Tilemap>();
        activePiece = GetComponentInChildren<Piece>();

        // �������������� ������ ������
        for (int i = 0; i < tetrominoes.Length; i++)
        {
            tetrominoes[i].Initialize();
        }
    }

    /// <summary>
    /// ������ �������������
    /// </summary>
    private void Start()
    {
        SpawnPiece();
    }

    /// <summary>
    /// ������ ������
    /// </summary>
    public void SpawnPiece()
    {
        // ���������� ��������� ������ ������ � �������� ����� ������
        int random = 0;
        if (nextTetrominos == -1) 
        {
            random = Random.Range(0, tetrominoes.Length);
            nextTetrominos = Random.Range(0, tetrominoes.Length);
        }
        else 
        { 
            random = nextTetrominos;
            nextTetrominos = Random.Range(0, tetrominoes.Length);
        }
        TetrominoData data = tetrominoes[random];

        // �������������� ��������� ������ data, ��� �������� � ����� spawnPosition 
        activePiece.Initialize(this, spawnPosition, data);

        // ���������, �� ������������� �� ������ �� ����� spawnPosition
        if (IsValidPosition(activePiece, spawnPosition))
        {
            // ���� �� �������������, ������������� ����� �������� ������
            Set(activePiece);
            ShowNextPiece();
        }
        else
        {
            // ���� �������������, �������� ��������� ����
            GameOver();
        }
    }

    /// <summary>
    /// ���������� ��������� ������
    /// </summary>
    public void ShowNextPiece()
    {
        ClearNextPiece();

        TetrominoData data = tetrominoes[nextTetrominos];
        Vector3Int[] cells = new Vector3Int[data.cells.Length]; ;
        for (int i = 0; i < cells.Length; i++)
            cells[i] = (Vector3Int) data.cells[i];

        // ������������ I ������
        if (nextTetrominos == 0)
        {
            int direction = 1;

            // �������� ������� ��������
            float[] matrix = Data.RotationMatrix;

            // ������������ ��� ��������� (������ - cells) � ������ �������
            // � ������� ������� ������
            for (int i = 0; i < cells.Length; i++)
            {
                Vector3 cell = cells[i];

                int x, y;

                switch (data.tetromino)
                {
                    case Tetromino.I:
                    case Tetromino.O:
                        cell.x -= 0.5f;
                        cell.y -= 0.5f;
                        x = Mathf.CeilToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                        y = Mathf.CeilToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                        break;

                    default:
                        x = Mathf.RoundToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                        y = Mathf.RoundToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                        break;
                }

                // ���������� ����� ��������
                cells[i] = new Vector3Int(x, y, 0);
            }

        }
        
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3Int showPosition = new Vector3Int(0, 0, 0);
            if (nextTetrominos == 0)
                showPosition = cells[i] + (nextPieceSpawnPosition - new Vector3Int(1,1,0));
            else
                showPosition = cells[i] + nextPieceSpawnPosition;
            tilemap.SetTile(showPosition, data.tile);
        }
    }

    /// <summary>
    /// ������� ��������� ������
    /// </summary>
    public void ClearNextPiece()
    {
        for (int x = 6; x <= 8; x++)
        {
            for (int y = 6; y <= 9; y++)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), null);
            }
        }
    }

    /// <summary>
    /// ���� ��������
    /// </summary>
    public void GameOver()
    {
        // �������� ��� �����
        tilemap.ClearAllTiles();

        UI_Manager.ScoreUpdate();
    }

    /// <summary>
    /// ������������� ������
    /// </summary>
    /// <param name="piece"></param>
    public void Set(Piece piece)
    {
        // ����������� �� ���� ���������� (������� - cells)
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    /// <summary>
    /// ������� ������
    /// </summary>
    /// <param name="piece"></param>
    public void Clear(Piece piece)
    {
        // ����������� �� ���� ���������� (������� - cells)
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, null);
        }
    }

    /// <summary>
    /// ���������, �������� �� �������(position) ��������� ��� ������ (piece)
    /// </summary>
    /// <param name="piece">������</param>
    /// <param name="position">�������</param>
    /// <returns></returns>
    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = Bounds;

        // ��������� ��� ������ ������
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;
            
            // ����� �� ������� �����
            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }

            // ������� ��� ������
            if (tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// ��������� �������� ����� �� ������ ����� � ���������� ���������� ��������� �����
    /// </summary>
    public void ClearLines()
    {
        int linesCount = 0;

        // ���������� ����� � RectInt
        RectInt bounds = Bounds;

        // ������ ����� �� ������ �� �����
        int row = bounds.yMin;

        // ����������� �� ���� ������ ����� �����
        while (row < bounds.yMax)
        {
            // ��������� ����� �� �������
            if (IsLineFull(row))
            {
                LineClear(row);
                linesCount++;
            }
            else
            {
                row++;
            }
        }

        int Score = 0;
        if (linesCount == 1)
            Score += 100;
        if (linesCount == 2)
            Score += 300;
        if (linesCount == 3)
            Score += 700;
        if (linesCount == 4)
            Score += 1500;
        UI_Manager.AddScore(Score);
    }

    /// <summary>
    /// ���������, �������� �� ����� row �����������
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    public bool IsLineFull(int row)
    {
        RectInt bounds = Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {

            // ����� �� �������� ������, ���� �� ����� ��� ������
            if (!tilemap.HasTile(new Vector3Int(col, row, 0)))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// �������� ����� row �� �����
    /// </summary>
    /// <param name="row"></param>
    public void LineClear(int row)
    {
        RectInt bounds = Bounds;

        // �������� ��� ������ � ������
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            tilemap.SetTile(position, null);
        }

        // �������� ��� ����� ����
        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                tilemap.SetTile(position, above);
            }

            row++;
        }
    }

}
