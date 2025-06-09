using System;
using System.Collections.Generic;

[Serializable]
public class LoginData // UNITY CANNOT SERIALISE DICTIONARIES, SO A SMALL DETOUR IS USED
{
    public List<UserDetails> userDetails;
    public Dictionary<string, string> dict = new();

    public void PopulateDictionary()
    {
        foreach (UserDetails pair in userDetails)
            dict[pair.username] = pair.password;
    }

    public void PopulateList()
    {
        userDetails = new List<UserDetails>();
        foreach (KeyValuePair<string, string> pair in dict)
        {
            UserDetails sp = new()
            {
                username = pair.Key,
                password = pair.Value
            };
            userDetails.Add(sp);
        }
    }
}

[Serializable]
public class UserDetails
{
    public string username;
    public string password;
}
