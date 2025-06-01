# Map Systems Implementation - June 1st, 2024

## Overview

This document covers the implementation of two key mapping systems for the YRAF Gallery Hop project: the **MapPinController** and **MapZoomController**. These systems provide accurate GPS positioning on angled/rotated maps and intuitive zoom/pan functionality for enhanced user interaction.

---

## MapPinController.cs

### Problem Solved: Accurate GPS Positioning on Angled Maps

The primary challenge was achieving exact Google Maps positioning accuracy on a map image that wasn't aligned with cardinal directions (North/South/East/West). Traditional coordinate mapping using simple latitude/longitude bounds fails when the map is rotated or skewed at an angle.

### Solution: Quad-to-Quad Bilinear Interpolation

Instead of using rotation-based transformations (which proved problematic), we implemented a **4-point reference system** using bilinear interpolation. This approach handles any map orientation, rotation, or distortion seamlessly.

#### How It Works:

1. **4 Reference Points**: Define GPS coordinates and screen positions for 4 corners/areas of the map
2. **Barycentric Coordinates**: Calculate where the target GPS location falls within the GPS quadrilateral
3. **Screen Mapping**: Apply the same barycentric coordinates to the screen quadrilateral
4. **Result**: Pixel-perfect positioning regardless of map angle or distortion

#### Key Features:

- **Double Precision GPS**: All coordinates use `double` type to preserve GPS accuracy
- **Exact Match Detection**: Handles cases where target GPS exactly matches reference points
- **Degenerate Quad Detection**: Fallback to simple bounds mapping when reference points are invalid
- **Fallback System**: Simple bounds mapping available if 4-point system isn't configured
- **Cross-Platform Location**:
  - Editor: Debug coordinates for testing
  - Mobile: Standard location services
  - **WebGL/Browser**: Proper geolocation permission handling with extended timeout
- **Position Clamping**: Optional rectangular bounds to keep pins within visible map area

#### Configuration:

```csharp
[Header("4-Point Reference System")]
[SerializeField] private bool useReferencePoints = false;

// Reference Point 1 (Top-Left area)
[SerializeField] private RectTransform referencePin1;
[SerializeField] private double referencePin1Latitude;
[SerializeField] private double referencePin1Longitude;

// ... (similar for points 2, 3, 4)

[Header("Pin Position Clamping")]
[SerializeField] private bool enablePositionClamping = false;
[SerializeField] private Vector2 clampTopLeft = new Vector2(-500, 500);
[SerializeField] private Vector2 clampBottomRight = new Vector2(500, -500);
```

#### Technical Implementation:

- **Bilinear Interpolation Math**: Solves inverse interpolation to find barycentric coordinates (u,v)
- **Quadratic Equation Solving**: Handles complex cases where simple linear interpolation isn't sufficient
- **Numerical Stability**: Epsilon-based comparisons and fallback handling for edge cases
- **Real-time Updates**: 3-second GPS polling with immediate debug coordinate updates

---

## MapZoomController.cs

### Problem Solved: Intuitive Map Navigation

Users need to zoom in/out and pan around the map to explore different areas while maintaining proper boundaries and preventing white space exposure.

### Solution: Multi-Input Zoom & Smart Drag Clamping

A comprehensive zoom and pan system that works across all platforms with intelligent boundary detection.

#### Key Features:

##### **Zoom Functionality:**

- **Pinch-to-Zoom** (Mobile): Two-finger gesture support
- **Mouse Wheel Zoom** (Desktop/Editor): Scroll wheel zoom
- **Configurable Limits**: Min/max zoom levels (default: 0.5x to 3x)
- **Smooth Scaling**: Applies zoom via `localScale` transformation

##### **Drag/Pan Functionality:**

- **Tap & Hold Drag** (Mobile): Single finger touch and drag
- **Click & Drag** (Desktop): Left mouse button drag
- **Smart Clamping**: Drag distance based on actual map size and zoom level
- **No White Space**: Prevents dragging beyond map content boundaries

##### **Intelligent Clamping System:**

```csharp
// At 1x zoom: No dragging allowed (map fits perfectly)
// At higher zooms: Drag distance = mapSize * (zoom - 1) / 2

float maxDragX = Mathf.Abs(mapSize.width) * (currentZoom - 1f) / 2f;
float maxDragY = Mathf.Abs(mapSize.height) * (currentZoom - 1f) / 2f;
```

This ensures:

- **1x Zoom**: `maxDrag = 0` → No dragging possible
- **2x Zoom**: `maxDrag = mapSize/2` → Can access all off-screen content
- **3x Zoom**: `maxDrag = mapSize` → Full range to explore zoomed content

#### Configuration:

```csharp
[Header("Zoom Settings")]
[SerializeField] private float minZoom = 0.5f;
[SerializeField] private float maxZoom = 3f;
[SerializeField] private float zoomSpeed = 0.5f;
[SerializeField] private float mouseWheelZoomSpeed = 0.1f;

[Header("Drag Settings")]
[SerializeField] private float dragSensitivity = 1f;
[SerializeField] private float maxDragDistance = 500f; // No longer used - calculated automatically
```

#### Public Methods:

- `ResetZoom()`: Returns to 1x zoom and center position
- `GetCurrentZoom()`: Returns current zoom level
- `SetDragSensitivity(float)`: Adjust drag responsiveness at runtime

---

## Integration & Usage

### Setup Steps:

1. **MapPinController**:

   - Attach to a GameObject with the map pin UI element
   - Configure 4 reference points with known GPS coordinates
   - Set map bounds for fallback system
   - Enable position clamping if needed

2. **MapZoomController**:
   - Attach to a GameObject with the map RectTransform
   - Assign the map RectTransform reference
   - Adjust zoom limits and sensitivity as needed

### Best Practices:

- **Reference Points**: Choose 4 well-distributed points across your map for best accuracy
- **Testing**: Use debug coordinates in editor before deploying with real GPS
- **Browser Builds**: Ensure HTTPS for WebGL geolocation to work
- **Performance**: 3-second GPS updates provide good balance of accuracy vs. battery life

---

## Technical Achievements

1. **Precision**: Double-precision GPS handling prevents coordinate drift
2. **Flexibility**: Works with any map orientation without manual rotation calculations
3. **Robustness**: Handles edge cases like duplicate reference points and exact matches
4. **Cross-Platform**: Unified codebase for Editor, Mobile, and Browser builds
5. **User Experience**: Intuitive controls with smart boundary detection

This system provides museum visitors with accurate, responsive map navigation that works reliably across all devices and platforms.
