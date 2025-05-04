using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;

[Serializable]
public class UserData
{
    public string Email;
    public bool CompletedGame;
    public string FavouriteExhibit;
    public string FullName;
    public string DateCreated;
    public string DateUpdated;
}

[Serializable]
public class StringValue
{
    public string stringValue;
}

[Serializable]
public class BoolValue
{
    public bool booleanValue;
}

[Serializable]
public class FirestoreFields
{
    public StringValue Email;
    public BoolValue CompletedGame;
    public StringValue FavouriteExhibit;
    public StringValue FullName;
    public StringValue DateCreated;
    public StringValue DateUpdated;
}

[Serializable]
public class FirestoreDocument
{
    public string name;  // Contains the document path including ID
    public FirestoreFields fields;
}

[Serializable]
public class FirestoreListResponse
{
    public List<FirestoreDocument> documents;
}

public class FirebaseService : MonoBehaviour
{
    private const string PROJECT_ID = "yraf-gallery-hop";
    private const string FirebaseBaseUrl = "https://firestore.googleapis.com/v1/projects/" + PROJECT_ID + "/databases/(default)/documents";

    private async Task<(bool exists, string docId)> FindDocumentByEmail(string email)
    {
        try
        {
            var url = $"{FirebaseBaseUrl}/Visitors";
            Debug.Log($"Listing documents to find email match: {url}");
            
            var www = new UnityEngine.Networking.UnityWebRequest(url, "GET");
            www.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            
            await www.SendWebRequest();
            
            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<FirestoreListResponse>(www.downloadHandler.text);
                if (response?.documents != null)
                {
                    foreach (var doc in response.documents)
                    {
                        if (doc.fields?.Email?.stringValue == email)
                        {
                            // Extract document ID from the name field (last part of the path)
                            string docId = doc.name.Substring(doc.name.LastIndexOf('/') + 1);
                            Debug.Log($"Found matching email in document: {docId}");
                            return (true, docId);
                        }
                    }
                }
            }
            else
            {
                Debug.LogError($"Failed to list documents: {www.error}");
            }
            
            return (false, null);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error checking for existing email: {e.Message}");
            return (false, null);
        }
    }

    public async Task SaveUserData(UserData userData)
    {
        try
        {
            Debug.Log($"Checking if user exists with email: {userData.Email}");
            
            // Check if a document with this email already exists
            var (exists, _) = await FindDocumentByEmail(userData.Email);
            Debug.Log($"Document exists check result: {exists}");
            
            if (exists)
            {
                throw new Exception("User already exists. Use UpdateFavouriteExhibit to modify existing users.");
            }

            // Set initial timestamps for new document
            userData.DateCreated = DateTime.UtcNow.ToString("o");
            userData.DateUpdated = userData.DateCreated;

            string jsonData = $@"{{
                ""fields"": {{
                    ""Email"": {{ ""stringValue"": ""{userData.Email}"" }},
                    ""CompletedGame"": {{ ""booleanValue"": {userData.CompletedGame.ToString().ToLower()} }},
                    ""FavouriteExhibit"": {{ ""stringValue"": ""{userData.FavouriteExhibit}"" }},
                    ""FullName"": {{ ""stringValue"": ""{userData.FullName}"" }},
                    ""DateCreated"": {{ ""stringValue"": ""{userData.DateCreated}"" }},
                    ""DateUpdated"": {{ ""stringValue"": ""{userData.DateUpdated}"" }}
                }}
            }}";

            Debug.Log($"Creating new user data: {jsonData}");

            var url = $"{FirebaseBaseUrl}/Visitors";  // POST to collection for auto-generated ID
            var www = new UnityEngine.Networking.UnityWebRequest(url, "POST");
            www.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonData));
            www.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            await www.SendWebRequest();

            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.Log("Successfully created new user data in Firestore");
            }
            else
            {
                Debug.LogError($"Failed to create user data: {www.error}");
                Debug.LogError($"Response: {www.downloadHandler.text}");
                throw new Exception($"Failed to create user data: {www.error}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error handling user data: {ex.Message}");
            throw;
        }
    }
}