using UnityEngine;
using System.Collections;
using System.IO;

public class ban_icon_ot : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void show(int i)
    {
        UITexture t = gameObject.GetComponent<UITexture>();
        if (t != null)
        {
            if (i == 2 || i == 4)  //[1: OCG]、[2: TCG]、[3: OCG&TCG]、[4: Anime]
            {
                t.mainTexture = GameTextureManager.get("ban_ot_" + i.ToString());
            }
            else
            {
                t.mainTexture = GameTextureManager.get("ban_ot_3");
            }
        }
        else
        {
            Renderer r = GetComponent<Renderer>();
            if (i == 2 || i == 4)  //[1: OCG]、[2: TCG]、[3: OCG&TCG]、[4: Anime]
            {
                r.material.mainTexture = GameTextureManager.get("ban_ot_" + i.ToString());
            }
            else
            {
                r.material.mainTexture = GameTextureManager.get("ban_ot_3");
            }
        }
    }
}
