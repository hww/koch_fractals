using UnityEngine;

namespace VARP.KochFractals
{
    [RequireComponent(typeof(LineRenderer))]
    public class KochLine : KochGenerator
    {
        [HideInInspector]
        public LineRenderer lineRenderer;


        [Header("Blend Source & Target")] 
        public bool enableInterpolation;
        [UnityEngine.Range(0f,1f)]
        public float interpolationRatio = 1f;

        // use or not the audio source to control this koch line
        [Header("Audio")]
        public bool enableAudio;
        // reference to audio source
        public AudioPeer audioPeer;
        // setup bands for for each initializer side
        public int[] audioBandPerSide;
        // material will change brightness by this band
        public int audioBandForMaterial;
        // color for line renderer
        [Header("Material")]
        public Color color;
        // material will be assigned to the line
        public Material material;
        // multiply emission by this value
        public float emissionMultiplier = 1;
        
        private Vector3[] pointsCollectionBlended;

        private Material materialInstance;
        private float[] lerpAudio;
        
        private void Start()
        {
            materialInstance = new Material(material);
            lerpAudio = new float[initiatorPointsAmount];
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.material = materialInstance;
            lineRenderer.enabled = true;
            lineRenderer.useWorldSpace = false;
            lineRenderer.loop = true;
            lineRenderer.positionCount = sourcePositions.Length;
            lineRenderer.SetPositions(sourcePositions);
            if (audioBandPerSide.Length != initiatorPointsAmount)
                System.Array.Resize(ref audioBandPerSide, initiatorPointsAmount);
        }

        public void Update()
        {
            if (enableAudio)
                materialInstance.SetColor("_EmissionColor", color * audioPeer.bandBuffer[audioBandForMaterial] * emissionMultiplier);
            
            if (isDirty || enableAudio)
            {
                isDirty = false;
                
                // make blended version of the lines
                if (pointsCollectionBlended == null || pointsCollectionBlended.Length != sourcePositions.Length)
                    pointsCollectionBlended = new Vector3[sourcePositions.Length];

                if (enableInterpolation)
                {
                    if (enableAudio)
                    {
                        var count = 0;
                        for (var i=0; i<initiatorPointsAmount; i++)
                        {
                            lerpAudio[i] = audioPeer.bandBuffer[audioBandPerSide[i]];
                            var end = (sourcePositions.Length - 1) / initiatorPointsAmount;
                            for (var j = 0; j < end; j++)
                            {
                                pointsCollectionBlended[count] = Vector3.Lerp(
                                    sourcePositions[count], 
                                    targetPositions[count], 
                                    lerpAudio[i]);
                                count++;
                            }
                        }
                        pointsCollectionBlended[count] = Vector3.Lerp(
                            sourcePositions[count], 
                            targetPositions[count], 
                            lerpAudio[initiatorPointsAmount-1]);
                    }
                    else
                    {
                        for (var i = 0; i < pointsCollectionBlended.Length; i++)
                            pointsCollectionBlended[i] =
                                Vector3.Lerp(sourcePositions[i], targetPositions[i], interpolationRatio);
                    }                
                }
                
                if (enableInterpolation)
                {
                    lineRenderer.positionCount = pointsCollectionBlended.Length;
                    lineRenderer.SetPositions(pointsCollectionBlended);
                }
                else
                {
                    lineRenderer.positionCount = targetPositions.Length;
                    lineRenderer.SetPositions(targetPositions); 
                }
            }
        }
    }
}