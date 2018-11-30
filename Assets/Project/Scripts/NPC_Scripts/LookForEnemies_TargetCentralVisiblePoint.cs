using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using HelperFunctions;

public class LookForEnemies_TargetCentralVisiblePoint : MonoBehaviour, ICharacterDetector {

    // Public fields
    public LayerMask characterLayerMask;    //Used to only look for colliders which are CHARACTERS (i.e. on the character layer)
    public LayerMask sightLayerMask;
    public float maxVisionDistance;
    public float checkFrequencySeconds;
    public Transform eyesPosition;
    public int numHorizontalChecks;
    public int numVerticalChecks;
    public bool showingCorners;

    [HideInInspector]
    public event EventHandler<IReadOnlyList<TargetInformation>> VisionUpdatedEvent;   // Interested parties can receive updates when we do vision updates.

    private List<Collider> selfColliders;
    private float nextCheckTime;

    // Use this for initialization
    void Start() {
        // Make sure we don't detect ourselves, by keeping track of which colliders are our own!
        selfColliders = new List<Collider>();
        Collider c = this.gameObject.GetComponent<Collider>();
        if (c != null) selfColliders.Add(this.gameObject.GetComponent<Collider>());
        nextCheckTime = Time.time + checkFrequencySeconds;

        if (numHorizontalChecks <= 1) { numHorizontalChecks = 2; }
        if (numVerticalChecks <= 1) { numVerticalChecks = 2; }
    }

    // Update is called once per frame
    void Update() {
        // If we need to do another check, then do the check!
        if (Time.time >= nextCheckTime) {
            nextCheckTime = Time.time + checkFrequencySeconds;

            List<TargetInformation> visibleCharacterColliders = new List<TargetInformation>();
            // Find all colliders in the character layer that are within the search radius of us!
            foreach (Collider potentialTarget in Physics.OverlapSphere(transform.position, maxVisionDistance, characterLayerMask, QueryTriggerInteraction.Ignore)) {
                if (!selfColliders.Contains(potentialTarget)) {
                    TargetInformation data = CalculateTargetInfo(potentialTarget);
                    if (data != null) visibleCharacterColliders.Add(data);
                }
            }

            // Notify interested parties of the update!
            // DebuggingHelpers.Log("I am the ICharacterDetector and i am sending a vision update, sending " + visibleCharacterColliders.Count + " possible colliders to check");
            VisionUpdatedEvent?.Invoke(this, visibleCharacterColliders.AsReadOnly());
        }
    }

    // Returns null if the target is not visible. If it is visible, this will calculate the 'aim point' based on finding the largest area rectangle
    // of 'hits' in the linecast grid, and then taking the central point of those points as the aim point. Then, we return target info with that 
    // data attached.
    private TargetInformation CalculateTargetInfo(Collider target) {
        // DebuggingHelpers.DrawAxisAlignedBoundingBox(target.bounds, Color.cyan);

        // Perform a 'numHorizontalChecks' by 'numVerticalChecks' number of line casts to try to see the character in the case he is partially obscured
        // First, figure out the maximum 'width' of where the target could be, as the widest range to be the hypotenuse of the Axis-aligned-bounding-box of the target collider.
        Bounds b = target.bounds;
        float width = Mathf.Sqrt(b.size.x * b.size.x + b.size.z * b.size.z);
        float height = b.size.y;
        float horSpacing = (width * 0.95f) / (numHorizontalChecks - 1);
        float verSpacing = (height * 0.95f) / (numVerticalChecks - 1);
        Vector3 horizontalOffsetDirection = Vector3.Cross(target.transform.position - eyesPosition.position, Vector3.up).normalized;
        Vector3 verticalOffsetDirection = Vector3.up;

        Vector3 pointToCheck = target.bounds.center - (horizontalOffsetDirection * (width * 0.95f / 2)) - (verticalOffsetDirection * (height * 0.95f / 2));

        bool scanningForwards = true;
        Vector3[,] pointMatrix = new Vector3[numVerticalChecks, numHorizontalChecks];
        int[] histogramForCurrentRow = new int[numHorizontalChecks];    // Will store the up to date histogram values for 'hits' to assist calcualting largest rectangle in matrix.
                                                                        // SEE: https://www.youtube.com/watch?time_continue=414&v=g8bSdXCG
                                                                        // Histogram array should be initialised to zero by default.

        int maxArea = 0;    // If the max area is still zero by the end of the calculations, then we cannot see the target.
        int maxRegionLeftIndex = -1;
        int maxRegionRightIndex = -1;
        int maxRegionBottomIndex = -1;
        int maxRegionTopIndex = -1;

        for (int i = 0; i < numVerticalChecks; i++) {
            for (int j = 0; j < numHorizontalChecks; j++) {
                // Every point we check should be added to the point matrix, so we can calculate a final position by averaging the positions of the most visible region.
                int col = (scanningForwards) ? j : numHorizontalChecks - 1 - j;
                pointMatrix[i, col] = pointToCheck;

                // Check this spot!
                if (LineCastCheck(target, pointToCheck)) {
                    // HIT! Add to the value of the histogram bar corresponding with this column!
                    // Increment, since the unbroken connection of hits extends downwards 1 more row.
                    // The values for each histogram bar represents how many unbroken hits go upwards from this row and column.
                    histogramForCurrentRow[col] += 1;   
                }
                else {
                    // MISS! Reset the value to zero.
                    histogramForCurrentRow[col] = 0;
                }

                // Move horizontally
                pointToCheck += horizontalOffsetDirection * horSpacing;
            }

            // Now, we can calculate the max area for this histogram region
            int l, r, h;
            int area = GetMaxAreaInHistogram(histogramForCurrentRow, out l, out r, out h);

            // If this row histogram yielded an area with greater area, then we should update everything!
            if (area > maxArea) {
                maxArea = area;
                maxRegionLeftIndex = l;
                maxRegionRightIndex = r;
                maxRegionBottomIndex = i;
                maxRegionTopIndex = i - (h - 1);
            }

            // Move vertically, and flip the direction of the horizontal offset direction vector, so it scans back the opposite direction!
            pointToCheck += verticalOffsetDirection * verSpacing;
            horizontalOffsetDirection = -horizontalOffsetDirection;
            pointToCheck += horizontalOffsetDirection * horSpacing;     // FIX OVERSHOOT. // TODO - do this more elegantly.
            scanningForwards = !scanningForwards;
        }

        // FINISHED SCANNING! Now to calculate the average point of the biggest visible region, or return null if we cannot see the target.
        if (maxArea == 0) {
            return null;
        }
        else {
            // Average the corner positions to get the aim point. 
            // TODO: (Note this algorithm can be optimized to only use two point, opposite corners. But MEH for now)
            Vector3 bottomLeft = pointMatrix[maxRegionBottomIndex, maxRegionLeftIndex];
            Vector3 bottomRight = pointMatrix[maxRegionBottomIndex, maxRegionRightIndex];
            Vector3 topLeft = pointMatrix[maxRegionTopIndex, maxRegionLeftIndex];
            Vector3 topRight = pointMatrix[maxRegionTopIndex, maxRegionRightIndex];

            //if (showingCorners) {
            //    DebuggingHelpers.DrawLine(eyesPosition.position, (bottomLeft + bottomRight + topLeft + topRight) / 4, Color.green);
            //    DebuggingHelpers.DrawLine(eyesPosition.position, bottomLeft, Color.black);
            //    DebuggingHelpers.DrawLine(eyesPosition.position, bottomRight, Color.blue);
            //    DebuggingHelpers.DrawLine(eyesPosition.position, topRight, Color.red);
            //    DebuggingHelpers.DrawLine(eyesPosition.position, topLeft, Color.grey);
            //}

            return new TargetInformation(target, (bottomLeft + bottomRight + topLeft + topRight) / 4);
        }
    }

    private bool LineCastCheck(Collider target, Vector3 point) {
        RaycastHit hitInfo;
        
        if (Physics.Linecast(eyesPosition.position, point, out hitInfo, sightLayerMask, QueryTriggerInteraction.Ignore)) {
            if (hitInfo.transform == target.transform) {
                // We can see the target!
                // if (!showingCorners) DebuggingHelpers.DrawLine(eyesPosition.position, point, Color.green);
                return true;
            }
        }
        // if (!showingCorners) DebuggingHelpers.DrawLine(eyesPosition.position, point, Color.red);
        return false;
    }

    // Static function to calculate the max rectangular area of a histogram (https://www.geeksforgeeks.org/largest-rectangle-under-histogram
    // Modified to pass out through reference parameters the left index, right index, and height of the largest rectangle
    private static int GetMaxAreaInHistogram(int[] hist, out int leftIndexOfMax, out int rightIndexOfMax, out int heightOfMax) {
        // Create an empty stack. The stack holds indexes of hist[] array 
        // The bars stored in stack are always in increasing order of their 
        // heights. 
        Stack<int> stack = new Stack<int>();
        leftIndexOfMax = 0;
        rightIndexOfMax = 0;
        heightOfMax = 0;

        int maxArea = 0; // Initialize max area 
        int topIndex;  // To store top of stack 
        int areaWithTop; // To store area with top bar as the smallest bar 

        // Look through all bars of the histogram.
        int i = 0;
        while (i < hist.Length) {
            // If this bar is higher than the bar on top stack, push it to stack as an index.
            // The stack therefore remembering all the previous 'top' bars.
            // Only increase i if the current bar (i'th bar) is bigger than all the ones currently in the stack.
            if (stack.Count == 0 || hist[stack.Peek()] <= hist[i]) {
                stack.Push(i++);
            }
            // The i'th bar is LOWER than the bar at the current top of the stack. Therefore, we can calculate the area
            // in the histogram assuming that the current 'top' bar (at the top of the stack) is the shortest height.
            // In this case, the i'th bar is NOT included (because it is shorter than the currecnt top), and the second
            // tallest (the second-from-top in the stack) is the left most bound. So, the area is the height of the
            // top-of-stack bar, times the difference in indexes of the second-from-top-of-stack, and i.
            else {
                topIndex = stack.Pop();

                // Calculate the area with hist[topIndex] as smallest bar.
                areaWithTop = hist[topIndex] * (stack.Count == 0 ? i : i - (stack.Peek() + 1));

                // Update max area and out parameters if we found a new contender
                if (maxArea < areaWithTop) {
                    maxArea = areaWithTop;
                    leftIndexOfMax = stack.Count == 0 ? 0 : stack.Peek() + 1;
                    rightIndexOfMax = i-1;
                    heightOfMax = hist[topIndex];
                }
            }
        }

        // Now pop the remaining bars from stack and calculate area with every 
        // popped bar as the smallest bar 
        while (stack.Count > 0) {
            topIndex = stack.Pop();
            areaWithTop = hist[topIndex] * (stack.Count == 0 ? i : i - (stack.Peek() + 1));

            // Update max area and out parameters if we found a new contender
            if (maxArea < areaWithTop) {
                maxArea = areaWithTop;
                leftIndexOfMax = stack.Count == 0 ? 0 : stack.Peek() + 1;
                rightIndexOfMax = i-1;
                heightOfMax = hist[topIndex];
            }
        }

        return maxArea;
    }
}
