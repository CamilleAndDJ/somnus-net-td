using SomnusNet.Data;
using UnityEngine;
using UnityEngine.UI;

namespace SomnusNet.UI
{
    public class BlobProjectilePreviewRow : MonoBehaviour
    {
        Text _label;
        Image _image;
        BlobProjectilePreviewKind _kind;
        BlobKind _blobKind;
        float _timer;
        bool _active;

        public void Bind(Text label, Image image)
        {
            _label = label;
            _image = image;
        }

        public void Show(string label, BlobKind blobKind, BlobProjectilePreviewKind kind)
        {
            _blobKind = blobKind;
            _kind = kind;
            _timer = 0f;
            _active = true;
            gameObject.SetActive(true);

            if (_label != null)
            {
                _label.text = kind == BlobProjectilePreviewKind.None
                    ? $"{label}\n(ability)"
                    : label;
            }

            if (_image != null)
            {
                _image.preserveAspect = true;
                _image.color = EncyclopediaProjectilePreviews.GetTint(kind, blobKind);
            }

            RefreshSprite();
        }

        public void Hide()
        {
            _active = false;
            gameObject.SetActive(false);
        }

        void Update()
        {
            if (!_active || _image == null)
                return;

            if (EncyclopediaProjectilePreviews.IsAnimated(_kind))
                _timer += Time.unscaledDeltaTime;

            RefreshSprite();
        }

        void RefreshSprite()
        {
            if (_image == null || !_active)
                return;

            if (_kind == BlobProjectilePreviewKind.None)
            {
                _image.enabled = false;
                return;
            }

            var frame = EncyclopediaProjectilePreviews.GetFrame(_kind, _timer, _image.color);
            _image.sprite = frame;
            _image.enabled = frame != null;
        }
    }
}
