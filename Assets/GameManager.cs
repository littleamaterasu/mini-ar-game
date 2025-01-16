using UnityEngine;
using TMPro;
using System;
using Random = System.Random;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public float scaleMultiplier = 0.1f; // Tốc độ scale theo độ nghiêng
    public float maxScale = 2.0f; // Kích thước tối đa của Image
    public float minScale = 0.5f; // Kích thước tối thiểu của Image

    public int level = 1; // Mức độ khó mặc định

    private Canvas canvas;
    private Image left, right, question;
    private TMP_Text leftText, rightText, questionText;

    private TMP_Text scoreText;
    private Button retryButton;

    private int a, b, ans, index, trueAns, wrongAns, n;
    private bool isProcessing = false;

    private Canvas menuPanel; // Panel menu
    private Button easyButton;
    private Button mediumButton;
    private Button hardButton;
    private Button extremeButton;
    private Button close;

    void Start()
    {
        
        trueAns = 0;
        wrongAns = 0;
        n = 1;

        // Lấy Canvas và các thành phần cần thiết
        if (!transform.Find("Canvas").TryGetComponent<Canvas>(out canvas))
        {
            Debug.LogError("Canvas không được tìm thấy! Vui lòng kiểm tra prefab.");
            return;
        }

        left = canvas.transform.Find("InGame").Find("A").GetComponent<Image>();
        right = canvas.transform.Find("InGame").Find("B").GetComponent<Image>();
        question = canvas.transform.Find("InGame").Find("Question").GetComponent<Image>();
        menuPanel = GameObject.Find("MenuPanel").GetComponent<Canvas>();
        retryButton = GameObject.Find("Retry").GetComponent<Button>();
        easyButton = GameObject.Find("Easy").GetComponent<Button>();
        mediumButton = GameObject.Find("Medium").GetComponent<Button>();
        hardButton = GameObject.Find("Hard").GetComponent<Button>();
        extremeButton = GameObject.Find("????").GetComponent<Button>();
        close = GameObject.Find("Close").GetComponent<Button>();
        scoreText = GameObject.Find("Score").GetComponent<TMP_Text>();


        questionText = question.transform.Find("Text").GetComponent<TMP_Text>();
        leftText = left.transform.Find("Text").GetComponent<TMP_Text>();
        rightText = right.transform.Find("Text").GetComponent<TMP_Text>();
        scoreText.text = "0/10";
        menuPanel.enabled = true;
        canvas.enabled = false;
        retryButton.enabled = false;
        // Đăng ký các sự kiện nút bấm
        easyButton.onClick.AddListener(() => SetDifficulty(1));
        mediumButton.onClick.AddListener(() => SetDifficulty(2));
        hardButton.onClick.AddListener(() => SetDifficulty(3));
        extremeButton.onClick.AddListener(() => SetDifficulty(4));
        retryButton.onClick.AddListener(() => ShowMenu());
        close.onClick.AddListener(() => Application.Quit());

        // Hiển thị menu ban đầu
        ShowMenu();
    }

    void Update()
    {
        if (canvas == null || n > 10 || isProcessing)
            return;

        // Lấy góc nghiêng (roll) từ ARFace
        Vector3 faceEulerAngles = this.transform.rotation.eulerAngles;
        float tiltAngle = NormalizeAngle(faceEulerAngles.z); // Độ nghiêng theo trục Z

        // Kiểm tra hướng nghiêng và điều chỉnh scale
        if (tiltAngle < 0) // Nghiêng sang trái
        {
            float scaleFactor = Mathf.Clamp(1 + Mathf.Abs(tiltAngle) * scaleMultiplier, minScale, maxScale);

            left.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
            right.transform.localScale = Vector3.one; // Đặt lại scale của right
        }
        else if (tiltAngle > 0) // Nghiêng sang phải
        {
            float scaleFactor = Mathf.Clamp(1 + tiltAngle * scaleMultiplier, minScale, maxScale);
            right.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
            left.transform.localScale = Vector3.one; // Đặt lại scale của left
        }

        // Khi nghiêng quá 30 độ mỗi bên thì là chọn
        if (Mathf.Abs(tiltAngle) >= 30)
        {
            if (tiltAngle < 0)
            {
                if (index == 0)
                {
                    trueAns++;
                }
                else
                {
                    wrongAns++;
                }
            }
            else if (tiltAngle > 0)
            {
                if (index == 1)
                {
                    trueAns++;
                }
                else
                {
                    wrongAns++;
                }
            }
            ++n;
            scoreText.text = trueAns.ToString() + "/10";
            if (n > 10)
            {
                canvas.enabled = false;
                retryButton.enabled = true;
            }
            else
            {
                StartCoroutine(WaitAndNextQuestion());
            }
        }
    }

    private IEnumerator WaitAndNextQuestion()
    {
        isProcessing = true;
        yield return new WaitForSeconds(2f); // Wait for 2 seconds
        GenQuiz();
        isProcessing = false;
    }

    private void GenQuiz()
    {
        Random random = new();
        int maxRange = (int)Math.Pow(10, level);

        // Tạo số ngẫu nhiên và tính tổng
        a = random.Next(0, maxRange);
        b = random.Next(0, maxRange);
        ans = a + b;

        // Tạo câu trả lời sai ngẫu nhiên
        int wrongAns;
        int randomSelection = random.Next(0, 2);
        if (randomSelection == 0)
        {
            wrongAns = ans - random.Next(1, 10);
        }
        else
        {
            wrongAns = ans + random.Next(1, 10);
        }

        // Tạo mảng câu trả lời
        string[] strings = { ans.ToString(), wrongAns.ToString() };

        // Trộn câu trả lời ngẫu nhiên
        index = random.Next(0, 2);

        questionText.text = "Câu hỏi " + n + ": " + a.ToString() + " + " + b.ToString() + " = ?";
        leftText.text = strings[index];
        rightText.text = strings[(index + 1) % 2];
    }

    // Hàm chuẩn hóa góc độ
    private float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;
        return angle;
    }

    // Hàm thay đổi mức độ
    private void SetDifficulty(int difficultyLevel)
    {
        level = difficultyLevel;

        // Ẩn menu và hiển thị game
        retryButton.enabled = false;
        menuPanel.enabled = false;
        canvas.enabled = true;
        scoreText.text = "0/10";
        n = 1;
        // Bắt đầu chơi game
        GenQuiz();
    }

    // Hiển thị menu ban đầu
    private void ShowMenu()
    {
        n = 1;
        canvas.enabled = false;
        retryButton.enabled = false;
        scoreText.text = "";
        menuPanel.enabled = true;
    }
}
