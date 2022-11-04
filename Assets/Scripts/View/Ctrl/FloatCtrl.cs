using UnityEngine;

public class FloatCtrl : MonoBehaviour
{
    private Vector3 _srcPos;
    private Vector3 _dstPos;
    private Vector3 _posOffset = new Vector3(0, 0.2f, 0);
    private float _elapsedTime = 0;
    private float _duration = 2f;
    private bool _forward = true;

    private void Awake()
    {
        _srcPos = transform.localPosition;
        _dstPos = _srcPos + _posOffset;
    }

    private void Update()
    {
        if (_duration <= 0)
            return;

        _elapsedTime += Time.deltaTime;
        if (_elapsedTime > _duration)
        {
            _elapsedTime -= _duration;
            _forward = !_forward;
        }

        var t = _elapsedTime / _duration;
        if (_forward)
            transform.localPosition = Vector3.Lerp(_srcPos, _dstPos, t);
        else
            transform.localPosition = Vector3.Lerp(_dstPos, _srcPos, t);
    }
}
