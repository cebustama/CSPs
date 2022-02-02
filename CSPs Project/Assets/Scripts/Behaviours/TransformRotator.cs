using UnityEngine;

// TODO: ScriptableObject settings
public class TransformRotator : MonoBehaviour
{
    [SerializeField]
    private float rotateSpeed = 10f;

    [SerializeField]
    private Vector3 rotateDirection;

    [SerializeField]
    private AnimationCurve velocityCurve;

    private Quaternion initialRotation;
    private Vector3 rotateEulerOffset;

    private void Awake()
    {
        initialRotation = transform.localRotation;
        rotateEulerOffset = Vector3.zero;
    }

    private void Update()
    {
        if (rotateSpeed == 0) return;
        rotateEulerOffset += rotateDirection * rotateSpeed * velocityCurve.Evaluate(Time.time) * Time.deltaTime;
    }

    private void LateUpdate()
    {
        if (rotateSpeed == 0) return;
        transform.localRotation = Quaternion.Euler(initialRotation.eulerAngles + rotateEulerOffset);
    }
}
