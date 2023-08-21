using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebcamController : MonoBehaviour
{
    private int current_gc_count = 0;

    //当前相机索引
    private int index = 0;

    //当前运行的相机
    [HideInInspector] public WebCamTexture currentWebCam;
    private int webcamWidth; //相机需要的宽度
    private int webcamHeight;

    [HideInInspector] public int actualTexWidth;
    [HideInInspector] public int actualTexHeight;

    Texture2D tex; //获取到的图像texture2d

    void Start()
    {
        ChangeRawImageSize();
        BindEvents();
    }

    void BindEvents()
    {
        EventManager.Instance.AddListener("开启摄像头", OnStartBtnClick);
        EventManager.Instance.AddListener("拍照", TakePhoto);
    }

    // 调整显示相机镜头的UI组件大小
    void ChangeRawImageSize(float requestWidth = 0, float requestHeight = 0)
    {
        if (requestWidth <= 0 && requestHeight <= 0)
        {
            requestWidth = Screen.width;
            requestHeight = Screen.height;
        }

        webcamWidth = Mathf.FloorToInt(requestWidth);
        webcamHeight = Mathf.FloorToInt(requestHeight);

        float width = requestWidth;
        float height = requestHeight;

        //跟屏幕长宽做对比
        float uiWidth = width;
        float uiHeight = height;

        float wantedRatio = 1.0f * requestWidth / requestHeight;

        uiWidth = Screen.width;
        uiHeight = (int)(uiWidth / wantedRatio * 1.0f);
        if (uiHeight > Screen.height)
        {
            uiHeight = Screen.height;
            uiWidth = (int)(uiHeight * wantedRatio * 1.0f);
        }

        UISize size = new UISize();
        size.width = uiWidth;
        size.height = uiHeight;
        EventManager.Instance.DispatchEvent("设置相机画面UI尺寸", JsonUtility.ToJson(size));

        UnityEngine.Debug.Log("ui width: " + uiWidth + "   height: " + uiHeight);
    }

    public void OnStartBtnClick(string event_name = null, object udata = null)
    {
        StartCoroutine(Call());
    }

    public IEnumerator Call()
    {
        // 请求权限
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

        if (Application.HasUserAuthorization(UserAuthorization.WebCam) && WebCamTexture.devices.Length > 0)
        {
            // 创建相机贴图
            currentWebCam = new WebCamTexture(WebCamTexture.devices[index].name, webcamWidth, webcamHeight, 60);
            currentWebCam.Play();

            //新加上去的!!获取实际上可以获取的画面像素
            UnityEngine.Debug.Log("------想要的像素-------  " + webcamWidth + "_" + webcamHeight);
            UnityEngine.Debug.Log("------相机的像素-------  " + currentWebCam.width + "_" + currentWebCam.height);
            GetActualResolution(out actualTexWidth, out actualTexHeight);
            ChangeRawImageSize(actualTexWidth, actualTexHeight);
            tex = new Texture2D(actualTexWidth, actualTexHeight);

            float angle = -currentWebCam.videoRotationAngle;
            EventManager.Instance.DispatchEvent("设置相机画面UI角度", angle);

            EventManager.Instance.DispatchEvent("摄像头启动成功", null);
        }

        UnityEngine.Debug.Log("------获取相机的像素-------  " + actualTexWidth + "_" + actualTexHeight);
    }

    private void Update()
    {
        if (currentWebCam == null || !currentWebCam.isPlaying) return;
        if (tex == null) return;
        SetWebcamPicToUI();
    }

    //获取实际展示到画面的像素
    void GetActualResolution(out int width, out int height)
    {
        float wantedRatio1 = 1.0f * webcamWidth / webcamHeight;

        width = currentWebCam.width;
        height = (int)(width / wantedRatio1 * 1.0f);
        if (height > currentWebCam.height)
        {
            height = currentWebCam.height;
            width = (int)(height * wantedRatio1 * 1.0f);
        }

        UnityEngine.Debug.Log("width: " + width + "   height: " + height);
    }

    //设置相机画面到UI上
    void SetWebcamPicToUI()
    {
        Color[] pix = currentWebCam.GetPixels(0, 0, actualTexWidth, actualTexHeight);
        tex.SetPixels(pix);
        tex.Apply();

        EventManager.Instance.DispatchEvent("设置相机画面到UI", tex);
    }

    // 获取截图
    public void TakePhoto(string event_name = null, object udata = null)
    {
        Color[] pix = tex.GetPixels();
        Texture2D destTex = new Texture2D(tex.width, tex.height);
        destTex.SetPixels(pix);
        destTex.Apply();

        EventManager.Instance.DispatchEvent("截图", destTex);

        CheckShouldCollectGC();
    }

    void CheckShouldCollectGC()
    {
        current_gc_count++;
        if (current_gc_count > 100)
        {
            current_gc_count = 0;
            System.GC.Collect();
        }
    }

    //停止拍照
    void StopCamera(string event_name = null, object udata = null)
    {
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            if (currentWebCam != null && currentWebCam.isPlaying)
                currentWebCam.Stop();
        }
    }

    //切换前后摄像头
    /*public void SwitchCamera(string event_name = null, object udata = null)
    {
        if (WebCamTexture.devices.Length < 1)
            return;

        if (currentWebCam != null)
            currentWebCam.Stop();

        index++;
        index = index % WebCamTexture.devices.Length;

        // 创建相机贴图
        currentWebCam = new WebCamTexture(WebCamTexture.devices[index].name, webcamWidth, webcamHeight, 60);
        videoImg.texture = currentWebCam;
        currentWebCam.Play();

        float angle = -currentWebCam.videoRotationAngle;
        videoImg.rectTransform.localEulerAngles = new Vector3(0, 0, angle);
        screenShot.rectTransform.localEulerAngles = new Vector3(0, 0, angle);
    }*/
}
