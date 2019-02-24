using Boo.Lang;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.UI;

namespace VARP.KochFractals
{
    public class KochTrail : KochGenerator
    {
        public class TrailItem
        {
            public GameObject go;
            public TrailRenderer trail;
            public int currentTargetNum;
            public Vector3 targetPosition;
            public Color emissionColor;
        }

        [HideInInspector] 
        public List<TrailItem> trails;

        [Header("Trail Properties")] 
        public GameObject trailPrefab;

        public AnimationCurve trailWidthCurve;
        [Range(0, 8)] 
        public int trailEndCapVertices;

        public Material trailMaterial;
        public Gradient trailColor;

        [Header("Audio")] 
        public AudioPeer audioPeer;

        public int[] audioBand = new int[8];

        public float lerpPosSpeed;
        public float distanceSnap;
        public Vector3 speedMinMax = Vector3.one;
        
        private void Start()
        {
            trails = new List<TrailItem>();

            for (var i = 0; i < initiatorPointsAmount; i++)
            {
                var item = new TrailItem();
                item.go = Instantiate(trailPrefab, transform.position, Quaternion.identity);
                item.trail = item.go.GetComponent<TrailRenderer>();
                item.trail.material = new Material(trailMaterial);
                item.emissionColor = trailColor.Evaluate(i * (1f / initiatorPointsAmount));
                item.trail.numCapVertices = trailEndCapVertices;
                item.trail.widthCurve = trailWidthCurve;
                
                Vector3 instantiatePosition;

                if (isDirty)
                {
                    int step;
                    if (enableBezier)
                    {
                        step = bezierPositions.Length / initiatorPointsAmount;
                        instantiatePosition = bezierPositions[i * step];
                        item.currentTargetNum = i * step + 1;
                        item.targetPosition =
                            bezierPositions[item.currentTargetNum];
                    }
                    else
                    {
                        step = targetPositions.Length / initiatorPointsAmount;
                        instantiatePosition = targetPositions[i * step];
                        item.currentTargetNum = i * step + 1;
                        item.targetPosition =
                            targetPositions[item.currentTargetNum];  
                    }
                }
                else
                {
                    instantiatePosition = sourcePositions[i];
                    item.currentTargetNum = i + 1;
                    item.targetPosition = sourcePositions[item.currentTargetNum];
                }

                item.go.transform.localPosition = instantiatePosition;
                trails.Add(item);
            }
        }

        void Update()
        {
            Movement();
        }

        void Movement()
        {
            lerpPosSpeed = Mathf.Lerp(speedMinMax.x, speedMinMax.y, audioPeer.amplitude); 
            for (var i = 0; i < trails.Count; i++)
            {
                var item = trails[i];
                var trailTransform = item.go.transform;
                trailTransform.localPosition = Vector3.MoveTowards(trailTransform.localPosition, item.targetPosition, Time.deltaTime * lerpPosSpeed);
                distanceSnap = Vector3.Distance(trailTransform.localPosition, item.targetPosition);
                if (distanceSnap < 0.05f)
                {
                    trailTransform.localPosition = item.targetPosition;
                    if (enableBezier)
                    {
                        if (item.currentTargetNum < bezierPositions.Length - 1)
                            item.currentTargetNum ++;
                        else
                            item.currentTargetNum = 1;
                        item.targetPosition = bezierPositions[item.currentTargetNum];    
                    }
                    else
                    {
                        if (item.currentTargetNum < sourcePositions.Length - 1)
                            item.currentTargetNum ++;
                        else
                            item.currentTargetNum = 1;
                        item.targetPosition = sourcePositions[item.currentTargetNum];
                    }
                }
            }
        }
    }
}