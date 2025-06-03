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

    // Helper struct for double-precision 2D coordinates
    [System.Serializable]
    public struct DoubleVector2
    {
        public double x, y;
        
        public DoubleVector2(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
        
        public static DoubleVector2 operator +(DoubleVector2 a, DoubleVector2 b)
        {
            return new DoubleVector2(a.x + b.x, a.y + b.y);
        }
        
        public static DoubleVector2 operator -(DoubleVector2 a, DoubleVector2 b)
        {
            return new DoubleVector2(a.x - b.x, a.y - b.y);
        }
        
        public static DoubleVector2 operator *(DoubleVector2 a, double scalar)
        {
            return new DoubleVector2(a.x * scalar, a.y * scalar);
        }
        
        public static DoubleVector2 operator *(double scalar, DoubleVector2 a)
        {
            return new DoubleVector2(a.x * scalar, a.y * scalar);
        }
        
        public static double Distance(DoubleVector2 a, DoubleVector2 b)
        {
            double dx = a.x - b.x;
            double dy = a.y - b.y;
            return System.Math.Sqrt(dx * dx + dy * dy);
        }
        
        public Vector2 ToVector2()
        {
            return new Vector2((float)x, (float)y);
        }
        
        public override string ToString()
        {
            return $"({x:F6}, {y:F6})";
        }
    }

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
        DoubleVector2 finalPosition;

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

        // Convert to Vector2 for Unity UI and apply clamping if enabled
        Vector2 finalPositionVector = finalPosition.ToVector2();
        if (enablePositionClamping)
        {
            finalPositionVector = ClampPinPosition(finalPositionVector);
            Debug.Log($"Position after clamping: ({finalPositionVector.x:F2}, {finalPositionVector.y:F2})");
        }

        // Update pin position
        if (pinRect != null)
        {
            pinRect.anchoredPosition = finalPositionVector;
            Debug.Log($"Pin moved to: ({finalPositionVector.x:F2}, {finalPositionVector.y:F2}) from GPS: ({latitude:F6}, {longitude:F6})");
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

    private DoubleVector2 MapCoordinateToPosition(double latitude, double longitude)
    {
        // Get reference points in screen space (convert to double precision)
        DoubleVector2 p1 = new DoubleVector2(referencePin1.anchoredPosition.x, referencePin1.anchoredPosition.y);
        DoubleVector2 p2 = new DoubleVector2(referencePin2.anchoredPosition.x, referencePin2.anchoredPosition.y);
        DoubleVector2 p3 = new DoubleVector2(referencePin3.anchoredPosition.x, referencePin3.anchoredPosition.y);
        DoubleVector2 p4 = new DoubleVector2(referencePin4.anchoredPosition.x, referencePin4.anchoredPosition.y);

        // Get reference coordinates (lat/lng) with full double precision
        DoubleVector2 c1 = new DoubleVector2(referencePin1Longitude, referencePin1Latitude);
        DoubleVector2 c2 = new DoubleVector2(referencePin2Longitude, referencePin2Latitude);
        DoubleVector2 c3 = new DoubleVector2(referencePin3Longitude, referencePin3Latitude);
        DoubleVector2 c4 = new DoubleVector2(referencePin4Longitude, referencePin4Latitude);

        // Target coordinate
        DoubleVector2 targetCoord = new DoubleVector2(longitude, latitude);

        Debug.Log($"Reference points GPS: c1={c1}, c2={c2}, c3={c3}, c4={c4}");
        Debug.Log($"Reference points Screen: p1={p1}, p2={p2}, p3={p3}, p4={p4}");
        Debug.Log($"Target GPS: {targetCoord}");

        // Check for exact matches first
        double eps = 1e-9; // Higher precision for doubles
        if (DoubleVector2.Distance(targetCoord, c1) < eps) return p1;
        if (DoubleVector2.Distance(targetCoord, c2) < eps) return p2;
        if (DoubleVector2.Distance(targetCoord, c3) < eps) return p3;
        if (DoubleVector2.Distance(targetCoord, c4) < eps) return p4;

        // Check for degenerate cases (duplicate reference points)
        if (DoubleVector2.Distance(c1, c2) < eps || DoubleVector2.Distance(c1, c3) < eps || 
            DoubleVector2.Distance(c1, c4) < eps || DoubleVector2.Distance(c2, c3) < eps || 
            DoubleVector2.Distance(c2, c4) < eps || DoubleVector2.Distance(c3, c4) < eps)
        {
            Debug.LogWarning("Degenerate quad detected - some reference points have identical GPS coordinates!");
            // Fallback to simple bounds mapping
            return MapCoordinateToPositionSimple(latitude, longitude);
        }

        // Use generalized coordinate mapping instead of bilinear interpolation
        DoubleVector2 result = MapUsingGeneralizedCoordinates(targetCoord, c1, c2, c3, c4, p1, p2, p3, p4);
        
        Debug.Log($"Final screen position: {result}");
        
        return result;
    }

    private DoubleVector2 MapUsingGeneralizedCoordinates(DoubleVector2 target, 
        DoubleVector2 c1, DoubleVector2 c2, DoubleVector2 c3, DoubleVector2 c4,
        DoubleVector2 p1, DoubleVector2 p2, DoubleVector2 p3, DoubleVector2 p4)
    {
        // Use proper GPS distance calculations with Haversine formula
        // This accounts for the spherical nature of GPS coordinates
        
        double d1 = CalculateGPSDistance(target, c1);
        double d2 = CalculateGPSDistance(target, c2);
        double d3 = CalculateGPSDistance(target, c3);
        double d4 = CalculateGPSDistance(target, c4);
        
        Debug.Log($"GPS distances (meters): d1={d1:F1}m, d2={d2:F1}m, d3={d3:F1}m, d4={d4:F1}m");
        
        // Add small epsilon to prevent division by zero
        double eps = 1e-6; // 1 millimeter
        d1 = System.Math.Max(d1, eps);
        d2 = System.Math.Max(d2, eps);
        d3 = System.Math.Max(d3, eps);
        d4 = System.Math.Max(d4, eps);
        
        // Calculate weights (inverse distance squared for smoother interpolation)
        double w1 = 1.0 / (d1 * d1);
        double w2 = 1.0 / (d2 * d2);
        double w3 = 1.0 / (d3 * d3);
        double w4 = 1.0 / (d4 * d4);
        
        double totalWeight = w1 + w2 + w3 + w4;
        
        // Normalize weights
        w1 /= totalWeight;
        w2 /= totalWeight;
        w3 /= totalWeight;
        w4 /= totalWeight;
        
        Debug.Log($"GPS-corrected IDW weights: w1={w1:F4}, w2={w2:F4}, w3={w3:F4}, w4={w4:F4}");
        
        // Calculate weighted average of screen positions
        DoubleVector2 result = w1 * p1 + w2 * p2 + w3 * p3 + w4 * p4;
        
        return result;
    }

    private double CalculateGPSDistance(DoubleVector2 coord1, DoubleVector2 coord2)
    {
        // Haversine formula for calculating distance between two GPS coordinates
        // Returns distance in meters
        
        double lat1 = coord1.y * System.Math.PI / 180.0; // Convert to radians
        double lon1 = coord1.x * System.Math.PI / 180.0;
        double lat2 = coord2.y * System.Math.PI / 180.0;
        double lon2 = coord2.x * System.Math.PI / 180.0;
        
        double dlat = lat2 - lat1;
        double dlon = lon2 - lon1;
        
        double a = System.Math.Sin(dlat / 2.0) * System.Math.Sin(dlat / 2.0) +
                   System.Math.Cos(lat1) * System.Math.Cos(lat2) *
                   System.Math.Sin(dlon / 2.0) * System.Math.Sin(dlon / 2.0);
        
        double c = 2.0 * System.Math.Atan2(System.Math.Sqrt(a), System.Math.Sqrt(1.0 - a));
        
        double earthRadiusMeters = 6371000.0; // Earth's radius in meters
        double distance = earthRadiusMeters * c;
        
        return distance;
    }

    private DoubleVector2 MapCoordinateToPositionSimple(double latitude, double longitude)
    {
        // Fallback simple mapping using bounds - all calculations in double precision
        double normalizedX = (longitude - topLeftLongitude) / (bottomRightLongitude - topLeftLongitude);
        double normalizedY = (latitude - topLeftLatitude) / (bottomRightLatitude - topLeftLatitude);

        double xPos = normalizedX * mapWidth;
        double yPos = normalizedY * mapHeight;

        return new DoubleVector2(xPos - mapWidth/2.0, yPos - mapHeight/2.0);
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