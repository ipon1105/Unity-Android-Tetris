using System.Data;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Класс описывающий работы с границами карты
/// </summary>
public class Board : MonoBehaviour
{
    // Игровая карта
    public Tilemap tilemap { get; private set; }

    // Активная фигура
    public Piece activePiece { get; private set; }

    // Информация о фигурах
    public TetrominoData[] tetrominoes;

    // Размеры границ карты 20х10
    public Vector2Int boardSize = new Vector2Int(10, 20);

    // Место появления фигуры на карте
    public Vector3Int spawnPosition = new Vector3Int(-1, 8, 0);

    // Место появления следующей фигуры
    public Vector3Int nextPieceSpawnPosition = new Vector3Int(7, 8, 0);

    // Индекс следующей фигуры
    public int nextTetrominos = -1;

    // Формируем границы из карты
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
        // Подгружаем основные переменные из сцены
        tilemap = GetComponentInChildren<Tilemap>();
        activePiece = GetComponentInChildren<Piece>();

        // Инициализируем каждую фигуру
        for (int i = 0; i < tetrominoes.Length; i++)
        {
            tetrominoes[i].Initialize();
        }
    }

    /// <summary>
    /// Первая инициализация
    /// </summary>
    private void Start()
    {
        SpawnPiece();
    }

    /// <summary>
    /// Создаём фигуру
    /// </summary>
    public void SpawnPiece()
    {
        // Генерируем рандомный индекс фигуры и копируем новую фигуру
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

        // Инициализируем рандомную фигуру data, как активную в точке spawnPosition 
        activePiece.Initialize(this, spawnPosition, data);

        // Проверяем, не накладывается ли фигура на точку spawnPosition
        if (IsValidPosition(activePiece, spawnPosition))
        {
            // Если не накладывается, устанавливаем новую активную фигуру
            Set(activePiece);
            ShowNextPiece();
        }
        else
        {
            // Если накладывается, вызываем окончания игры
            GameOver();
        }
    }

    /// <summary>
    /// Отображаем следующую фигуру
    /// </summary>
    public void ShowNextPiece()
    {
        ClearNextPiece();

        TetrominoData data = tetrominoes[nextTetrominos];
        Vector3Int[] cells = new Vector3Int[data.cells.Length]; ;
        for (int i = 0; i < cells.Length; i++)
            cells[i] = (Vector3Int) data.cells[i];

        // Поворачиваем I фигуру
        if (nextTetrominos == 0)
        {
            int direction = 1;

            // Получаем матрицу поворота
            float[] matrix = Data.RotationMatrix;

            // Поворачиваем все кирпичики (ячейки - cells) в нужную сторону
            // С помощью матрицу повора
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

                // Записываем новые значения
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
    /// Удаляем следующую фигуру
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
    /// Игра окончена
    /// </summary>
    public void GameOver()
    {
        // Отчищаем все клетк
        tilemap.ClearAllTiles();

        UI_Manager.ScoreUpdate();
    }

    /// <summary>
    /// Устанавливаем фигуру
    /// </summary>
    /// <param name="piece"></param>
    public void Set(Piece piece)
    {
        // Пробегаемся по всем кирпичикам (ячейкам - cells)
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    /// <summary>
    /// Удаляем фигуру
    /// </summary>
    /// <param name="piece"></param>
    public void Clear(Piece piece)
    {
        // Пробегаемся по всем кирпичикам (ячейкам - cells)
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, null);
        }
    }

    /// <summary>
    /// Проверяем, является ли позиция(position) свободной для фигуры (piece)
    /// </summary>
    /// <param name="piece">Фигура</param>
    /// <param name="position">Позиция</param>
    /// <returns></returns>
    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = Bounds;

        // Проверяем для каждой ячейки
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;
            
            // Выход за границы карты
            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }

            // Позиция уже занята
            if (tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Выполняем отчистку карты от полных линий и возвращаем количество удалённыйх линий
    /// </summary>
    public void ClearLines()
    {
        int linesCount = 0;

        // Превращаем карту в RectInt
        RectInt bounds = Bounds;

        // Индекс линии от начала до конца
        int row = bounds.yMin;

        // Пробегаемся по всем линиям снизу вверх
        while (row < bounds.yMax)
        {
            // Проверяем линию на полноту
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
    /// Проверяем, является ли линия row заполненной
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    public bool IsLineFull(int row)
    {
        RectInt bounds = Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {

            // Линия не является полной, если на карте нет ячейки
            if (!tilemap.HasTile(new Vector3Int(col, row, 0)))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Отчищаем линию row от ячеек
    /// </summary>
    /// <param name="row"></param>
    public void LineClear(int row)
    {
        RectInt bounds = Bounds;

        // Отчищаем все ячейки в строке
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            tilemap.SetTile(position, null);
        }

        // Сдвигаем все линии вниз
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
