using UnityEngine;

public class SwipeDetection : MonoBehaviour
{
    // ��������� �� ����� �� ������
    private bool fingerDown;

    // ��������� ������� �� ������� ��� ����������� Swipe
    private Vector2 startPos;

    // ���������� ��� ����������� �������� �������������� Swipe
    public int pixelDistToDetect = 20;

    // ���������� ��������� Swipe
    private static Swipe swipe;

    // ���������� Swipe
    public static Swipe getSwipe()
    {
        return swipe;
    }

    // ���������� �������� Swipe
    public static void reset()
    {
        swipe = Swipe.None;
    }

    // ����������
    void Update()
    {
        
        // ������������ ������ ������� �� �����
        if (fingerDown == false && Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began) 
        {
            startPos = Input.touches[0].position;
            fingerDown = true;
        }

        // ������������ �������� �� ����� ������� �� ������
        if (fingerDown && Input.touches[0].phase == TouchPhase.Moved)
        {
            // ���������, ��������� �� ����� ������ ��� ����������� Swipe 
            if (Input.touches[0].position.y >= startPos.y + pixelDistToDetect)
            {
                fingerDown = false;
                swipe = Swipe.Up;
            }
            else if (Input.touches[0].position.y <= startPos.y - pixelDistToDetect)
            {
                fingerDown = false;
                swipe = Swipe.Down;
            }
            else if (Input.touches[0].position.x <= startPos.x - pixelDistToDetect)
            {
                fingerDown = false;
                swipe = Swipe.Left;
            }
            else if (Input.touches[0].position.x >= startPos.x + pixelDistToDetect)
            {
                fingerDown = false;
                swipe = Swipe.Right;
            } 
        }

        // ������������ ���������� ������ �� ������
        if (fingerDown && Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended)
        {
            fingerDown = false;
        }
    }

}
