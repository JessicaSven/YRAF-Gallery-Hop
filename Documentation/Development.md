# Development Documentation

## System Overview

The YRAF Gallery Hop is a Unity-based interactive gallery experience that combines QR code scanning, place tracking, and user data management. The system is built around several key components that work together to create an engaging user experience.

## Core Components

### 1. Session Management

- **SessionManager**: Central controller managing game state and progression
- **VisitedPlacesManager**: Tracks and persists visited locations
- **UIController**: Manages UI state and user interactions

### 2. Data Persistence

- **Firebase Integration**: Stores user data and preferences
- **PlayerPrefs**: Local storage for visited places and game progress
- **Data Structures**: Well-defined classes for data management

### 3. User Interface

- **QR Scanner**: Camera-based QR code detection
- **Talking Raven**: Interactive dialogue system
- **Gallery List**: Displays completed locations
- **Sound System**: Audio feedback and ambiance

## Recent Changes and Improvements

### VisitedPlacesManager Updates

```csharp
public class VisitedPlacesManager : MonoBehaviour
{
    // Added persistence using PlayerPrefs
    private const string VISITED_PLACES_KEY = "VisitedPlaces";

    // New methods for data persistence
    private void LoadVisitedPlaces()
    private void SaveVisitedPlaces()

    // New method for resetting progress
    public void ClearAllVisitedPlaces()
}
```

Key improvements:

1. Persistent storage of visited places
2. Automatic loading of saved state on game start
3. Ability to reset progress
4. Integration with SessionManager for state updates

## Best Practices

### 1. Data Management

- Use PlayerPrefs for local data persistence
- Implement proper save/load mechanisms
- Clear data appropriately when resetting
- Maintain data consistency across components

### 2. State Management

- Keep track of visited places in a HashSet for O(1) lookups
- Update UI and game state when places are visited
- Ensure proper synchronization between components
- Handle edge cases (e.g., no places visited)

### 3. Code Organization

- Use singleton pattern for managers
- Implement proper initialization in Awake()
- Maintain clear separation of concerns
- Follow Unity's component-based architecture

## Implementation Guidelines

### Adding New Places

1. Create a new PlaceSO asset
2. Configure place details (name, URL, position)
3. Add to SessionManager's places array
4. Test QR code scanning and place tracking

### Modifying Place Tracking

1. Update VisitedPlacesManager methods
2. Ensure proper saving/loading
3. Update UI components
4. Test persistence across sessions

### Resetting Progress

1. Call ClearAllVisitedPlaces()
2. Verify UI updates
3. Check persistence is cleared
4. Test new place tracking

## Testing Checklist

### Place Tracking

- [ ] Places are properly marked as visited
- [ ] Progress persists between sessions
- [ ] UI updates correctly
- [ ] Reset functionality works

### Data Persistence

- [ ] PlayerPrefs saves correctly
- [ ] Data loads on game start
- [ ] Reset clears all data
- [ ] No data corruption

### Integration

- [ ] SessionManager updates properly
- [ ] UI reflects current state
- [ ] Sound effects trigger correctly
- [ ] Raven dialogue works

## Future Improvements

1. **Enhanced Persistence**

   - Add cloud backup
   - Implement data versioning
   - Add progress recovery

2. **UI/UX Improvements**

   - Add progress indicators
   - Implement place categories
   - Add achievement system

3. **Performance Optimization**
   - Optimize data structures
   - Implement caching
   - Reduce memory usage

## Troubleshooting

### Common Issues

1. Places not saving

   - Check PlayerPrefs implementation
   - Verify save calls
   - Check for exceptions

2. UI not updating

   - Verify event calls
   - Check component references
   - Validate state changes

3. Reset not working
   - Check ClearAllVisitedPlaces implementation
   - Verify UI updates
   - Test persistence clearing

## Security Considerations

1. **Data Protection**

   - Validate input data
   - Sanitize saved strings
   - Implement proper error handling

2. **User Privacy**
   - Clear sensitive data on reset
   - Implement proper data deletion
   - Follow privacy guidelines

## Performance Guidelines

1. **Memory Management**

   - Use appropriate data structures
   - Clear unused resources
   - Implement proper cleanup

2. **Optimization**
   - Minimize PlayerPrefs operations
   - Cache frequently used data
   - Optimize UI updates
