using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace HelperFunctions {
    public static class DebuggingHelpers {
        public static bool showDebugging = true;

        public static void DrawAxisAlignedBoundingBox(Bounds b, Color c, float duration = 0.0f) {
            if (!showDebugging) return;
            Debug.Log(b);

            // Draw a bunch of lines to show the bounding box!
            // First, find all the corner points.
            Vector3[] frontFaces = {
                b.min,
                new Vector3(b.max.x, b.min.y, b.min.z),
                new Vector3(b.max.x, b.max.y, b.min.z),
                new Vector3(b.min.x, b.max.y, b.min.z)
            };

            Vector3[] backFaces = {
                new Vector3(b.min.x, b.min.y, b.max.z),
                new Vector3(b.max.x, b.min.y, b.max.z),
                b.max,
                new Vector3(b.min.x, b.max.y, b.max.z)
            };

            // Draw lines
            int prev = 3;
            for (int i=0; i<4; i++) {
                Debug.DrawLine(frontFaces[i], frontFaces[prev], c, duration);
                Debug.DrawLine(backFaces[i], backFaces[prev], c, duration);
                prev = i;

                Debug.DrawLine(frontFaces[i], backFaces[i], c, duration);
            }
        }

        public static void DrawLine(Vector3 from, Vector3 to, Color c, float duration = 0f) {
            if (!showDebugging) return;
            Debug.DrawLine(from, to, c, duration);
        }

        public static void DrawRay(Vector3 from, Vector3 dist, Color c, float duration = 0.0f) {
            if (!showDebugging) return;
            Debug.DrawRay(from, dist, c, duration);
        }

        public static void Log(object msg) {
            if (!showDebugging) return;
            Debug.Log(msg);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string GetCurrentMethodName() {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(2);

            return sf.GetMethod().Name;
        }

        public static string GetCurrentCallstackNames(int maxDepth) {
            var st = new StackTrace();
            var sf = st.GetFrame(1);
            string names = sf.GetMethod().Name;
            for (int i = 1; i < maxDepth && i < st.FrameCount; i++) {
                sf = st.GetFrame(i + 1);
                names = names + ", " + sf.GetMethod().Name;
            }

            return names;
        }

        public static void PrintCurrentMethodName() {
            if (!showDebugging) return;
            Debug.Log(GetCurrentMethodName());
        }

    }
}

