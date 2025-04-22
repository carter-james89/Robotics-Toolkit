using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

using UnityEngine.Rendering;
using static ARPoolTrainer.ARPoolTable;

namespace ARPoolTrainer
{
    public class ARPoolTable : MonoBehaviour
    {

        [SerializeField] private Transform _pocketSW;
        [SerializeField] private Transform _pocketNW;
        [SerializeField] private Transform _pocketN;
        [SerializeField] private Transform _pocketNE;
        [SerializeField] private Transform _pocketSE;
        [SerializeField] private Transform _pocketS;

        [SerializeField] private Transform _bankPoint;

        public enum BankRail
        {
            North,
            South,
            East,
            West,
            None
        }
       [SerializeField] private BankRail _bankRail = BankRail.None;
        public enum TargetPocket
        {
            SW,
            NW,
            N,
            NE,
            SE,
            S
        }
       [ SerializeField] TargetPocket _targetPocket;

        public TargetPocket GetTargetPocket()
        {
            return _targetPocket;
        }

        [SerializeField] private Transform _previewBall;

        [SerializeField] private ARPoolBall _cueBall;
        [SerializeField] private ARPoolBall _targetBall;
        // Start is called once before the first execution of Update after the MonoBehaviour is created

void OnEnable()
    => RenderPipelineManager.beginCameraRendering += OnCameraRender;
    void OnDisable()
        => RenderPipelineManager.beginCameraRendering -= OnCameraRender;

    void OnCameraRender(ScriptableRenderContext ctx, Camera cam)
    {
            ClampBallTransform(_cueBall);
            ClampBallTransform(_targetBall);
        }

    //void LateUpdate()
    //    {
    //        ClampBallTransform(_cueBall);
    //        ClampBallTransform(_targetBall);
    //    }

        private void ClampBallTransform(ARPoolBall ball)
        {
            var grab = ball.GetComponent<XRGrabInteractable>();
            if (grab != null && grab.isSelected)
            {
                // Lock Y position
                var pos = ball.transform.localPosition;
                pos.y = 0f;
                ball.transform.localPosition = pos;

                // Zero rotation (optional: only zero certain axes if needed)
                ball.transform.localRotation = Quaternion.identity;
            }
        }


        // Update is called once per frame
        void Update()
        {
            Transform targetPocket = _pocketN;
            switch (_targetPocket)
            {
                case TargetPocket.SW:
                    targetPocket = _pocketSW;
                    break;
                case TargetPocket.NW:
                    targetPocket = _pocketNW;
                    break;
                case TargetPocket.N:
                    targetPocket = _pocketN;
                    break;
                case TargetPocket.NE:
                    targetPocket = _pocketNE;
                    break;
                case TargetPocket.SE:
                    targetPocket = _pocketSE;
                    break;
                case TargetPocket.S:
                    targetPocket = _pocketS;
                    break;
                default:
                    break;
            }

            if(_bankRail == BankRail.None)
            {
                Vector3 direction = (_targetBall.transform.position - targetPocket.position).normalized;
                float ballRadius = 0.05715f;// _cueBall.transform.localScale.x / 2f;
                var distance = direction.magnitude;

                _previewBall.position = _targetBall.transform.position + direction * (ballRadius);

                var previewRenderer = _previewBall.GetComponent<LineRenderer>();
                previewRenderer.SetPosition(0, _previewBall.transform.position);
                previewRenderer.SetPosition(1, targetPocket.transform.position);
                _bankPoint.gameObject.SetActive(false);
            }
            else
            {
                _bankPoint.gameObject.SetActive(true);
                PositionBankPoint(targetPocket.position);

                // … then you can continue to compute your reflected shot, preview ball, etc. …

                Vector3 direction = (_targetBall.transform.position - _bankPoint.position).normalized;
                float ballRadius = 0.05715f;// _cueBall.transform.localScale.x / 2f;
                var distance = direction.magnitude;

                _previewBall.position = _targetBall.transform.position + direction * (ballRadius);

                var previewRenderer = _previewBall.GetComponent<LineRenderer>();
                previewRenderer.SetPosition(0, _previewBall.transform.position);
                previewRenderer.SetPosition(1, _bankPoint.transform.position);

                var bankRenderer = _bankPoint.GetComponent<LineRenderer>();
                bankRenderer.SetPosition(0, _bankPoint.transform.position);
                bankRenderer.SetPosition(1, targetPocket.transform.position);
            }


            var cueRenderer = _cueBall.GetComponent<LineRenderer>();
            cueRenderer.SetPosition(0, _cueBall.transform.position);
            cueRenderer.SetPosition(1, _previewBall.transform.position);

       
        }
        private void PositionBankPoint(Vector3 targetPocket)
        {
            // 1. pick the two endpoints of the selected rail in 3D
            Vector3 railStart3D = Vector3.zero;
            Vector3 railEnd3D = Vector3.zero;
            switch (_bankRail)
            {
                case BankRail.North:
                    railStart3D = _pocketNW.position;
                    railEnd3D = _pocketNE.position;
                    break;
                case BankRail.South:
                    railStart3D = _pocketSW.position;
                    railEnd3D = _pocketSE.position;
                    break;
                case BankRail.East:
                    railStart3D = _pocketNE.position;
                    railEnd3D = _pocketSE.position;
                    break;
                case BankRail.West:
                    railStart3D = _pocketNW.position;
                    railEnd3D = _pocketSW.position;
                    break;
                default:
                    // no rail selected: bail out
                    return;
            }

            // 2. flatten everything into XZ (2D)
            Vector2 railStart = new Vector2(railStart3D.x, railStart3D.z);
            Vector2 railEnd = new Vector2(railEnd3D.x, railEnd3D.z);
            Vector2 railDir = (railEnd - railStart).normalized;
            Vector2 ballPos = new Vector2(_targetBall.transform.position.x,
                                             _targetBall.transform.position.z);
            Vector2 pocketPos = new Vector2(targetPocket.x, targetPocket.z);

            // 3. reflect the pocket across the infinite rail line to get the "virtual" pocket
            float projLen = Vector2.Dot(pocketPos - railStart, railDir);
            Vector2 projPt = railStart + railDir * projLen;
            Vector2 mirroredPocket = projPt * 2f - pocketPos;

            // 4. intersect (ballPos -> mirroredPocket) with the rail line
            Vector2 v = mirroredPocket - ballPos;
            Vector2 c = railStart - ballPos;
            // use cross(v, railDir) for the denominator
            float denom = v.x * railDir.y - v.y * railDir.x;
            if (Mathf.Abs(denom) < 1e-6f)
            {
                // lines nearly parallel: fallback to midpoint
                Vector3 mid = (_targetBall.transform.position + targetPocket) * 0.5f;
                _bankPoint.position = new Vector3(mid.x, _bankPoint.position.y, mid.z);
                return;
            }

            float numer = c.x * v.y - c.y * v.x;
            float tRail = numer / denom;

            // clamp to the actual rail segment length
            float railLength = Vector2.Distance(railStart, railEnd);
            tRail = Mathf.Clamp(tRail, 0f, railLength);

            Vector2 hit2D = railStart + railDir * tRail;

            // 5. write back into 3D (preserving bankPoint's Y)
            _bankPoint.position = new Vector3(hit2D.x,
                                              _bankPoint.position.y,
                                              hit2D.y);
        }

        //private void PositionBankPoint(Vector3 targetPocket)
        //{
        //    // 1. midpoint between ball and pocket
        //    Vector3 midPoint = (_targetBall.transform.position + targetPocket) * 0.5f;

        //    // 2. pick the two endpoints of the selected rail
        //    Vector3 railStart = Vector3.zero, railEnd = Vector3.zero;
        //    switch (_bankRail)
        //    {
        //        case BankRail.North:
        //            railStart = _pocketNW.position;
        //            railEnd = _pocketNE.position;
        //            break;
        //        case BankRail.South:
        //            railStart = _pocketSW.position;
        //            railEnd = _pocketSE.position;
        //            break;
        //        case BankRail.East:
        //            railStart = _pocketNE.position;
        //            railEnd = _pocketSE.position;
        //            break;
        //        case BankRail.West:
        //            railStart = _pocketNW.position;
        //            railEnd = _pocketSW.position;
        //            break;
        //    }

        //    // 3. project midpoint onto the infinite rail line
        //    Vector3 railDir = (railEnd - railStart).normalized;
        //    float t = Vector3.Dot(midPoint - railStart, railDir);
        //    // (optional) clamp t to [0, railLength] if you want to stay within the segment:
        //    // float railLength = Vector3.Distance(railStart, railEnd);
        //    // t = Mathf.Clamp(t, 0f, railLength);

        //    Vector3 bankPos = railStart + railDir * t;

        //    // 4. move your bankPoint there
        //    _bankPoint.position = bankPos;
        //}

        internal void SetTargetPocket(TargetPocket targetPocket)
        {
            _targetPocket = targetPocket;
        }

        internal void SetBankRail(BankRail bankRail)
        {
          _bankRail = bankRail;
        }

        internal BankRail GetBankRail()
        {
            return _bankRail;
        }
    }

}