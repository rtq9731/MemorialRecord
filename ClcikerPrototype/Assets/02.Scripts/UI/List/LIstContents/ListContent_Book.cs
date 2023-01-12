using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListContent_Book : ListContent
{
    public override void InitContent<T>(T data)
    {
        base.InitContent(data);

        int level = SaveManager.GetContentLevel(DataType.Book, data._idx);
        
        _textTitle.text = $"{data._title} Lv.{(level < 0 ? 0 : level)}";
        _textWriter.text = data._writer;
        _textOutput.text = $"{DataManager.GetValueF(ValueCalculator.GetOutputValue(DataType.Book, data))} / 터치";

        CommonRefresh(level);
        RefreshBtn(data, level);
    }

    public override void RefreshContent<T>(T data)
    {
        int level = SaveManager.GetContentLevel(DataType.Book, data._idx);
        _textTitle.text = $"{data._title} Lv.{(level < 0 ? 0 : level)}";

        CommonRefresh(level);
        RefreshBtn(data, level);
    }

    protected override void LevelUp<T>(T data)
    {
        if (SaveManager.GetContentLevel(DataType.Book, data._idx) >= 100)
            return;

        double upgradeValue = ValueCalculator.GetUpgradeValue(DataType.Book, data);
        if (SaveManager.CurMemorial >= upgradeValue)
        {
            SaveManager.CurMemorial -= upgradeValue;
            SaveManager.ContentLevelUp(data._idx, DataType.Book);
            RefreshContent(data);
            RefreshBtn(data, SaveManager.GetContentLevel(DataType.Book, data._idx));
        }
    }

    protected override void RefreshBtn<T>(T data, int level)
    {
        if (level == 20 && SaveManager.GetContentLevel(DataType.Book, data._idx + 1) == -1)
        {
            SaveManager.GetContentLevel(DataType.BookMark, data._idx + 1);

            SaveManager.SetContentLevel(data._idx + 1, DataType.Book, 0);
            SaveManager.SetContentLevel(data._idx + 1, DataType.BookMark, 0);
            FindObjectOfType<StoryAlarm>().CallStoryAlarm((int)SaveManager.IdxCurStory);
            OnLockOff();
        }

        if(level >= 100)
        {
            _textLevelUp.text = "완독";
            _textLevelUpCost.gameObject.SetActive(false);
            _textOutput.text = $"{DataManager.GetValueF(ValueCalculator.GetOutputValue(DataType.Book, data))} / 터치";
            _btnLevelup.onClick.RemoveAllListeners();
        }
        else
        {
            _textLevelUp.text = "구매\n";
            _textLevelUpCost.text = $"{DataManager.GetValueF(ValueCalculator.GetUpgradeValue(DataType.Book, data))}";
            _textOutput.text = $"{DataManager.GetValueF(ValueCalculator.GetOutputValue(DataType.Book, data))} / 터치";
        }
    }
}
