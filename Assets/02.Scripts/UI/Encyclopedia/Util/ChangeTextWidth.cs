using UnityEngine;
using TMPro;

namespace Quiz
{
    public class ChangeTextWidth : MonoBehaviour
    {
        private TMP_Text tmpText;
        private RectTransform rectTransform;

        void Start()
        {
            tmpText = GetComponent<TMP_Text>();
            rectTransform = GetComponent<RectTransform>();

            // 초기 width 설정
            AdjustWidth();
        }

        void Update()
        {
            // 텍스트가 변경될 때마다 width 조절
            AdjustWidth();
        }

        private void AdjustWidth()
        {
            if (tmpText != null)
            {
                // 텍스트의 선호 크기를 계산
                Vector2 preferredSize = tmpText.GetPreferredValues();

                // RectTransform의 width를 텍스트 크기에 맞게 설정
                rectTransform.sizeDelta = new Vector2(preferredSize.x, rectTransform.sizeDelta.y);
            }
        }
    }
}
