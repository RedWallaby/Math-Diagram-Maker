using System.IO;
using TMPro;
using UnityEngine;

public class RegistrationManager : MonoBehaviour
{
    public TMP_InputField registerUsernameField;
    public TMP_InputField registerPasswordField;

    public GameObject errorObject;
    public TMP_Text errorText;

    public GameObject registrationSucessMenu;

    /// <summary>
    /// Attempts to register a new user with the provided username and password
    /// </summary>
    public void Register()
    {
        string username = registerUsernameField.text.Trim().ToLower();
        string password = registerPasswordField.text.Trim();

        if (!ValidateUsernameAndPassword(username, password)) return;

        byte[] hashedPassword = LoginManager.CreateHash(password);
        string hashedPasswordString = LoginManager.ByteArrayToString(hashedPassword);

        bool newUsername = InsertIntoKey(username, hashedPasswordString);
        if (newUsername)
        {
            MenuManager.CloseMenu(gameObject);
            MenuManager.OpenMenu(registrationSucessMenu);
            JsonManager.CreateDirectory(username);
        }
        else
        {
            SetError("Username already exists.");
        }
    }

    /// <summary>
    /// Validates the username and password according to specified rules
    /// This includes checking for empty fields, length restrictions, and character requirements
    /// </summary>
    /// <param name="username">The user's username</param>
    /// <param name="password">The user's password pre-hash conversion</param>
    /// <returns>Whether the username and password were both valid</returns>
    public bool ValidateUsernameAndPassword(string username, string password)
    {
        if (string.IsNullOrEmpty(username))
        {
            SetError("Username cannot be empty");
        }
        else if (string.IsNullOrEmpty(password))
        {
            SetError("Password cannot be empty");
        }
        else if (username.Length > 50)
        {
            SetError("Username is too long (maximum 50 characters)");
        }
        else if (password.Length > 50)
        {
            SetError("Password is too long (maximum 50 characters)");
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
        {
            SetError("Username can only contain letters, numbers, and underscores");
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&])[A-Za-z\d@$!%*#?&]{8,}$"))
        {
            SetError("Password must be at least 8 characters, contain at least one letter, one number, and one special character");
        }
        else
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Attempts to insert a new username and password into the key.json file
    /// </summary>
    /// <param name="username">The user's username</param>
    /// <param name="password">The user's password post-hash conversion</param>
    /// <returns>Whether the username was new</returns>
    public bool InsertIntoKey(string username, string password)
    {
        string path = Application.persistentDataPath + "/key.json";
        LoginData loginData = new();
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            LoginData newData = JsonUtility.FromJson<LoginData>(json);
            if (newData != null) loginData = newData;
            if (loginData.userDetails != null)
            {
                loginData.PopulateDictionary();
            }
        }
        bool success = loginData.dict.TryAdd(username, password);
        loginData.PopulateList();
        string wrapperJson = JsonUtility.ToJson(loginData, true);
        File.WriteAllText(path, wrapperJson);
        return success;
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
        registerUsernameField.text = string.Empty;
        registerPasswordField.text = string.Empty;
    }
}
