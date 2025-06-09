using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;
using System.IO;
using TMPro;

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

// New Vote data classes
[Serializable]
public class VoteData
{
    public string UUID;
    public string VoteValue;
    public string DateCreated;
    public string DateUpdated;
}

[Serializable]
public class VoteFirestoreFields
{
    public StringValue UUID;
    public StringValue VoteValue;
    public StringValue DateCreated;
    public StringValue DateUpdated;
}

[Serializable]
public class VoteFirestoreDocument
{
    public string name;
    public VoteFirestoreFields fields;
}

[Serializable]
public class VoteFirestoreListResponse
{
    public List<VoteFirestoreDocument> documents;
}

public class FirebaseService : MonoBehaviour
{
    private const string PROJECT_ID = "yraf-gallery-hop";
    private const string FirebaseBaseUrl = "https://firestore.googleapis.com/v1/projects/" + PROJECT_ID + "/databases/(default)/documents";
    private const string UUID_PLAYERPREFS_KEY = "UserUUID";

    // Triple-tap protection for report generation
    private int reportGenerationTapCount = 0;
    private float lastTapTime = 0f;
    private const float TAP_TIMEOUT = 1f; // Reset count after 1 seconds of no taps

    // UUID Management
    private string GetOrCreateUUID()
    {
        string uuid = PlayerPrefs.GetString(UUID_PLAYERPREFS_KEY, "");
        
        if (string.IsNullOrEmpty(uuid))
        {
            uuid = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString(UUID_PLAYERPREFS_KEY, uuid);
            PlayerPrefs.Save();
            Debug.Log($"Generated new UUID: {uuid}");
        }
        else
        {
            Debug.Log($"Using existing UUID: {uuid}");
        }
        
        return uuid;
    }

    // Vote CRUD Operations
    
    // Read: Check if UUID exists in vote collection
    private async Task<(bool exists, string docId, VoteData voteData)> FindVoteByUUID(string uuid)
    {
        try
        {
            var url = $"{FirebaseBaseUrl}/vote";
            Debug.Log($"Checking for existing vote with UUID: {uuid}");
            
            var www = new UnityEngine.Networking.UnityWebRequest(url, "GET");
            www.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            
            await www.SendWebRequest();
            
            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<VoteFirestoreListResponse>(www.downloadHandler.text);
                if (response?.documents != null)
                {
                    foreach (var doc in response.documents)
                    {
                        if (doc.fields?.UUID?.stringValue == uuid)
                        {
                            string docId = doc.name.Substring(doc.name.LastIndexOf('/') + 1);
                            var voteData = new VoteData
                            {
                                UUID = doc.fields.UUID.stringValue,
                                VoteValue = doc.fields.VoteValue.stringValue,
                                DateCreated = doc.fields.DateCreated.stringValue,
                                DateUpdated = doc.fields.DateUpdated.stringValue
                            };
                            Debug.Log($"Found existing vote for UUID: {uuid}, DocId: {docId}");
                            return (true, docId, voteData);
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Failed to check for existing vote: {www.error}");
            }
            
            return (false, null, null);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error checking for existing vote: {e.Message}");
            return (false, null, null);
        }
    }

    // Create: Add new vote
    private async Task<bool> CreateVote(string uuid, string voteValue)
    {
        try
        {
            string currentTimestamp = DateTime.UtcNow.ToString("o");
            
            string jsonData = $@"{{
                ""fields"": {{
                    ""UUID"": {{ ""stringValue"": ""{uuid}"" }},
                    ""VoteValue"": {{ ""stringValue"": ""{voteValue}"" }},
                    ""DateCreated"": {{ ""stringValue"": ""{currentTimestamp}"" }},
                    ""DateUpdated"": {{ ""stringValue"": ""{currentTimestamp}"" }}
                }}
            }}";

            Debug.Log($"Creating new vote: UUID={uuid}, Value={voteValue}");

            var url = $"{FirebaseBaseUrl}/vote";
            var www = new UnityEngine.Networking.UnityWebRequest(url, "POST");
            www.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonData));
            www.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            await www.SendWebRequest();

            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.Log("Successfully created new vote");
                return true;
            }
            else
            {
                Debug.LogError($"Failed to create vote: {www.error}");
                Debug.LogError($"Response: {www.downloadHandler.text}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error creating vote: {ex.Message}");
            return false;
        }
    }

    // Update: Update existing vote
    private async Task<bool> UpdateVote(string docId, string uuid, string voteValue)
    {
        try
        {
            string currentTimestamp = DateTime.UtcNow.ToString("o");
            
            string jsonData = $@"{{
                ""fields"": {{
                    ""UUID"": {{ ""stringValue"": ""{uuid}"" }},
                    ""VoteValue"": {{ ""stringValue"": ""{voteValue}"" }},
                    ""DateUpdated"": {{ ""stringValue"": ""{currentTimestamp}"" }}
                }}
            }}";

            Debug.Log($"Updating vote: DocId={docId}, UUID={uuid}, Value={voteValue}");

            var url = $"{FirebaseBaseUrl}/vote/{docId}";
            var www = new UnityEngine.Networking.UnityWebRequest(url, "PATCH");
            www.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonData));
            www.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            await www.SendWebRequest();

            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.Log("Successfully updated vote");
                return true;
            }
            else
            {
                Debug.LogError($"Failed to update vote: {www.error}");
                Debug.LogError($"Response: {www.downloadHandler.text}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error updating vote: {ex.Message}");
            return false;
        }
    }

    // Delete: Remove vote by UUID
    public async Task<bool> DeleteVoteByUUID(string uuid)
    {
        try
        {
            var (exists, docId, _) = await FindVoteByUUID(uuid);
            
            if (!exists)
            {
                Debug.LogWarning($"No vote found for UUID: {uuid}");
                return false;
            }

            var url = $"{FirebaseBaseUrl}/vote/{docId}";
            var www = new UnityEngine.Networking.UnityWebRequest(url, "DELETE");
            www.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();

            await www.SendWebRequest();

            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.Log($"Successfully deleted vote for UUID: {uuid}");
                return true;
            }
            else
            {
                Debug.LogError($"Failed to delete vote: {www.error}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error deleting vote: {ex.Message}");
            return false;
        }
    }

    // Overall Submit Vote Function
    public async Task<bool> SubmitVote(string voteValue)
    {
        try
        {
            string uuid = GetOrCreateUUID();
            
            // Check if user has already voted
            var (exists, docId, existingVoteData) = await FindVoteByUUID(uuid);
            
            if (exists)
            {
                // Check if the vote value is the same
                if (existingVoteData.VoteValue == voteValue)
                {
                    Debug.Log("Vote value is the same as existing vote. No update needed.");
                    return true;
                }
                
                // Update existing vote with new value
                Debug.Log("User has voted before. Updating with new value.");
                return await UpdateVote(docId, uuid, voteValue);
            }
            else
            {
                // Create new vote
                Debug.Log("User hasn't voted before. Creating new vote.");
                return await CreateVote(uuid, voteValue);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error submitting vote: {ex.Message}");
            return false;
        }
    }

    // Public method to check if user has voted
    public async Task<(bool hasVoted, string voteValue)> CheckUserVote()
    {
        try
        {
            string uuid = GetOrCreateUUID();
            var (exists, _, voteData) = await FindVoteByUUID(uuid);
            
            if (exists)
            {
                return (true, voteData.VoteValue);
            }
            
            return (false, null);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error checking user vote: {ex.Message}");
            return (false, null);
        }
    }

    // Unity-callable wrapper for generating vote tally report with triple-tap protection
    public async void GenerateVoteTallyReportForUnity()
    {
        float currentTime = Time.time;
        
        // Reset count if too much time has passed since last tap
        if (currentTime - lastTapTime > TAP_TIMEOUT)
        {
            reportGenerationTapCount = 0;
        }
        
        reportGenerationTapCount++;
        lastTapTime = currentTime;
        
        Debug.Log($"Report generation tap {reportGenerationTapCount}/3");
        
        if (reportGenerationTapCount < 3)
        {
            Debug.Log($"Tap {reportGenerationTapCount} more time(s) within {TAP_TIMEOUT} seconds to generate report");
            return;
        }
        
        // Reset count and generate report
        reportGenerationTapCount = 0;
        Debug.Log("Generating vote tally report...");
        await GenerateVoteTallyReport();
    }

    // Get all votes and create tally report
    public async Task<bool> GenerateVoteTallyReport()
    {
        try
        {
            Debug.Log("Starting vote tally report generation...");
            
            // Get all votes from Firebase
            var url = $"{FirebaseBaseUrl}/vote";
            var www = new UnityEngine.Networking.UnityWebRequest(url, "GET");
            www.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            
            await www.SendWebRequest();
            
            if (www.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to retrieve votes: {www.error}");
                return false;
            }

            var response = JsonUtility.FromJson<VoteFirestoreListResponse>(www.downloadHandler.text);
            
            if (response?.documents == null || response.documents.Count == 0)
            {
                Debug.Log("No votes found in database");
                
                // Create empty report
                string emptyReport = "VOTE TALLY REPORT\n";
                emptyReport += "=================\n";
                emptyReport += $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n";
                emptyReport += "No votes have been submitted yet.\n";
                
                await WriteReportToFile(emptyReport);
                return true;
            }

            // Dictionary to store vote counts
            var voteCounts = new Dictionary<string, int>();
            int totalVotes = 0;

            // Tally up the votes
            foreach (var doc in response.documents)
            {
                if (doc.fields?.VoteValue?.stringValue != null)
                {
                    string voteValue = doc.fields.VoteValue.stringValue;
                    
                    if (voteCounts.ContainsKey(voteValue))
                    {
                        voteCounts[voteValue]++;
                    }
                    else
                    {
                        voteCounts[voteValue] = 1;
                    }
                    
                    totalVotes++;
                }
            }

            // Sort by vote count (descending)
            var sortedVotes = voteCounts.OrderByDescending(kvp => kvp.Value).ToList();

            // Create report text
            var reportBuilder = new StringBuilder();
            reportBuilder.AppendLine("VOTE TALLY REPORT");
            reportBuilder.AppendLine("=================");
            reportBuilder.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            reportBuilder.AppendLine($"Total Votes: {totalVotes}");
            reportBuilder.AppendLine();
            reportBuilder.AppendLine("RESULTS (Highest to Lowest):");
            reportBuilder.AppendLine("----------------------------");

            int rank = 1;
            foreach (var vote in sortedVotes)
            {
                double percentage = totalVotes > 0 ? (double)vote.Value / totalVotes * 100 : 0;
                reportBuilder.AppendLine($"{rank}. {vote.Key}: {vote.Value} votes ({percentage:F1}%)");
                rank++;
            }

            reportBuilder.AppendLine();
            reportBuilder.AppendLine("=== END OF REPORT ===");

            string report = reportBuilder.ToString();
            Debug.Log("Generated vote tally report:\n" + report);

            // Write to file
            await WriteReportToFile(report);
            
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error generating vote tally report: {ex.Message}");
            return false;
        }
    }

    private async Task WriteReportToFile(string reportContent)
    {
        try
        {
            // Create filename with timestamp
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filename = $"VoteTallyReport_{timestamp}.txt";
            
#if UNITY_WEBGL && !UNITY_EDITOR
            // For WebGL builds, trigger browser download
            DownloadFileForWebGL(reportContent, filename);
            Debug.Log($"Vote tally report prepared for download: {filename}");
#else
            // For standalone builds and editor
            // Get the persistent data path (works across platforms)
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);
            
            // Write the file
            await System.IO.File.WriteAllTextAsync(filePath, reportContent);
            
            Debug.Log($"Vote tally report saved to: {filePath}");
            
            // Also log the path for easy access
            Debug.Log($"Full file path: {filePath}");
            
#if UNITY_EDITOR
            // In editor, also save to Assets folder for easy access
            string editorPath = System.IO.Path.Combine(Application.dataPath, "..", "VoteReports");
            if (!System.IO.Directory.Exists(editorPath))
            {
                System.IO.Directory.CreateDirectory(editorPath);
            }
            
            string editorFilePath = System.IO.Path.Combine(editorPath, filename);
            await System.IO.File.WriteAllTextAsync(editorFilePath, reportContent);
            Debug.Log($"Editor copy saved to: {editorFilePath}");
#endif
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error writing report to file: {ex.Message}");
            throw;
        }
    }

    #if UNITY_WEBGL && !UNITY_EDITOR
    private void DownloadFileForWebGL(string content, string filename)
    {
        // Convert content to base64 for web download
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(content);
        string base64 = System.Convert.ToBase64String(bytes);
        
        // Create data URL
        string dataUrl = "data:text/plain;base64," + base64;
        
        // Trigger download using JavaScript
        Application.ExternalEval($@"
            var link = document.createElement('a');
            link.download = '{filename}';
            link.href = '{dataUrl}';
            link.click();
        ");
    }
    #endif

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