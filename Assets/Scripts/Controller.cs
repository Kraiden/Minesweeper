using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    
    public System.Action<Ray> OnTouchEvent;
    public System.Action<Ray> OnLongTouchEvent;

    public System.Action OnTouchDown;
    public System.Action OnTouchUp;

    private Generator board;

    [Tooltip("Disable this for phone builds. It doesn't play well on touch devices")]
    public bool enableDebugMouse = false;

    private float touchStartTime = 0;
    private float longTouchThreshold = 0.65f;

    private Vector2 startTouch;
    private Vector3 cameraStart;
    private float moveThreshold = 3000;

    private bool isEndgaming = false;
    private bool isGenerating = false;

    void Start(){
        board = GetComponent<Generator>();
        board.OnGameOver += HandleEndgame;
        board.OnGeneration += OnStartGen;
        board.OnNewGame += OnEndGen;

        longTouchThreshold = PlayerPrefs.GetFloat(PrefsConstants.SET_TOUCH_SENSITIVITY, 0.65f);
        moveThreshold = PlayerPrefs.GetFloat(PrefsConstants.SET_MOVE_SENSITIVITY, 3000);
    }

    void Update()
    {
        HandleTouchInput();
        if(enableDebugMouse){
            HandleMouseInput();
        }
    }
    
    private bool blockPan = false;
    private void HandleTouchInput(){
        if(!isEndgaming){
            if(Input.touchCount == 1){
                Touch touch = Input.GetTouch(0);

                if(touch.phase == TouchPhase.Began){
                    if(OnTouchDown != null) OnTouchDown();
                    touchStartTime = Time.time;
                    startTouch = touch.position;
                    cameraStart = Camera.main.transform.position;

                    //Debug.Log("touchStart");
                } else if (touch.phase == TouchPhase.Moved){
                    float sqrDistance = (startTouch - touch.position).sqrMagnitude;
                    if((sqrDistance > moveThreshold || touchStartTime == 0) && !blockPan){
                        //Debug.Log("touchMove: " + sqrDistance);
                        HandleDrag(touch);
                        touchStartTime = 0;
                    }
                } else if(touch.phase == TouchPhase.Ended){
                    blockPan = false;
                    if(OnTouchUp != null) OnTouchUp();
                    float touchEndTime = Time.time;

                    if(touchStartTime != 0){
                        float touchTime = touchEndTime - touchStartTime;
                        //Debug.Log("Touch Time: " + touchTime);

                        if(touchTime < longTouchThreshold){
                            if(OnTouchEvent != null && !isGenerating){
                                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                                OnTouchEvent(ray);
                            }
                        } 
                        touchStartTime = 0;
                    }
                } else {
                    if(touchStartTime != 0){
                        float touchTime = Time.time - touchStartTime;

                        if(touchTime >= longTouchThreshold){
                            touchStartTime = 0;
                            if(OnLongTouchEvent != null && !isGenerating){
                                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                                OnLongTouchEvent(ray);
                            }
                            blockPan = true;
                        }
                    }
                }
            } else if(Input.touchCount > 1){
                //Debug.Log("Attempt zoom");
                blockPan = true;
                touchStartTime = 0;
                HandleZoom();
            } else {
                ClearZoom();
            }
        }
    }

    private void HandleMouseInput(){
        Vector3 _mp = Input.mousePosition;
        Vector2 mousePosition = new Vector2(_mp.x, _mp.y);

        if(Input.GetMouseButton(0)){
            if(OnTouchEvent != null && !isGenerating){
                Ray ray = Camera.main.ScreenPointToRay(mousePosition);
                OnTouchEvent(ray);
            }
        }

        if(Input.GetMouseButton(1)){
            if(Time.time - touchStartTime > 1){
                touchStartTime = Time.time;
                if(OnTouchEvent != null && !isGenerating){
                    Ray ray = Camera.main.ScreenPointToRay(mousePosition);
                    OnLongTouchEvent(ray);
                }
            }
        }
    }

    private void HandleDrag(Touch touch){
        Vector3 positionMin = board.MinPosition();
        Vector3 positionMax = board.MaxPosition();
        // Determine how much to move the camera
        Vector3 startWorld = Camera.main.ScreenToWorldPoint(startTouch);
        Vector3 touchWorld = Camera.main.ScreenToWorldPoint(touch.position);
        Vector3 offset =  startWorld - touchWorld;

        Vector3 newPostion = cameraStart + offset;
        Vector3 move = new Vector3(
            Mathf.Clamp(newPostion.x, positionMin.x, positionMax.x),
            Mathf.Clamp(newPostion.y, positionMin.y, positionMax.y),
            newPostion.z
        ); 


        Camera.main.transform.position = move;
    }

    private Touch? zoomTouch1Start;
    private Touch? zoomTouch2Start;
    private float startZoom = 0;

    private void HandleZoom(){
        if(zoomTouch1Start == null || zoomTouch2Start == null){
            zoomTouch1Start = Input.GetTouch(0);
            zoomTouch2Start = Input.GetTouch(1);
            startZoom = Camera.main.orthographicSize;
        } else {
            Touch? zoomTouch1Cur = Input.GetTouch(0);
            Touch? zoomTouch2Cur = Input.GetTouch(1);

            if(zoomTouch1Cur != null && zoomTouch2Cur != null){
                Zoom(zoomTouch1Start.Value, zoomTouch2Start.Value, zoomTouch1Cur.Value, zoomTouch2Cur.Value);
            }
        }
    }

    private void Zoom(Touch zoomStart1, Touch zoomStart2, Touch zoomCur1, Touch zoomCur2){
        float oldDistance = Vector2.Distance(zoomStart1.position, zoomStart2.position);
        float newDistance = Vector2.Distance(zoomCur1.position, zoomCur2.position);

        float offset = (newDistance - oldDistance) / 80f;
        float newZoom = Mathf.Clamp(startZoom - offset, 5f, 30f);
        //Debug.Log("Zoom: " + newZoom);
        Camera.main.orthographicSize = newZoom;
    }

    private void ClearZoom(){
        zoomTouch1Start = null;
        zoomTouch2Start = null;
    }

    private void HandleEndgame(bool isWin){
        float endZoom = board.startCamera.zoom;
        Vector3 endPosition = board.startCamera.position;

        float curZoom = Camera.main.orthographicSize;
        Vector3 curPosition = Camera.main.transform.position;

        float duration = 1f;
        
        isEndgaming = true;
        StartCoroutine(ResetCamera(curPosition, endPosition, curZoom, endZoom, duration));
    }

    private IEnumerator ResetCamera(Vector3 startPos, Vector3 endPos, float startZoom, float endZoom, float duration){
        float pct = 0;

        while(pct < 1){
            pct += Time.deltaTime * 1 / duration;
            float interp = -Mathf.Pow(pct, 2) + 2 * pct;

            Camera.main.transform.position = Vector3.Lerp(startPos, endPos, pct);
            Camera.main.orthographicSize = Mathf.Lerp(startZoom, endZoom, pct);

            yield return null;
        }

        Camera.main.transform.position = endPos;
        Camera.main.orthographicSize = endZoom;

        isEndgaming = false;
    }

    private void OnStartGen(){
        isGenerating = true;
    }

    private void OnEndGen(){
        isGenerating = false;
    }
}
