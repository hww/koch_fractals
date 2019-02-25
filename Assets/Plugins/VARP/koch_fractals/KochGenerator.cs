using System.Collections.Generic;
using UnityEngine;

namespace VARP.KochFractals
{
    public class KochGenerator : MonoBehaviour
    {
        // =============================================================================================================
        // Types
        // =============================================================================================================
        
        /// <summary>Initiator type</summary>
        public enum Initiator
        {
            Triangle,
            Square,
            Pentagon,
            Hexagon,
            Heptagon,
            Octagon
        }
        /// <summary>how many points per each initiator</summary> 
        private static readonly int[] pointsNumbersCollection = {3, 4, 5, 6, 7, 8};
        /// <summary>initial angle per each initiator</summary>
        private static readonly float[] initialRotationCollection = {0f, 45f, 36f, 30f, 25.71428f, 22.5f};

        /// <summary>Rotation axis enum</summary>
        public enum Axis { XAxis, YAxis, ZAxis }
        /// <summary>Rotation vector for each axis</summary>
        private static readonly Vector3[] rotateVectorCollection =
            {new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1)};
        /// <summary>Rotation axis for each variant of axis</summary>
        private static readonly Vector3[] rotateAxisCollection =
            {new Vector3(0, 0, 1), new Vector3(1, 0, 0), new Vector3(0, 1, 0)};

        // =============================================================================================================
        // Fields
        // =============================================================================================================


        /// <summary>Rotation axis</summary>
        public Axis axis;
        /// <summary>Initiator type</summary>
        public Initiator initiator;
        /// <summary>Initiator's initial size</summary>
        public float initiatorSize = 1f;
        [Header("Status")]
        /// <summary>Initiator's initial size</summary>
        public float lengthOfSides;
        /// <summary>Points collection</summary>
        protected Vector3[] initiatorPositions;
        /// <summary>Points collection</summary>
        protected Vector3[] sourcePositions;
        /// <summary>Points collection targets for blending</summary>
        protected Vector3[] targetPositions;
        /// <summary>Was any updates of the points</summary>
        protected bool isDirty;
        [Header("Gizmos")]
        /// <summary>Draw gizmos when true</summary>
        public bool drawSourcePosition = true;
        /// <summary>Draw gizmos when true</summary>
        public bool drawTargetPosition = true;

        /// <summary>Generator's curve</summary>
        [Header("Generators")]
        public AnimationCurve generatorCurve;
        /// <summary>Digits in fraction</summary>
        public int generatorPrecision = 2;
        /// <summary>
        /// The koch generators 
        /// </summary>
        public float[] generators;
        // generate bezier line as the last step
        [Header("Bezier")]
        public bool enableBezier;
        // generate bezier line as the last step
        [UnityEngine.Range(8,24)]
        public int bezierVertexCount = 4;
        
        protected int initiatorPointsAmount;
 
        
        private void Awake()
        {
            Initialize();
        }

        private void OnDrawGizmos()
        {
            if (drawSourcePosition || drawTargetPosition)
            {
                Initialize();
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            }

            if (drawSourcePosition)
            {
                Gizmos.color = Color.white;
                for (var i = 0; i < sourcePositions.Length - 1; i++)
                    Gizmos.DrawLine(sourcePositions[i], sourcePositions[i + 1]);
            }
            if (drawTargetPosition)
            {
                Gizmos.color = Color.red;
                for (var i = 0; i < targetPositions.Length - 1; i++)
                    Gizmos.DrawLine(targetPositions[i], targetPositions[i + 1]);
            }
        }

        /// <summary>
        /// Initialization method produces collection of points
        /// </summary>
        private void Initialize()
        {
            isDirty = true;
            var rotateAxis = rotateAxisCollection[(int) axis];
            var rotateVector = rotateVectorCollection[(int) axis];
            var initiatorRotation = initialRotationCollection[(int) initiator];
            initiatorPointsAmount = pointsNumbersCollection[(int) initiator];
            rotateVector = Quaternion.AngleAxis(initiatorRotation, rotateAxis) * rotateVector;
            initiatorPositions = new Vector3[initiatorPointsAmount + 1];
            for (var i = 0; i < initiatorPointsAmount; i++)
            {
                initiatorPositions[i] = rotateVector * initiatorSize;
                rotateVector = Quaternion.AngleAxis(360f / initiatorPointsAmount, rotateAxis) * rotateVector;
            }
            initiatorPositions[initiatorPointsAmount] = initiatorPositions[0]; // additional point is same as first
            lengthOfSides = Vector3.Distance(initiatorPositions[0], initiatorPositions[1]) * 0.5f;
            targetPositions = sourcePositions = initiatorPositions;
            for (var i = 0; i < generators.Length; i++)
                GenerateKoch(generatorCurve, targetPositions, generators[i], rotateAxis);
            
            if (enableBezier)
            {
                sourcePositions = BezierCurve.Generate(sourcePositions, bezierVertexCount);
                targetPositions = BezierCurve.Generate(targetPositions, bezierVertexCount);
            }
        }

        public struct LineSegment
        {
            public Vector3 startPosition;
            public Vector3 endPosition;
            public Vector3 direction;
            public float length;
        }

        /// <summary>
        /// Update sourcePositions and targetPositions by new versions
        /// </summary>
        /// <param name="generatorCurve"></param>
        /// <param name="positions"></param>
        /// <param name="generatorMultiplier"></param>
        /// <param name="rotateAxis"></param>
        private void GenerateKoch(AnimationCurve generatorCurve, Vector3[] positions, float generatorMultiplier,
            Vector3 rotateAxis)
        {
            // crate line segments list from source positions. each segment has start, end, direction and length
            var linesCollection = new List<LineSegment>();
            for (int i = 0; i < positions.Length - 1; i++)
            {
                var line = new LineSegment();
                line.startPosition = positions[i];
                line.endPosition = positions[i + 1];
                line.length = (line.endPosition - line.startPosition).magnitude;
                line.direction = (line.endPosition - line.startPosition) / line.length;
                linesCollection.Add(line);
            } 
            // direction outwards or inwards            
            var rotation = Quaternion.AngleAxis(-90f, rotateAxis);
            
            var newPos = new List<Vector3>();
            var targetPos = new List<Vector3>();
            var keys = generatorCurve.keys;
            var precisionMultiplier = Mathf.Pow(10, generatorPrecision);
            for (int i = 0; i < linesCollection.Count; i++)
            {
                newPos.Add(linesCollection[i].startPosition);
                targetPos.Add(linesCollection[i].startPosition);

                for (var j = 1; j < keys.Length - 1; j++) // skip first and last points as they are (0,0)
                {
                    var time = Mathf.Round(keys[j].time * precisionMultiplier) / precisionMultiplier;
                    var value = Mathf.Round(keys[j].value * precisionMultiplier) / precisionMultiplier;
                    var moveAmount = linesCollection[i].length * time;
                    var heightAmount =  /*moveAmount * */value * generatorMultiplier;
                    var movePos = linesCollection[i].startPosition + linesCollection[i].direction * moveAmount;
                    var heightDir = rotation * linesCollection[i].direction;
                    newPos.Add(movePos);
                    targetPos.Add(movePos + heightDir * heightAmount);
                }
            }
            
            newPos.Add(linesCollection[0].startPosition);
            targetPos.Add(linesCollection[0].startPosition);
            sourcePositions = newPos.ToArray();
            targetPositions = targetPos.ToArray();
            isDirty = true;
        }
    }
}