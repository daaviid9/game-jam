using UnityEngine;

public class HandController : MonoBehaviour
{
    [Header("Positions (Local to Parent)")]
    public Vector3 hiddenPosition = new Vector3(0f, -1.5f, 0f);
    public Vector3 visiblePosition = new Vector3(0f, -0.062f, 0f);
    public Vector3 hiddenRotation = new Vector3(-30f, 0f, 0f);
    public Vector3 visibleRotation = new Vector3(0f, 0f, 0f);
    
    [Header("Animation Speed")]
    public float raiseSpeed = 5f;
    public float lowerSpeed = 4f;
    public AnimationCurve raiseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Idle Sway (subtle hand movement)")]
    public bool enableSway = true;
    public float swayAmount = 0.01f;
    public float swaySpeed = 2f;
    
    private bool isRaised = false;
    private float currentT = 0f;
    private Vector3 swayOffset;
    
    public void SetRaised(bool raised)
    {
        isRaised = raised;
    }
    
    void Start()
    {
        transform.localPosition = hiddenPosition;
        transform.localRotation = Quaternion.Euler(hiddenRotation);
    }
    
    void Update()
    {
        float targetT = isRaised ? 1f : 0f;
        float speed = isRaised ? raiseSpeed : lowerSpeed;
        currentT = Mathf.MoveTowards(currentT, targetT, Time.deltaTime * speed);
        
        float easedT = raiseCurve.Evaluate(currentT);
        
        Vector3 pos = Vector3.Lerp(hiddenPosition, visiblePosition, easedT);
        Vector3 rot = Vector3.Lerp(hiddenRotation, visibleRotation, easedT);
        
        if (enableSway && easedT > 0.5f)
        {
            float swayStrength = (easedT - 0.5f) * 2f;
            swayOffset.x = Mathf.Sin(Time.time * swaySpeed) * swayAmount * swayStrength;
            swayOffset.y = Mathf.Cos(Time.time * swaySpeed * 0.7f) * swayAmount * 0.5f * swayStrength;
            pos += swayOffset;
        }
        
        transform.localPosition = pos;
        transform.localRotation = Quaternion.Euler(rot);
    }
}
