using MemorialRecord.Data;
using System;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

public enum DataType
{
    Book,
    BookMark,
    Accessory,
    Room,
    Research,
}


public static class SaveManager
{
    public static event Action<int, DataType, int> OnUpdateLevel; // idx, Type, level
    public static event Action<double> OnChangeMemorial;
    public static event Action<long> OnChangeQuillPen;
    public static event Action<float> OnChangeMusicVolume;
    public static event Action<float> OnChangeSoundEffectVolume;

    public static double CurMemorial
    {
        get
        {
            return _data.currentMemorial;
        }
        set
        {
            _data.currentMemorial = value;
            OnChangeMemorial?.Invoke(value);
            SaveData();
        }
    }

    public static long CurQuillPen
    {
        get
        {
            return _data.currentQuillPen;
        }
        set
        {
            _data.currentQuillPen = value;
            OnChangeQuillPen?.Invoke(value);
            SaveData();
        }
    }

    public static float MusicVolume
    {
        get
        {
            return _data.musicVolume;
        }
        set
        {
            _data.musicVolume = value;
            OnChangeMusicVolume?.Invoke(value);
            SaveData();
        }
    }

    public static float SoundEffectVolume
    {
        get
        {
            return _data.soundEffectVolume;
        }
        set
        {
            _data.soundEffectVolume = value;
            OnChangeSoundEffectVolume?.Invoke(value);
            SaveData();
        }
    }

    public static string Name
    {
        get
        {
            return _data.characterName;
        }
        set
        {
            _data.characterName = value;
            SaveData();
        }
    }

    public static int IdxCurMusic
    {
        get
        {
            return _data.curMusicIndex;
        }
        set
        {
            _data.curMusicIndex = value;
            GameObject.FindObjectOfType<BGMSelector>()?.SetBGM(value);
            SaveData();
        }
    }

    public static uint IdxCurStory
    {
        get
        {
            return _data.idxCurStory;
        }

        set
        {
            _data.idxCurStory = value;
            SaveData();
        }
    }

    public static bool[] MusicBoughtArr
    {
        get
        {
            return _data.MusicBoughtArr;
        }

        set
        {
            _data.MusicBoughtArr = value;
            SaveData();
        }
    }

    #region readonly private fields
    public static readonly string _savePath = Application.persistentDataPath + "/SaveData.txt";
    public static readonly string _defaultSavePath = "Assets/Resources/DefaultSaveData.txt";
    private static readonly string _privateKey = "z$C&F)J@NcRfUjXn";//16byte ?????? ??????
    #endregion

    private static SaveData _data = new MemorialRecord.Data.SaveData();

    #region ???? ???????? ????
    public static void SetStoryDict(int idx, bool value)
    {
        if(!_data.storyReadDict.ContainsKey(idx))
        {
            _data.storyReadDict.Add(idx, value);
        }
        else
        {
            _data.storyReadDict[idx] = value;
        }
        SaveData();
    }

    public static double GetBookmarkValuePerSec()
    {
        double value = 0.0d;
        double roomValue = SaveManager.GetRoomValue();

        foreach (var item in _data.bookMarkLevelDict)
        {
            if(item.Value > -1)
            {
                if (TryGetContentLevel(DataType.Accessory, item.Key, out int level) && level > 0)
                {
                    value += ValueCalculator.GetOutputValueOnLevel(item.Key, item.Value) / 3 * 2;
                }
                else
                {
                    value += ValueCalculator.GetOutputValueOnLevel(item.Key, item.Value) / 3;
                }
            }
        }

        value *= roomValue < 1 ? 1 : roomValue;
        return value;
    }

    public static int GetRoomValue()
    {
        var roomlist = _data.roomInfoLevelsDict.ToList();
        roomlist.Sort((x, y) => 
        {
            if(y.Value.CompareTo(x.Value) == 0)
            {
                return y.Key.CompareTo(x.Key);
            }
            else
            {
                return y.Value.CompareTo(x.Value);
            }    
        });
        return roomlist[0].Key + 1;
    }

    public static bool TryGetContentLevel(DataType type, int idx, out int level)
    {
        switch (type)
        {
            case DataType.Book:
                if (!_data.bookLevelDict.ContainsKey(idx))
                {
                    break;
                }
                level = _data.bookLevelDict[idx];
                return true;
            case DataType.BookMark:
                if (!_data.bookMarkLevelDict.ContainsKey(idx))
                {
                    break;
                }
                level = _data.bookMarkLevelDict[idx];
                return true;
            case DataType.Accessory:
                if (!_data.accessoryLevelDict.ContainsKey(idx))
                {
                    break;
                }
                level = _data.accessoryLevelDict[idx];
                return true;
            case DataType.Room:
                if (!_data.roomInfoLevelsDict.ContainsKey(idx))
                {
                    break;
                }
                level = _data.roomInfoLevelsDict[idx];
                return true;
            case DataType.Research:
                // TODO : ???? ????
                break;
        }

        level = -2;
        return false;
    }

    /// <summary>
    /// ???? ???????? ?????? ??????????.
    /// ???? ???? ???? ???????? -1 ?? ??????????.
    /// ???? ?????? ???? ?????? -1 ?? ???????? ????????.
    /// ???? ?????? -2?? ??????????.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="idx"></param>
    /// <returns></returns>
    public static int GetContentLevel(DataType type, int idx)
    {
        switch (type)
        {
            case DataType.Book:
                if (!_data.bookLevelDict.ContainsKey(idx))
                {
                    _data.bookLevelDict.Add(idx, -1);
                }
                return _data.bookLevelDict[idx];
            case DataType.BookMark:
                if (!_data.bookMarkLevelDict.ContainsKey(idx))
                {
                    _data.bookMarkLevelDict.Add(idx, -1);
                }
                return _data.bookMarkLevelDict[idx];
            case DataType.Accessory:
                if (!_data.accessoryLevelDict.ContainsKey(idx))
                {
                    _data.accessoryLevelDict.Add(idx, -1);
                }
                return _data.accessoryLevelDict[idx];
            case DataType.Room:
                if (!_data.roomInfoLevelsDict.ContainsKey(idx))
                {
                    _data.roomInfoLevelsDict.Add(idx, -1);
                }
                return _data.roomInfoLevelsDict[idx];
            case DataType.Research:
                // TODO : ???? ????
                break;
        }

        return -2;
    }

    public static bool ContentLevelUp(int idx, DataType type)
    {
        bool isSuccessful = false;

        int updatedLevel = 0;
        switch (type)
        {
            case DataType.Book:
                if (_data.bookLevelDict.ContainsKey(idx))
                {
                    updatedLevel = ++_data.bookLevelDict[idx];
                    isSuccessful = true;
                }
                break;
            case DataType.BookMark:
                if (_data.bookMarkLevelDict.ContainsKey(idx))
                {
                    updatedLevel = ++_data.bookMarkLevelDict[idx];
                    isSuccessful = true;
                }
                break;
            case DataType.Accessory:
                if (_data.accessoryLevelDict.ContainsKey(idx))
                {
                    updatedLevel = ++_data.accessoryLevelDict[idx];
                    isSuccessful = true;
                }
                break;
            case DataType.Room:
                if (_data.roomInfoLevelsDict.ContainsKey(idx))
                {
                    updatedLevel = ++_data.roomInfoLevelsDict[idx];
                    isSuccessful = true;
                }
                break;
            case DataType.Research:
                // TODO : ???? ????
                break;
        }

        if (isSuccessful)
            OnUpdateLevel?.Invoke(idx, type, updatedLevel);

        return isSuccessful;
    }

    public static bool SetContentLevel(int idx, DataType type, int level)
    {
        bool isSuccessful = false;

        switch (type)
        {
            case DataType.Book:
                if (_data.bookLevelDict.ContainsKey(idx))
                {
                    _data.bookLevelDict[idx] = level;
                    isSuccessful = true;
                }
                break;
            case DataType.BookMark:
                if (_data.bookMarkLevelDict.ContainsKey(idx))
                {
                    _data.bookMarkLevelDict[idx] = level;
                    isSuccessful = true;
                }
                break;
            case DataType.Accessory:
                if (_data.accessoryLevelDict.ContainsKey(idx))
                {
                    _data.accessoryLevelDict[idx] = level;
                    isSuccessful = true;
                }
                break;
            case DataType.Room:
                if (_data.roomInfoLevelsDict.ContainsKey(idx))
                {
                    _data.roomInfoLevelsDict[idx] = level;
                    isSuccessful = true;
                }
                break;
            case DataType.Research:
                // TODO : ???? ????
                break;
        }

        if(isSuccessful) // ?????? ???????? ???? ?????? ????
            OnUpdateLevel?.Invoke(idx, type, level);

        return isSuccessful;
    }
    #endregion

    #region ?????? ???? ????
    public static void SaveData()
    {
        SaveFile(Encrypt(JsonUtility.ToJson(_data)));
    }

    public static SaveData LoadData()
    {
        //?????? ?????????????? ????.
        if (File.Exists(_savePath))
        {
            _data = JsonUtility.FromJson<SaveData>(Decrypt(LoadFile(_savePath)));
            return _data;
        }

        _data = new SaveData();
        if (!_data.roomInfoLevelsDict.ContainsKey(0))
        {
            _data.roomInfoLevelsDict.Add(0, 1);
        }
        else
        {
            _data.roomInfoLevelsDict[0] = 1;
        }
            
        return _data;
    }

    private static void SaveFile(string jsonData)
    {
        using (FileStream fs = new FileStream(_savePath, FileMode.Create, FileAccess.Write))
        {
            //?????? ?????? ?? ???? ????????
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

            //bytes?? ???????? 0 ~ max ???????? fs?? ????
            fs.Write(bytes, 0, bytes.Length);
        }
    }

    private static string LoadFile(string path)
    {
        using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
        {
            //?????? ???????? ???? ?? ???? ?????? ????
            byte[] bytes = new byte[(int)fs.Length];

            //?????????????? ???? ?????? ????
            fs.Read(bytes, 0, (int)fs.Length);

            //?????? ???????? json string???? ??????
            string jsonString = System.Text.Encoding.UTF8.GetString(bytes);
            return jsonString;
        }
    }
    #endregion

    #region ??????
    private static string Encrypt(string data)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);
        RijndaelManaged rm = CreateRijndaelManaged();
        ICryptoTransform ct = rm.CreateEncryptor();
        byte[] results = ct.TransformFinalBlock(bytes, 0, bytes.Length);
        return System.Convert.ToBase64String(results, 0, results.Length);
    }

    private static string Decrypt(string data)
    {
        byte[] bytes = System.Convert.FromBase64String(data);
        RijndaelManaged rm = CreateRijndaelManaged();
        ICryptoTransform ct = rm.CreateDecryptor();
        byte[] resultArray = ct.TransformFinalBlock(bytes, 0, bytes.Length);
        return System.Text.Encoding.UTF8.GetString(resultArray);
    }

    private static RijndaelManaged CreateRijndaelManaged()
    {
        byte[] keyArray = System.Text.Encoding.UTF8.GetBytes(_privateKey);
        RijndaelManaged result = new RijndaelManaged();

        byte[] newKeysArray = new byte[16];
        System.Array.Copy(keyArray, 0, newKeysArray, 0, 16);

        result.Key = newKeysArray;
        result.Mode = CipherMode.ECB;
        result.Padding = PaddingMode.PKCS7;
        return result;
    }
    #endregion
}
