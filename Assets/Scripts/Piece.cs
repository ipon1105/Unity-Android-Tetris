using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Piece : MonoBehaviour
{

    // ������� �����
    public Board board { get; private set; }

    // ��� ������
    public TetrominoData data { get; private set; }

    // ������ ����� � ������������
    public Vector3Int[] cells { get; private set; }

    // ������� �������
    public Vector3Int position { get; private set; }

    // ������� ������� ��������
    public int rotationIndex { get; private set; }

    // ������������ �������� ����� �����
    public float stepDelay = 1f;

    // ������������ �������� ����� ���������
    public float moveDelay = 0.1f;
    
    // ������������ �������� ����� ���������
    public float lockDelay = 0.5f;

    // ����� ����
    private float stepTime;

    // ����� ��������
    private float moveTime;

    // ����� ��������
    private float lockTime;

    // ������ ������
    private float ScreenWidth;

    // ��������� ��� ������ ��������
    private float Dist;

    // ������ ������
    public void Start()
    {
        ScreenWidth = Screen.width;
        Dist = (float)(ScreenWidth / 2) / 10;

    }

    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        // ��������� ������� �����(board), ������� ���������(position) � ������(data)
        this.data = data;
        this.board = board;
        this.position = position;

        // ���������� ������� � ��������� �������� (���, �������� � ��������)
        rotationIndex = 0;
        stepTime = Time.time + stepDelay;
        moveTime = Time.time + moveDelay;
        lockTime = 0f;

        // �������������� ������ � ������������
        if (cells == null)
        {
            cells = new Vector3Int[data.cells.Length];
        }
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = (Vector3Int)data.cells[i];
        }
    }

    private bool fingerDown = false;
    private float startPos;
    private float pos;
    private int rot = 0;
    private int drp = 0;
    /// <summary>
    /// ������� �������������� �������� ����� � ����������� �� �������� ������ �� ������
    /// </summary>
    /// <returns></returns>
    private int moving()
    {
        // ������������ ������ ������� �� �����
        if (fingerDown == false && Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            startPos = pos = Input.touches[0].position.x;
            fingerDown = true;
            return 0;
        }

        // ������������ �������� �� ����� ������� �� ������
        if (fingerDown && Input.touches[0].phase == TouchPhase.Moved)
        {
            pos = Input.touches[0].position.x;
        }

        // ������������ ���������� ������ �� ������
        if (fingerDown && Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended)
        {
            fingerDown = false;
            startPos = pos = 0;
            return 0;
        }

        if (((startPos - pos) / Dist) < 0)
            Debug.Log("step = " + ((int)Mathf.Ceil((startPos - pos) / Dist)).ToString());
        else
            Debug.Log("step = " + ((int)Mathf.Floor((startPos - pos) / Dist)).ToString());

        Debug.Log("startPos = " + (startPos).ToString());
        Debug.Log("currnPos = " + (pos).ToString());

        if (((startPos - pos) / Dist) < 0)
            return -(int)Mathf.Ceil((startPos - pos) / Dist);
        else
            return -(int)Mathf.Floor((startPos - pos) / Dist);
    }

    public void rotation(int rotation) 
    {
        rot = rotation;
    }

    public void drop()
    {
        drp = 1;
    }

    private void Update()
    {
        board.Clear(this);
        
        // ���������� ������ ��� ������ ������ �� �������
        lockTime += Time.deltaTime;

        int move = moving();
        if (move != 0) {
            Move(new Vector2Int(move, 0));
            startPos = pos;
        }

        // Handle rotation
        if (Input.GetKeyDown(KeyCode.Q) || rot == -1)
        {
            rot = 0;
            Rotate(-1);
        }
        else if (Input.GetKeyDown(KeyCode.E) || rot == 1)
        {
            rot = 0;
            Rotate(1);
        }

        // Handle hard drop
        if (Input.GetKeyDown(KeyCode.Space) || drp == 1)
        {
            HardDrop();
            drp = 0;
        }

        // Allow the player to hold movement keys but only after a move delay
        // so it does not move too fast
        if (Time.time > moveTime)
        {
            HandleMoveInputs();
        }

        // Advance the piece to the next row every x seconds
        if (Time.time > stepTime)
        {
            Step();
        }

        board.Set(this);
    }

    private void HandleMoveInputs()
    {
        /*Swipe swipe = SwipeDetection.getSwipe();*/

        // Soft drop movement
        if (Input.GetKey(KeyCode.S))
        {
            if (Move(Vector2Int.down))
            {
                // Update the step time to prevent double movement
                stepTime = Time.time + stepDelay;
            }
        }

        // Left/right movement
        if (Input.GetKey(KeyCode.A))
        {
            Move(Vector2Int.left);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            Move(Vector2Int.right);
        }
    }

    private void Step()
    {
        stepTime = Time.time + stepDelay;

        // ������� ������ ���� �� ��������� ������
        Move(Vector2Int.down);

        // ���� ����� �� ���������� ������ �������, �� ������� ������� Lock
        if (lockTime >= lockDelay)
        {
            Lock();
        }
    }

    private void HardDrop()
    {
        while (Move(Vector2Int.down))
        {
            continue;
        }

        Lock();
    }

    private void Lock()
    {
        board.Set(this);
        board.ClearLines();
        board.SpawnPiece();
    }

    private bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = board.IsValidPosition(this, newPosition);

        // Only save the movement if the new position is valid
        if (valid)
        {
            position = newPosition;
            moveTime = Time.time + moveDelay;
            lockTime = 0f; // reset
        }

        return valid;
    }

    public void Rotate(int direction)
    {
        // Store the current rotation in case the rotation fails
        // and we need to revert
        int originalRotation = rotationIndex;

        // Rotate all of the cells using a rotation matrix
        rotationIndex = Wrap(rotationIndex + direction, 0, 4);
        ApplyRotationMatrix(direction);

        // Revert the rotation if the wall kick tests fail
        if (!TestWallKicks(rotationIndex, direction))
        {
            rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
    }

    /// <summary>
    /// ������������ ������ � ������� direction
    /// </summary>
    /// <param name="direction"></param>
    private void ApplyRotationMatrix(int direction)
    {
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

    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = data.wallKicks[wallKickIndex, i];

            if (Move(translation))
            {
                return true;
            }
        }

        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;

        if (rotationDirection < 0)
        {
            wallKickIndex--;
        }

        return Wrap(wallKickIndex, 0, data.wallKicks.GetLength(0));
    }

    private int Wrap(int input, int min, int max)
    {
        if (input < min)
        {
            return max - (min - input) % (max - min);
        }
        else
        {
            return min + (input - min) % (max - min);
        }
    }

}
