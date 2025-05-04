# Firebase Data Structure Documentation

## Overview

The Firebase integration manages visitor data for the gallery application, storing information about users' interactions, favorite exhibits, and game completion status. The system uses Firestore's REST API for all database operations.

## Components

### 1. UserData (FirebaseService.cs)

Core data structure representing a visitor's information.

#### Fields:

```csharp
public class UserData
{
    public string Email;           // Visitor's email address
    public bool CompletedGame;     // Game completion status
    public string FavouriteExhibit;// Preferred exhibit
    public string FullName;        // Visitor's full name
    public string DateCreated;     // ISO 8601 timestamp
    public string DateUpdated;     // ISO 8601 timestamp
}
```

### 2. Firestore Field Wrappers

Specialized classes for Firestore's type system requirements.

```csharp
public class StringValue
{
    public string stringValue;
}

public class BoolValue
{
    public bool booleanValue;
}

public class FirestoreFields
{
    public StringValue Email;
    public BoolValue CompletedGame;
    public StringValue FavouriteExhibit;
    public StringValue FullName;
    public StringValue DateCreated;
    public StringValue DateUpdated;
}
```

## Implementation

### Database Structure

1. **Collection**: Visitors
2. **Documents**: Auto-generated IDs
3. **Fields**: Strongly typed according to Firestore requirements

### REST API Endpoints

Base URL:

```
https://firestore.googleapis.com/v1/projects/yraf-gallery-hop/databases/(default)/documents
```

### Operations

1. **Create New Visitor**

   ```csharp
   // Example usage
   UserData newUser = new UserData
   {
       Email = "visitor@example.com",
       CompletedGame = true,
       FavouriteExhibit = "Exhibit A",
       FullName = "John Doe"
   };
   await firebaseService.SaveUserData(newUser);
   ```

2. **Update Favorite Exhibit**
   ```csharp
   // Example usage
   await firebaseService.UpdateFavouriteExhibit("visitor@example.com", "New Exhibit");
   ```

## Data Rules

1. **Email Uniqueness**

   - Each email can only have one document
   - Checked before creation

2. **Timestamps**

   - DateCreated: Set once at creation
   - DateUpdated: Modified on every update
   - Format: ISO 8601 (e.g., "2024-04-01T10:30:00.000Z")

3. **Required Fields**
   - All fields must be present
   - CompletedGame defaults to true for new users

## Best Practices

1. **Error Handling**

   - Check for existing users before creation
   - Validate email format
   - Handle network errors gracefully

2. **Data Updates**

   - Use PATCH for partial updates
   - Only update necessary fields
   - Always update DateUpdated timestamp

3. **Data Validation**
   - Verify email existence before updates
   - Ensure all required fields are present
   - Validate data types before sending

## Technical Requirements

- Unity 2020.3 or later
- Internet connection
- Firestore database configured in project

## Integration Example

```csharp
// Example of saving new visitor data
public class VisitorForm : MonoBehaviour
{
    [SerializeField] private FirebaseService firebaseService;

    public async Task SaveVisitor(string email, string fullName, string exhibit)
    {
        var userData = new UserData
        {
            Email = email,
            FullName = fullName,
            FavouriteExhibit = exhibit,
            CompletedGame = true
        };

        await firebaseService.SaveUserData(userData);
    }
}
```

## Troubleshooting

1. **Creation Failures**

   - Check email uniqueness
   - Verify all required fields
   - Check network connection

2. **Update Issues**

   - Verify document exists
   - Check field types match
   - Ensure valid JSON format

3. **Data Retrieval Problems**
   - Verify collection path
   - Check document IDs
   - Validate query parameters

## Future Improvements

- Add batch operations support
- Implement offline data persistence
- Add real-time data synchronization
- Enhance error reporting
- Add data migration tools
