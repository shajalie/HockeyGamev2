using UnityEngine;

namespace HockeyGame.Core
{
    public class Billboard : MonoBehaviour
    {
        public enum BillboardMode
        {
            LookAtCamera,
            CameraForward,
            CameraForwardInverted
        }

        [Header("Settings")]
        [SerializeField] private BillboardMode _mode = BillboardMode.CameraForward;
        [SerializeField] private bool _lockYAxis = true;
        [SerializeField] private bool _useMainCamera = true;
        [SerializeField] private Camera _customCamera;

        [Header("Offset")]
        [SerializeField] private Vector3 _rotationOffset = Vector3.zero;

        private Transform _cameraTransform;

        private void Start()
        {
            UpdateCameraReference();
        }

        private void OnEnable()
        {
            UpdateCameraReference();
        }

        private void UpdateCameraReference()
        {
            if (_useMainCamera)
            {
                if (Camera.main != null)
                {
                    _cameraTransform = Camera.main.transform;
                }
            }
            else if (_customCamera != null)
            {
                _cameraTransform = _customCamera.transform;
            }
        }

        private void LateUpdate()
        {
            if (_cameraTransform == null)
            {
                UpdateCameraReference();
                if (_cameraTransform == null) return;
            }

            Quaternion targetRotation = Quaternion.identity;

            switch (_mode)
            {
                case BillboardMode.LookAtCamera:
                    Vector3 lookDirection = _cameraTransform.position - transform.position;
                    if (_lockYAxis)
                    {
                        lookDirection.y = 0;
                    }
                    if (lookDirection != Vector3.zero)
                    {
                        targetRotation = Quaternion.LookRotation(-lookDirection);
                    }
                    break;

                case BillboardMode.CameraForward:
                    if (_lockYAxis)
                    {
                        Vector3 forward = new Vector3(
                            _cameraTransform.forward.x,
                            0,
                            _cameraTransform.forward.z
                        ).normalized;

                        if (forward != Vector3.zero)
                        {
                            targetRotation = Quaternion.LookRotation(forward);
                        }
                    }
                    else
                    {
                        targetRotation = Quaternion.LookRotation(_cameraTransform.forward);
                    }
                    break;

                case BillboardMode.CameraForwardInverted:
                    if (_lockYAxis)
                    {
                        Vector3 forward = new Vector3(
                            -_cameraTransform.forward.x,
                            0,
                            -_cameraTransform.forward.z
                        ).normalized;

                        if (forward != Vector3.zero)
                        {
                            targetRotation = Quaternion.LookRotation(forward);
                        }
                    }
                    else
                    {
                        targetRotation = Quaternion.LookRotation(-_cameraTransform.forward);
                    }
                    break;
            }

            transform.rotation = targetRotation * Quaternion.Euler(_rotationOffset);
        }

        public void SetMode(BillboardMode mode)
        {
            _mode = mode;
        }

        public void SetCamera(Camera cam)
        {
            _customCamera = cam;
            _useMainCamera = false;
            _cameraTransform = cam.transform;
        }
    }
}
