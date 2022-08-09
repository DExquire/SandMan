using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector2f = UnityEngine.Vector2;
using Vector2i = ClipperLib.IntPoint;

namespace MoreMountains.NiceVibrations
{
    public class RuntimeCircleClipper : MonoBehaviour, IClip
    {
        private struct TouchLineOverlapCheck
        {
            public float a;
            public float b;
            public float c;
            public float angle;
            public float dividend;

            public TouchLineOverlapCheck(Vector2 p1, Vector2 p2)
            {
                Vector2 d = p2 - p1;
                float m = d.magnitude;
                a = -d.y / m;
                b = d.x / m;
                c = -(a * p1.x + b * p1.y);
                angle = Mathf.Rad2Deg * Mathf.Atan2(-a, b);

                float da;
                if (d.x / d.y < 0f)
                    da = 45 + angle;
                else
                    da = 45 - angle;

                dividend = Mathf.Abs(1.0f / 1.4f * Mathf.Cos(Mathf.Deg2Rad * da));
            }

            public float GetDistance(Vector2 p)
            {
                return Mathf.Abs(a * p.x + b * p.y + c);
            }
        }

        public DestructibleTerrain terrain;

        public float diameter = 1.2f;

        private float radius = 1.2f;

        public float curRadius;

        public int segmentCount = 10;

        public float touchMoveDistance = 0.1f;

        private Vector2f currentTouchPoint;

        private Vector2f previousTouchPoint;

        private TouchPhase touchPhase;

        private TouchLineOverlapCheck touchLine;

        private List<Vector2i> vertices = new List<Vector2i>();

        private Camera mainCamera;

        private float cameraZPos;

        public bool CheckBlockOverlapping(Vector2f p, float size)
        {
            if (touchPhase == TouchPhase.Began)
            {
                float dx = Mathf.Abs(currentTouchPoint.x - p.x) - radius - size / 2;
                float dy = Mathf.Abs(currentTouchPoint.y - p.y) - radius - size / 2;
                return dx < 0f && dy < 0f;
            }
            else if (touchPhase == TouchPhase.Moved)
            {
                float distance = touchLine.GetDistance(p) - radius - size / touchLine.dividend;
                return distance < 0f;
            }
            else
                return false;
        }

        public ClipBounds GetBounds()
        {
            if (touchPhase == TouchPhase.Began)
            {
                return new ClipBounds
                {
                    lowerPoint = new Vector2f(currentTouchPoint.x - radius, currentTouchPoint.y - radius),
                    upperPoint = new Vector2f(currentTouchPoint.x + radius, currentTouchPoint.y + radius)
                };
            }
            else if (touchPhase == TouchPhase.Moved)
            {
                Vector2f upperPoint = currentTouchPoint;
                Vector2f lowerPoint = previousTouchPoint;
                if (previousTouchPoint.x > currentTouchPoint.x)
                {
                    upperPoint.x = previousTouchPoint.x;
                    lowerPoint.x = currentTouchPoint.x;
                }
                if (previousTouchPoint.y > currentTouchPoint.y)
                {
                    upperPoint.y = previousTouchPoint.y;
                    lowerPoint.y = currentTouchPoint.y;
                }

                return new ClipBounds
                {
                    lowerPoint = new Vector2f(lowerPoint.x - radius, lowerPoint.y - radius),
                    upperPoint = new Vector2f(upperPoint.x + radius, upperPoint.y + radius)
                };
            }
            else
                return new ClipBounds();
        }

        public List<Vector2i> GetVertices()
        {
            return vertices;
        }

        void Awake()
        {
            mainCamera = Camera.main;
            cameraZPos = mainCamera.transform.position.z;
            radius = diameter / 2f;
            StartCoroutine(TouchVFX());
        }

        void Shaking()
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector2(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y));
            RaycastHit2D hit;
            //if(Physics.Raycast(ray, out hit))
            {

            }


        }

        void SmoothDig()
        {

        }

        void Update()
        {
            UpdateTouch();
        }

        public ParticleSystem[] PSpool;
        IEnumerator TouchVFX()
        {
            float z = Camera.main.transform.position.z;
            int i = 0;
            while (true)
            {
                yield return null;
                if (TouchUtility.TouchCount > 0)
                {
                    Touch touch = TouchUtility.GetTouch(0);
                    if (touch.phase == TouchPhase.Moved)
                    {
                        PSpool[i].transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -z));
                        PSpool[i].Play();
                        MMVibrationManager.Haptic(HapticTypes.LightImpact, true, this);

                        i++;
                        yield return new WaitForSeconds(0.3f);
                        if (i >= PSpool.Length)
                        {
                            i = 0;
                        }
                    }
                }
            }
        }
        void UpdateTouch()
        {
            if (TouchUtility.TouchCount > 0)
            {
                Touch touch = TouchUtility.GetTouch(0);
                Vector2 touchPosition = touch.position;
                //StartCoroutine(beginTouch(touch, touchPosition));

                touchPhase = touch.phase;
                if (touch.phase == TouchPhase.Began)
                {
                    //MMVibrationManager.Haptic(HapticTypes.LightImpact, true, this);
                    Vector2 XOYPlaneLocation = mainCamera.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, -cameraZPos));
                    currentTouchPoint = XOYPlaneLocation - terrain.GetPositionOffset();
                    previousTouchPoint = currentTouchPoint;
                    curRadius = 0;
                    StartCoroutine(beginTouch(touch, touchPosition));

                    /*Vector2 XOYPlaneLocation = mainCamera.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, -cameraZPos));
                    currentTouchPoint = XOYPlaneLocation - terrain.GetPositionOffset();

                    BuildVertices(currentTouchPoint);

                    terrain.ExecuteClip(this);               

                    previousTouchPoint = currentTouchPoint;   */
                }
                else if (touch.phase == TouchPhase.Moved)
                {
                    Vector2 XOYPlaneLocation = mainCamera.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, -cameraZPos));
                    currentTouchPoint = XOYPlaneLocation - terrain.GetPositionOffset();

                    if ((currentTouchPoint - previousTouchPoint).sqrMagnitude <= touchMoveDistance * touchMoveDistance)
                        return;

                    BuildVertices(previousTouchPoint, currentTouchPoint);

                    terrain.ExecuteClip(this);

                    previousTouchPoint = currentTouchPoint;
                }
            }
        }

        private IEnumerator beginTouch(Touch touch, Vector2 touchPosition)
        {
            while (curRadius <= radius)
            {
                BuildVertices(currentTouchPoint);

                terrain.ExecuteClip(this);

                curRadius += radius / 5;



                yield return new WaitForSecondsRealtime(0.5f);
            }
        }

        void BuildVertices(Vector2 center)
        {
            vertices.Clear();
            for (int i = 0; i < segmentCount; i++)
            {
                float angle = Mathf.Deg2Rad * (-90f - 360f / segmentCount * i);

                Vector2 point = new Vector2(center.x + radius * Mathf.Cos(angle), center.y + radius * Mathf.Sin(angle));
                Vector2i point_i64 = point.ToVector2i();
                vertices.Add(point_i64);
            }
        }

        void BuildVertices(Vector2 begin, Vector2 end)
        {
            vertices.Clear();
            int halfSegmentCount = segmentCount / 2;
            touchLine = new TouchLineOverlapCheck(begin, end);

            for (int i = 0; i <= halfSegmentCount; i++)
            {
                float angle = Mathf.Deg2Rad * (touchLine.angle + 270f - (float)360f / segmentCount * i);
                Vector2 point = new Vector2(begin.x + radius * Mathf.Cos(angle), begin.y + radius * Mathf.Sin(angle));
                Vector2i point_i64 = point.ToVector2i();
                vertices.Add(point_i64);
            }

            for (int i = halfSegmentCount; i <= segmentCount; i++)
            {
                float angle = Mathf.Deg2Rad * (touchLine.angle + 270f - (float)360f / segmentCount * i);
                Vector2 point = new Vector2(end.x + radius * Mathf.Cos(angle), end.y + radius * Mathf.Sin(angle));
                Vector2i point_i64 = point.ToVector2i();
                vertices.Add(point_i64);
            }
        }
    }
}
