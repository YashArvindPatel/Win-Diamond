using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem
{
    static string path = Application.persistentDataPath + "/player.save";

    public static void SaveInfo(GameManager manager)
    {
        BinaryFormatter formatter = new BinaryFormatter();

        FileStream stream = new FileStream(path, FileMode.Create);

        PlayerData data = new PlayerData(manager);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static void ResetInfo()
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public static PlayerData LoadInfo()
    {
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();

            FileStream stream = new FileStream(path, FileMode.Open);

            if (stream.Length == 0)
            {
                stream.Close();
                return null;
            }

            PlayerData data = (PlayerData)formatter.Deserialize(stream);
            stream.Close();

            return data;
        }
        else
        {
            //Debug.Log("Save Path not found in " + path);
            return null;
        }
    }
}
