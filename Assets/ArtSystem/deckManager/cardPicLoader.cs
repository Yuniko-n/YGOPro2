using UnityEngine;
using System.Collections;

public class cardPicLoader : MonoBehaviour
{

    public int loaded_code = -1;

    public int code = 0;

    public YGOSharp.Banlist loaded_banlist = null;

    public Texture2D defaults = null;

    public ban_icon ico;

    public ban_icon_ot ico_ot;

    public void clear()
    {
        loaded_code = 0;
        code = 0;
        ico.show(3);
        uiTexture.mainTexture = null;
    }

    public void reCode(int c)
    {
        loaded_code = 0;
        code = c;
    }

    public void relayer(int l)
    {
        uiTexture.depth = 50 + l * 2;
        UITexture t = ico.gameObject.GetComponent<UITexture>();
        t.depth = 51 + l * 2;
    }

    public Collider coli = null;

    void Update()
    {
        if (coli != null)
        {
            if (Program.InputGetMouseButtonDown_0)
            {
                if (Program.pointedCollider == coli)
                {
                    ((CardDescription)(Program.I().cardDescription)).setData(YGOSharp.CardsManager.Get(code), GameTextureManager.myBack,"",true);
                }
            }
        }
        if (Program.I().deckManager != null)
        {
            if (loaded_code != code)
            {
                Texture2D t = GameTextureManager.get(code, GameTextureType.card_picture, defaults);
                if (t != null)
                {
                    uiTexture.mainTexture = t;
                    uiTexture.aspectRatio = ((float)t.width) / ((float)t.height);
                    uiTexture.forceWidth((int)(uiTexture.height * uiTexture.aspectRatio));
                    loaded_code = code;
                    loaded_banlist = null;
                }
            }
            if (loaded_banlist != Program.I().deckManager.currentBanlist)
            {
                loaded_banlist = Program.I().deckManager.currentBanlist;
                if (ico != null)
                {
                    if (loaded_banlist == null)
                    {
                        ico.show(3);
                        return;
                    }
                    ico.show(loaded_banlist.GetQuantity(code));
                }
                if (ico_ot != null)
                {
                    //[1: OCG]、[2: TCG]、[3: OCG&TCG]、[4: Anime]
                    if (data.Ot == 2 || data.Ot == 4)
                    {
                        ico_ot.show(data.Ot);
                        return;
                    }
                    ico_ot.show(3);
                }
            }
        }
    }

    public UITexture uiTexture;

    public YGOSharp.Card data { get; set; }
}
