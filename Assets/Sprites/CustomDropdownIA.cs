using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CustomDropdownIA : TMP_Dropdown
{
    [SerializeField] private List<Sprite> iconosPorEstilo;

    protected override GameObject CreateDropdownList(GameObject template)
    {
        var list = base.CreateDropdownList(template);

        StartCoroutine(AsignarIconos(list.transform.Find("Viewport/Content")));
        return list;
    }

    private IEnumerator AsignarIconos(Transform content)
    {
        yield return null; // esperamos 1 frame para que se instancien

        for (int i = 0; i < content.childCount && i < iconosPorEstilo.Count; i++)
        {
            var item = content.GetChild(i);
            var pic = item.Find("Pic")?.GetComponent<Image>();
            if (pic != null)
                pic.sprite = iconosPorEstilo[i];
        }
    }
}
