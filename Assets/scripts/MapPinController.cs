using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class MapPinController : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void RequestGeolocation();
    
    [DllImport("__Internal")]
    private static extern bool IsGeolocationAvailable();
    
    [DllImport("__Internal")]
    private static extern double GetLatitude();
    
    [DllImport("__Internal")]
    private static extern double GetLongitude();
    
    [DllImport("__Internal")]
    private static extern bool HasLocationData();
#endif

    [Header("Map Display")]
    [SerializeField] private double mapWidth = 1000.0;  // Width of the map in pixels/units
    [SerializeField] private double mapHeight = 1000.0; // Height of the map in pixels/units
    [SerializeField] private RectTransform pinRect;   // Reference to the pin's RectTransform

    [Header("4-Point Reference System")]
    [SerializeField] private bool useReferencePoints = false;
    
    [Header("Reference Point 1 (Top-Left area)")]
    [SerializeField] private RectTransform referencePin1;
    [SerializeField] private double referencePin1Latitude;
    [SerializeField] private double referencePin1Longitude;
    
    [Header("Reference Point 2 (Top-Right area)")]
    [SerializeField] private RectTransform referencePin2;
    [SerializeField] private double referencePin2Latitude;
    [SerializeField] private double referencePin2Longitude;
    
    [Header("Reference Point 3 (Bottom-Left area)")]
    [SerializeField] private RectTransform referencePin3;
    [SerializeField] private double referencePin3Latitude;
    [SerializeField] private double referencePin3Longitude;
    
    [Header("Reference Point 4 (Bottom-Right area)")]
    [SerializeField] private RectTransform referencePin4;
    [SerializeField] private double referencePin4Latitude;
    [SerializeField] private double referencePin4Longitude;

    [Header("Fallback Map Bounds (if not using 4-point system)")]
    [SerializeField] private double topLeftLatitude;
    [SerializeField] private double topLeftLongitude;
    [SerializeField] private double bottomRightLatitude;
    [SerializeField] private double bottomRightLongitude;

    [Header("Debug Settings")]
    [SerializeField] private bool useDebugLocation = false;
    [SerializeField] private double debugLatitude;
    [SerializeField] private double debugLongitude;

    [Header("Pin Position Clamping")]
    [SerializeField] private bool enablePositionClamping = false;
    [SerializeField] private Vector2 clampTopLeft = new Vector2(-500, 500);
    [SerializeField] private Vector2 clampBottomRight = new Vector2(500, -500);

    private bool isLocationServiceEnabled = false;
    private Coroutine locationUpdateCoroutine;

    private void Start()
    {
        #if UNITY_EDITOR
        // In editor, skip location service initialization and start update coroutine directly
        isLocationServiceEnabled = true;
        if (locationUpdateCoroutine != null)
        {
            StopCoroutine(locationUpdateCoroutine);
        }
        locationUpdateCoroutine = StartCoroutine(UpdateLocation());
        #elif UNITY_WEBGL
        // In WebGL (browser), request geolocation permission
        StartCoroutine(InitializeWebGLLocation());
        #else
        // In build, initialize location services properly
        StartCoroutine(InitializeLocationService());
        #endif
    }

    private void OnValidate()
    {
        #if UNITY_EDITOR
        // Update pin position immediately when debug values change in editor
        if (useDebugLocation)
        {
            UpdatePinPosition(debugLatitude, debugLongitude);
        }
        #endif
    }

    private IEnumerator InitializeLocationService()
    {
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogWarning("Location services are not enabled!");
            yield break;
        }

        // Start service before querying location
        Input.location.Start(1f, 0.1f);

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // If service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            Debug.LogWarning("Location service initialization timed out!");
            yield break;
        }

        // If service failed to initialize
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogWarning("Unable to determine device location!");
            yield break;
        }

        // Service has initialized successfully
        isLocationServiceEnabled = true;
        Debug.Log("Location service initialized successfully!");

        // Start the location update coroutine
        if (locationUpdateCoroutine != null)
        {
            StopCoroutine(locationUpdateCoroutine);
        }
        locationUpdateCoroutine = StartCoroutine(UpdateLocation());
    }

    private IEnumerator UpdateLocation()
    {
        while (true)
        {
            if (isLocationServiceEnabled && !useDebugLocation)
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                // Use WebGL geolocation
                if (HasLocationData())
                {
                    double latitude = GetLatitude();
                    double longitude = GetLongitude();
                    UpdatePinPosition(latitude, longitude);
                }
                else
                {
                    Debug.LogWarning("No WebGL location data available");
                }
#else
                // Use Unity's location service
                double latitude = Input.location.lastData.latitude;
                double longitude = Input.location.lastData.longitude;
                UpdatePinPosition(latitude, longitude);
#endif
            }
            else if (useDebugLocation)
            {
                UpdatePinPosition(debugLatitude, debugLongitude);
            }

            yield return new WaitForSeconds(3f);
        }
    }

    private void UpdatePinPosition(double latitude, double longitude)
    {
        Vector2 finalPosition;

        if (useReferencePoints && ValidateReferencePoints())
        {
            // Use 4-point bilinear interpolation
            finalPosition = MapCoordinateToPosition(latitude, longitude);
            Debug.Log($"Using 4-point mapping: ({latitude:F6}, {longitude:F6}) -> ({finalPosition.x:F2}, {finalPosition.y:F2})");
        }
        else
        {
            // Fallback to simple bounds mapping
            finalPosition = MapCoordinateToPositionSimple(latitude, longitude);
            Debug.Log($"Using simple mapping: ({latitude:F6}, {longitude:F6}) -> ({finalPosition.x:F2}, {finalPosition.y:F2})");
        }

        // Apply position clamping if enabled
        if (enablePositionClamping)
        {
            finalPosition = ClampPinPosition(finalPosition);
            Debug.Log($"Position after clamping: ({finalPosition.x:F2}, {finalPosition.y:F2})");
        }

        // Update pin position
        if (pinRect != null)
        {
            pinRect.anchoredPosition = finalPosition;
            Debug.Log($"Pin moved to: ({finalPosition.x:F2}, {finalPosition.y:F2}) from GPS: ({latitude:F6}, {longitude:F6})");
        }
    }

    private bool ValidateReferencePoints()
    {
        return referencePin1 != null && referencePin2 != null && 
               referencePin3 != null && referencePin4 != null &&
               referencePin1Latitude != 0 && referencePin1Longitude != 0 &&
               referencePin2Latitude != 0 && referencePin2Longitude != 0 &&
               referencePin3Latitude != 0 && referencePin3Longitude != 0 &&
               referencePin4Latitude != 0 && referencePin4Longitude != 0;
    }

    private Vector2 MapCoordinateToPosition(double latitude, double longitude)
    {
        // Get reference points in screen space
        Vector2 p1 = referencePin1.anchoredPosition; // Point 1
        Vector2 p2 = referencePin2.anchoredPosition; // Point 2
        Vector2 p3 = referencePin3.anchoredPosition; // Point 3
        Vector2 p4 = referencePin4.anchoredPosition; // Point 4

        // Get reference coordinates (lat/lng)
        Vector2 c1 = new Vector2((float)referencePin1Longitude, (float)referencePin1Latitude);
        Vector2 c2 = new Vector2((float)referencePin2Longitude, (float)referencePin2Latitude);
        Vector2 c3 = new Vector2((float)referencePin3Longitude, (float)referencePin3Latitude);
        Vector2 c4 = new Vector2((float)referencePin4Longitude, (float)referencePin4Latitude);

        // Target coordinate
        Vector2 targetCoord = new Vector2((float)longitude, (float)latitude);

        Debug.Log($"Reference points GPS: c1={c1}, c2={c2}, c3={c3}, c4={c4}");
        Debug.Log($"Reference points Screen: p1={p1}, p2={p2}, p3={p3}, p4={p4}");
        Debug.Log($"Target GPS: {targetCoord}");

        // Check for exact matches first
        float eps = 1e-6f;
        if (Vector2.Distance(targetCoord, c1) < eps) return p1;
        if (Vector2.Distance(targetCoord, c2) < eps) return p2;
        if (Vector2.Distance(targetCoord, c3) < eps) return p3;
        if (Vector2.Distance(targetCoord, c4) < eps) return p4;

        // Check for degenerate cases (duplicate reference points)
        if (Vector2.Distance(c1, c2) < eps || Vector2.Distance(c1, c3) < eps || 
            Vector2.Distance(c1, c4) < eps || Vector2.Distance(c2, c3) < eps || 
            Vector2.Distance(c2, c4) < eps || Vector2.Distance(c3, c4) < eps)
        {
            Debug.LogWarning("Degenerate quad detected - some reference points have identical GPS coordinates!");
            // Fallback to simple bounds mapping
            return MapCoordinateToPositionSimple(latitude, longitude);
        }

        // Find barycentric coordinates (u,v) for the target coordinate within the quad c1,c2,c3,c4
        Vector2 uv = FindBarycentricCoordinates(targetCoord, c1, c2, c3, c4);

        Debug.Log($"Calculated barycentric: u={uv.x:F3}, v={uv.y:F3}");

        // Apply the same barycentric coordinates to the screen quad p1,p2,p3,p4
        Vector2 result = BilinearInterpolateQuad(uv.x, uv.y, p1, p2, p3, p4);
        
        Debug.Log($"Final screen position: {result}");
        
        return result;
    }

    private Vector2 FindBarycentricCoordinates(Vector2 target, Vector2 c1, Vector2 c2, Vector2 c3, Vector2 c4)
    {
        // Solve the inverse bilinear interpolation problem
        // target = (1-u)(1-v)*c1 + u(1-v)*c2 + (1-u)v*c3 + uv*c4
        // This expands to: target = c1 + u*(c2-c1) + v*(c3-c1) + uv*(c1-c2-c3+c4)
        
        Vector2 A = target - c1;
        Vector2 B = c2 - c1;
        Vector2 C = c3 - c1;
        Vector2 D = c1 - c2 - c3 + c4;

        // We need to solve: A = B*u + C*v + D*u*v
        // This gives us two equations (one for x, one for y):
        // A.x = B.x*u + C.x*v + D.x*u*v
        // A.y = B.y*u + C.y*v + D.y*u*v

        float eps = 1e-6f;
        
        // If D is very small, this becomes a linear system
        if (Mathf.Abs(D.x) < eps && Mathf.Abs(D.y) < eps)
        {
            // Linear case: A = B*u + C*v
            // Solve using Cramer's rule
            float det = B.x * C.y - B.y * C.x;
            if (Mathf.Abs(det) < eps)
            {
                // Degenerate case, fallback
                Debug.LogWarning("Degenerate linear case in bilinear interpolation");
                return new Vector2(0.5f, 0.5f);
            }
            
            float uLinear = (A.x * C.y - A.y * C.x) / det;
            float vLinear = (B.x * A.y - B.y * A.x) / det;
            Debug.Log($"Linear case: u={uLinear:F3}, v={vLinear:F3} for target {target}");
            return new Vector2(uLinear, vLinear);
        }

        // Quadratic case - solve using the method from the research
        // Rearrange to: (A.x - C.x*v) = (B.x + D.x*v)*u and (A.y - C.y*v) = (B.y + D.y*v)*u
        // So: (A.x - C.x*v)/(B.x + D.x*v) = (A.y - C.y*v)/(B.y + D.y*v)
        // Cross multiply: (A.x - C.x*v)*(B.y + D.y*v) = (A.y - C.y*v)*(B.x + D.x*v)
        
        // Expand and collect terms for quadratic in v:
        // A.x*B.y + A.x*D.y*v - C.x*v*B.y - C.x*v*D.y*v = A.y*B.x + A.y*D.x*v - C.y*v*B.x - C.y*v*D.x*v
        // (A.x*D.y - A.y*D.x - C.x*B.y + C.y*B.x)*v + (C.y*D.x - C.x*D.y)*v^2 = A.y*B.x - A.x*B.y
        
        float a = C.y * D.x - C.x * D.y;
        float b = A.x * D.y - A.y * D.x - C.x * B.y + C.y * B.x;
        float c = A.y * B.x - A.x * B.y;

        float vResult;
        if (Mathf.Abs(a) < eps)
        {
            // Linear equation in v
            if (Mathf.Abs(b) < eps)
            {
                vResult = 0.5f; // Fallback
                Debug.LogWarning("Degenerate quadratic case in bilinear interpolation");
            }
            else
            {
                vResult = -c / b;
            }
        }
        else
        {
            // Quadratic equation: a*v^2 + b*v + c = 0
            float discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
            {
                Debug.LogWarning($"Negative discriminant in bilinear interpolation: {discriminant}");
                return new Vector2(0.5f, 0.5f); // Fallback
            }
            
            float sqrtDisc = Mathf.Sqrt(discriminant);
            float v1 = (-b + sqrtDisc) / (2 * a);
            float v2 = (-b - sqrtDisc) / (2 * a);
            
            // Choose the root that's in [0,1], or closest to that range
            if (v1 >= 0 && v1 <= 1)
                vResult = v1;
            else if (v2 >= 0 && v2 <= 1)
                vResult = v2;
            else
            {
                // Both roots are outside [0,1], choose the closer one
                float dist1 = Mathf.Min(Mathf.Abs(v1), Mathf.Abs(v1 - 1));
                float dist2 = Mathf.Min(Mathf.Abs(v2), Mathf.Abs(v2 - 1));
                vResult = (dist1 < dist2) ? v1 : v2;
                Debug.Log($"Both v roots outside [0,1]: v1={v1:F3}, v2={v2:F3}, chose v={vResult:F3}");
            }
        }

        // Now solve for u given v
        float denom_x = B.x + D.x * vResult;
        float denom_y = B.y + D.y * vResult;
        
        float uResult;
        if (Mathf.Abs(denom_x) > Mathf.Abs(denom_y))
        {
            uResult = (A.x - C.x * vResult) / denom_x;
        }
        else if (Mathf.Abs(denom_y) > eps)
        {
            uResult = (A.y - C.y * vResult) / denom_y;
        }
        else
        {
            uResult = 0.5f; // Fallback
            Debug.LogWarning("Zero denominator in u calculation");
        }

        Debug.Log($"Barycentric coords: u={uResult:F3}, v={vResult:F3} for target {target}");
        
        // Don't clamp - allow coordinates outside [0,1] for extrapolation
        return new Vector2(uResult, vResult);
    }

    private Vector2 BilinearInterpolateQuad(float u, float v, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        // Standard bilinear interpolation:
        // result = (1-u)(1-v)*p1 + u(1-v)*p2 + (1-u)v*p3 + uv*p4
        return (1f - u) * (1f - v) * p1 + 
               u * (1f - v) * p2 + 
               (1f - u) * v * p3 + 
               u * v * p4;
    }

    private Vector2 MapCoordinateToPositionSimple(double latitude, double longitude)
    {
        // Fallback simple mapping using bounds
        float normalizedX = Mathf.InverseLerp((float)topLeftLongitude, (float)bottomRightLongitude, (float)longitude);
        float normalizedY = Mathf.InverseLerp((float)topLeftLatitude, (float)bottomRightLatitude, (float)latitude);

        float xPos = Mathf.Lerp(0, (float)mapWidth, normalizedX);
        float yPos = Mathf.Lerp(0, (float)mapHeight, normalizedY);

        return new Vector2(xPos - (float)mapWidth/2, yPos - (float)mapHeight/2);
    }

    private void OnDestroy()
    {
        if (locationUpdateCoroutine != null)
        {
            StopCoroutine(locationUpdateCoroutine);
        }

        if (Input.location.status == LocationServiceStatus.Running)
        {
            Input.location.Stop();
        }
    }

    // Helper method to get current coordinates (can be used for debugging)
    public Vector2 GetCurrentCoordinates()
    {
        if (useDebugLocation)
        {
            return new Vector2((float)debugLongitude, (float)debugLatitude);
        }
        else if (isLocationServiceEnabled)
        {
            return new Vector2(Input.location.lastData.longitude, Input.location.lastData.latitude);
        }
        return Vector2.zero;
    }

#if UNITY_WEBGL
    private IEnumerator InitializeWebGLLocation()
    {
        #if UNITY_EDITOR
        // In editor, just enable and start updating
        isLocationServiceEnabled = true;
        if (locationUpdateCoroutine != null)
        {
            StopCoroutine(locationUpdateCoroutine);
        }
        locationUpdateCoroutine = StartCoroutine(UpdateLocation());
        yield break;
        #else
        
        Debug.Log("Initializing WebGL geolocation...");
        
        // Check if geolocation is available in browser
        if (!IsGeolocationAvailable())
        {
            Debug.LogWarning("Geolocation is not available in this browser!");
            yield break;
        }
        
        // Request geolocation permission
        RequestGeolocation();
        Debug.Log("Geolocation permission requested...");
        
        // Wait for permission and first location data
        int maxWait = 30; // 30 seconds timeout
        while (!HasLocationData() && maxWait > 0)
        {
            Debug.Log($"Waiting for geolocation data... ({maxWait}s remaining)");
            yield return new WaitForSeconds(1);
            maxWait--;
        }
        
        if (maxWait <= 0)
        {
            Debug.LogWarning("Geolocation request timed out!");
            yield break;
        }
        
        if (HasLocationData())
        {
            isLocationServiceEnabled = true;
            Debug.Log("WebGL geolocation initialized successfully!");
            
            // Start the location update coroutine
            if (locationUpdateCoroutine != null)
            {
                StopCoroutine(locationUpdateCoroutine);
            }
            locationUpdateCoroutine = StartCoroutine(UpdateLocation());
        }
        else
        {
            Debug.LogWarning("Failed to get geolocation data!");
        }
        #endif
    }
#endif

    private Vector2 ClampPinPosition(Vector2 position)
    {
        float clampedX = Mathf.Clamp(position.x, clampTopLeft.x, clampBottomRight.x);
        float clampedY = Mathf.Clamp(position.y, clampBottomRight.y, clampTopLeft.y);
        
        return new Vector2(clampedX, clampedY);
    }
} 