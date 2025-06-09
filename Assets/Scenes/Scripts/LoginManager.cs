using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using System.IO;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField signInUsernameField;
    public TMP_InputField signInPasswordField;

    public GameObject errorObject;
    public TMP_Text errorText;

    public GameObject mainMenu;

    /// <summary>
    /// Attempts to log in the user with the provided username and password
    /// </summary>
    /// </remarks>
    /// Ensures a valid key.json file exists and contains the user details
    /// </remarks>
    public void Login()
    {
        string username = signInUsernameField.text.Trim().ToLower();
        string password = signInPasswordField.text.Trim();
        
        ValidateUsernameAndPassword(username, password);

        string filePath = Application.persistentDataPath + "/key.json";
        if (!File.Exists(filePath))
        {
            SetError("Invalid username or password.");
            return;
        }
        string json = File.ReadAllText(filePath);
        LoginData loginData = JsonUtility.FromJson<LoginData>(json);
        if (loginData == null || loginData.userDetails == null)
        {
            SetError("Invalid username or password.");
            return;
        }
        loginData.PopulateDictionary();

        byte[] hashedPassword = CreateHash(password);
        string hashedPasswordString = ByteArrayToString(hashedPassword);

        if (loginData.dict.TryGetValue(username, out string storedPassword))
        {
            if (storedPassword.Equals(hashedPasswordString))
            {
                MenuManager.CloseMenu(gameObject);
                MenuManager.OpenMenu(mainMenu);
                JsonManager.CreateDirectory(username); // Create a directory for the user if it doesn't exist
                JsonManager.SetWorkingDirectory(username);
            }
            else
            {
                SetError("Invalid username or password.");
            }
        }
        else
        {
            SetError("Invalid username or password.");
        }
    }

    /// <summary>
    /// Validates the username and password fields for login
    /// This method checks if the fields are empty and sets an error message if they are
    /// </summary>
    /// <param name="username">The user's username</param>
    /// <param name="password">The user's password pre-hash conversion</param>
    public void ValidateUsernameAndPassword(string username, string password)
    {
        if (string.IsNullOrEmpty(username))
        {
            SetError("Username cannot be empty.");
            return;
        }
        else if (string.IsNullOrEmpty(password))
        {
            SetError("Password cannot be empty.");
            return;
        }
    }

    public void SetError(string message)
    {
        errorObject.SetActive(true);
        errorText.text = message;
    }


    public void ResetMenu()
    {
        errorObject.SetActive(false);
        errorText.text = string.Empty;
        signInUsernameField.text = string.Empty;
        signInPasswordField.text = string.Empty;
    }


    public static byte[] CreateHash(string input)
    {
        return SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(input));
    }

    public static string ByteArrayToString(byte[] arrInput)
    {
        int i;
        StringBuilder sOutput = new(arrInput.Length);
        for (i = 0; i < arrInput.Length; i++)
        {
            sOutput.Append(arrInput[i].ToString("X2"));
        }
        return sOutput.ToString();
    }
}
