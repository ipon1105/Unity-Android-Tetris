using UnityEngine;

public class SwipeDetection : MonoBehaviour
{
    // Находится ли палец на экране
    private bool fingerDown;

    // Начальная позиция от которой идёт регистрация Swipe
    private Vector2 startPos;

    // Расстояние при преодолении которого регистрируется Swipe
    public int pixelDistToDetect = 20;

    // Запоминаем последний Swipe
    private static Swipe swipe;

    // Возвращаем Swipe
    public static Swipe getSwipe()
    {
        return swipe;
    }

    // Сбрасываем значение Swipe
    public static void reset()
    {
        swipe = Swipe.None;
    }

    // Обновления
    void Update()
    {
        
        // Обрабатываем первое нажатие на экран
        if (fingerDown == false && Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began) 
        {
            startPos = Input.touches[0].position;
            fingerDown = true;
        }

        // Обрабатываем движения во время нажатия на экране
        if (fingerDown && Input.touches[0].phase == TouchPhase.Moved)
        {
            // Проверяем, преодолел ли Игрок придел для регистрации Swipe 
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

        // Обрабатываем отпускание пальца от экрана
        if (fingerDown && Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended)
        {
            fingerDown = false;
        }
    }

}
