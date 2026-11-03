using UnityEngine;
using UnityEngine.UI;

namespace Underfoot
{
    [RequireComponent(typeof(Image))]
    public class UnderfootMessagePanel : MonoBehaviour
    {
        public Text message;
        void LateUpdate() { GetComponent<Image>().enabled = !string.IsNullOrEmpty(message.text); }
    }
}
