using System.Collections.Generic;
using UnityEngine;

namespace VARP.KochFractals
{
    public class KochTrail : KochGenerator
    {
        public class TrailItem
        {
            public GameObject go;
            public TrailRenderer trail;
            public int currentTargetNum;
            public Vector3 targetLocalPosition;
            public Color emissionColor;
        }

        [HideInInspector] 
        public List<TrailItem> trails;

        [Header("Trail Properties")] 
        public GameObject trailPrefab;
        public AnimationCurve trailWidthCurve;
        [Range(0, 8)] 
        public int trailEndCapVertices = 4;
        public Material trailMaterial;
        public Gradient trailColor;

        [Header("Audio")] 
        public AudioPeer audioPeer;
        public int[] audioBand = new int[8];
        
        [Header("Other Settings")]
        public Vector2 speedMinMax = new Vector2(0, 100);
        public Vector2 trailWidthMinMax = new Vector2(0.1f, 0.5f);
        public Vector2 trailTimeMinMax = new Vector2(0f, 0.2f);
        public float colorMultiplier = 2f;

        private float lerpPosSpeed;
        private float distanceSnap;
        private Color startColor;
        private Color endColor;

        private void Start()
        {
            startColor = new Color(0,0,0, 0);
            endColor = new Color(0,0,0, 1);
            trails = new List<TrailItem>();
            var step = targetPositions.Length / initiatorPointsAmount;
            
            for (var i = 0; i < initiatorPointsAmount; i++)
            {
                var item = new TrailItem();
                item.go = Instantiate(trailPrefab, transform.position, Quaternion.identity);
                item.go.transform.parent = transform;
                item.trail = item.go.GetComponent<TrailRenderer>();
                item.trail.material = new Material(trailMaterial);
                item.emissionColor = trailColor.Evaluate(i * (1f / initiatorPointsAmount));
                item.trail.numCapVertices = trailEndCapVertices;
                item.trail.widthCurve = trailWidthCurve;
                
                item.go.transform.localPosition = targetPositions[i * step];
                item.currentTargetNum = i * step + 1;
                item.targetLocalPosition = targetPositions[item.currentTargetNum];  
                
                trails.Add(item);
            }
        }

        void Update()
        {
            Movement();
            AudioBehaviour();
        }

        void AudioBehaviour()
        {
            for (var i = 0; i < initiatorPointsAmount; i++)
            {
                var trail = trails[i];
                var bandValue = audioPeer.audioBand[audioBand[i]];
                
                var emissionLerp = Color.Lerp(startColor, trails[i].emissionColor * colorMultiplier,bandValue);
                trail.trail.material.SetColor("_EmissionColor", emissionLerp);
                
                var colorLerp = Color.Lerp(startColor, endColor, bandValue);
                trail.trail.material.SetColor("_Color", colorLerp);

                var widthLerp = Mathf.Lerp(trailWidthMinMax.x, trailWidthMinMax.y, bandValue);
                trail.trail.widthMultiplier = widthLerp;
                
                var timeLerp = Mathf.Lerp(trailTimeMinMax.x, trailTimeMinMax.y, bandValue);
                trail.trail.time = timeLerp;
            }
        }
        void Movement()
        {
            lerpPosSpeed = Mathf.Lerp(speedMinMax.x, speedMinMax.y, audioPeer.amplitude); 
            for (var i = 0; i < trails.Count; i++)
            {
                var item = trails[i];
                var trailTransform = item.go.transform;

                var pos = Vector3.MoveTowards(trailTransform.localPosition, item.targetLocalPosition, Time.deltaTime * lerpPosSpeed);
                if (float.IsNaN(pos.x))
                {
                    // TODO! Fix this case when pos.x,y,z can be NaN
                }
                else
                {
                    trailTransform.localPosition = pos;
                    distanceSnap = Vector3.Distance(trailTransform.localPosition, item.targetLocalPosition);
                
                    if (distanceSnap < 0.05f)
                    {
                        trailTransform.localPosition = item.targetLocalPosition;
                        if (item.currentTargetNum < sourcePositions.Length - 1)
                            item.currentTargetNum ++;
                        else
                            item.currentTargetNum = 1;
                        item.targetLocalPosition = sourcePositions[item.currentTargetNum];
                    }
                    
                }
            }
        }
    }
}