using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    float wantedWidth = 1;
    float wantedHeight = 1;

    [SerializeField] RectTransform videoImg;
    [SerializeField] RectTransform screenShot;

    [SerializeField] GameObject box;
    [SerializeField] RawImage boxedScreenshot;

    [SerializeField] Button screenshotBtn;
    [SerializeField] Button clearBtn;

    [SerializeField] InputField input;
    [SerializeField] Button confirmBtn;

    private void Start()
    {
        InitUI();
        BindEvents();
    }

    void InitUI()
    {
        box.SetActive(false);
        boxedScreenshot.gameObject.SetActive(false);

        confirmBtn.onClick.AddListener(() =>
        {
            EventManager.Instance.DispatchEvent("开启摄像头", null);
        });
        screenshotBtn.onClick.AddListener(() =>
        {
            EventManager.Instance.DispatchEvent("拍照", null);
        });
        clearBtn.onClick.AddListener(() =>
        {
            screenShot.GetComponent<RawImage>().texture = null;
            screenShot.gameObject.SetActive(false);
            boxedScreenshot.gameObject.SetActive(false);
        });

        input.onEndEdit.AddListener((value) =>
        {
            string[] arr = value.Split(":");
            if (arr.Length == 2)
            {
                int num1;
                int num2;
                if (int.TryParse(arr[0], out num1) && int.TryParse(arr[1], out num2))
                {
                    wantedWidth = num1;
                    wantedHeight = num2;
                    OnVideoBegan();
                }
            }
        });
    }

    void BindEvents()
    {
        EventManager.Instance.AddListener("摄像头启动成功", OnVideoBegan);
        EventManager.Instance.AddListener("截图", OnScreenshotBegan);
        EventManager.Instance.AddListener("设置相机画面UI尺寸", SetVideoImageSize);
        EventManager.Instance.AddListener("设置相机画面UI角度", SetVideoImageZAngle);
        EventManager.Instance.AddListener("设置相机画面到UI", SetVideoImage);
    }

    void SetVideoImageSize(string event_name = null, object udata = null)
    {
        UISize size = JsonUtility.FromJson((string)udata, typeof(UISize)) as UISize;

        videoImg.sizeDelta = new Vector2(size.width, size.height);
        videoImg.anchoredPosition = Vector2.zero;
        screenShot.sizeDelta = new Vector2(size.width, size.height);
        screenShot.anchoredPosition = Vector2.zero;

    }

    void SetVideoImageZAngle(string event_name = null, object udata = null)
    {
        float angle = (float)udata;
        videoImg.localEulerAngles = new Vector3(0, 0, angle);
        screenShot.localEulerAngles = new Vector3(0, 0, angle);
    }

    void SetVideoImage(string event_name = null, object udata = null)
    {
        Texture2D tex = (Texture2D)udata;
        videoImg.GetComponent<RawImage>().texture = tex;
    }

    //这里对比的是ui的像素
    public void OnVideoBegan(string event_name = null, object udata = null)
    {
        float wantedRatio = 1.0f * wantedWidth / wantedHeight;
        float uiWidth = videoImg.sizeDelta.x;
        float uiHeight = (int)(uiWidth / wantedRatio * 1.0f);
        if (uiHeight > videoImg.sizeDelta.y)
        {
            uiHeight = videoImg.sizeDelta.y;
            uiWidth = (int)(uiHeight * wantedRatio * 1.0f);
        }

        box.GetComponent<RectTransform>().sizeDelta = new Vector2(uiWidth, uiHeight);
        box.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        box.SetActive(true);
    }

    //这里取的是传过来的画面的像素
    public void OnScreenshotBegan(string event_name = null, object udata = null)
    {
        Texture2D tex = (Texture2D)udata;

        screenShot.GetComponent<RawImage>().texture = tex;
        screenShot.gameObject.SetActive(true);

        Vector2 texSize = new Vector2(tex.width, tex.height);
        float wantedRatio = 1.0f * wantedWidth / wantedHeight;
        float cuttedTexWidth = texSize.x;
        float cuttedTexHeight = (int)(cuttedTexWidth / wantedRatio * 1.0f);
        if (cuttedTexHeight > texSize.y)
        {
            cuttedTexHeight = texSize.y;
            cuttedTexWidth = (int)(cuttedTexHeight * wantedRatio * 1.0f);
        }

        UnityEngine.Debug.Log("原来的截图尺寸 ++++++++++  " + tex.width + "_" + tex.height);
        UnityEngine.Debug.Log("裁剪后的截图尺寸 ++++++++++  " + cuttedTexWidth + "_" + cuttedTexHeight);

        Texture2D newTex = new Texture2D((int)cuttedTexWidth, (int)cuttedTexHeight);
        int x = (int)(tex.width * 0.5f - cuttedTexWidth * 0.5f); //因为取的图像，中心点在图片中心点
        int y = (int)(tex.height * 0.5f - cuttedTexHeight * 0.5f); //因为取的图像，中心点在图片中心点
        int blockWidth = (int)cuttedTexWidth;
        int blockHeight = (int)cuttedTexHeight;

        Color[] pixels = tex.GetPixels(x, y, blockWidth, blockHeight);
        newTex.SetPixels(pixels);
        newTex.Apply();

        boxedScreenshot.texture = newTex;
        Vector2 size = box.GetComponent<RectTransform>().sizeDelta;
        boxedScreenshot.GetComponent<RectTransform>().sizeDelta = size;
        Vector2 scale = boxedScreenshot.GetComponent<RectTransform>().localScale;
        boxedScreenshot.GetComponent<RectTransform>().anchoredPosition = new Vector2(size.x / 2f * scale.x, size.y / -2f * scale.y);
        boxedScreenshot.gameObject.SetActive(true);
    }

}
