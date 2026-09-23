using SomnusNet.Data;
using UnityEngine;
using UnityEngine.UI;

namespace SomnusNet.UI
{
    public class BlobUpgradePreviewCard : MonoBehaviour
    {
        Text _title;
        Text _description;
        Text _cost;
        Text _projectileCaption;
        Text _effectCaption;
        Image _projectilePreview;
        Image _effectPreview;
        RectTransform _projectilePreviewRt;
        RectTransform _effectPreviewRt;
        BlobKind _blobKind;
        BlobProjectilePreviewKind _projectileKind;
        BlobProjectilePreviewKind _effectKind;
        float _timer;
        bool _active;

        public void Bind(Text title, Text description, Text cost, Text projectileCaption, Text effectCaption,
            Image projectilePreview, Image effectPreview)
        {
            _title = title;
            _description = description;
            _cost = cost;
            _projectileCaption = projectileCaption;
            _effectCaption = effectCaption;
            _projectilePreview = projectilePreview;
            _effectPreview = effectPreview;
            _projectilePreviewRt = projectilePreview != null ? projectilePreview.rectTransform : null;
            _effectPreviewRt = effectPreview != null ? effectPreview.rectTransform : null;
        }

        public void Show(string title, string description, string costLine, BlobKind blobKind,
            BlobProjectilePreviewKind projectilePreview, BlobProjectilePreviewKind effectPreview)
        {
            _blobKind = blobKind;
            _projectileKind = projectilePreview;
            _effectKind = effectPreview;
            _timer = 0f;
            _active = true;
            gameObject.SetActive(true);

            if (_title != null)
                _title.text = title;
            if (_description != null)
                _description.text = description;
            if (_cost != null)
            {
                _cost.text = costLine ?? string.Empty;
                _cost.gameObject.SetActive(!string.IsNullOrEmpty(costLine));
            }

            ApplyPreviewLayout(effectPreview != BlobProjectilePreviewKind.None);
            RefreshSprites();
        }

        public void Hide()
        {
            _active = false;
            gameObject.SetActive(false);
        }

        void ApplyPreviewLayout(bool showEffect)
        {
            if (_projectilePreviewRt != null)
            {
                if (showEffect)
                {
                    _projectilePreviewRt.anchorMin = new Vector2(0.52f, 0.08f);
                    _projectilePreviewRt.anchorMax = new Vector2(0.7f, 0.66f);
                }
                else
                {
                    _projectilePreviewRt.anchorMin = new Vector2(0.52f, 0.06f);
                    _projectilePreviewRt.anchorMax = new Vector2(0.94f, 0.68f);
                }

                _projectilePreviewRt.offsetMin = _projectilePreviewRt.offsetMax = Vector2.zero;
            }

            if (_projectileCaption != null)
            {
                _projectileCaption.gameObject.SetActive(showEffect && _projectileKind != BlobProjectilePreviewKind.None);
                _projectileCaption.text = "Projectile";
            }

            if (_effectPreviewRt != null)
                _effectPreviewRt.gameObject.SetActive(showEffect);
            if (_effectCaption != null)
                _effectCaption.gameObject.SetActive(showEffect);

            if (_projectilePreview != null && _projectileKind != BlobProjectilePreviewKind.None)
            {
                _projectilePreview.preserveAspect = true;
                _projectilePreview.color = EncyclopediaProjectilePreviews.GetTint(_projectileKind, _blobKind);
            }

            if (_effectPreview != null && showEffect)
            {
                _effectPreview.preserveAspect = true;
                _effectPreview.color = EncyclopediaProjectilePreviews.GetTint(_effectKind, _blobKind);
            }
        }

        void Update()
        {
            if (!_active)
                return;

            if (EncyclopediaProjectilePreviews.IsAnimated(_projectileKind)
                || EncyclopediaProjectilePreviews.IsAnimated(_effectKind))
                _timer += Time.unscaledDeltaTime;

            RefreshSprites();
        }

        void RefreshSprites()
        {
            RefreshPreviewImage(_projectilePreview, _projectileKind);
            RefreshPreviewImage(_effectPreview, _effectKind);
        }

        void RefreshPreviewImage(Image image, BlobProjectilePreviewKind kind)
        {
            if (image == null || !_active)
                return;

            if (kind == BlobProjectilePreviewKind.None)
            {
                image.enabled = false;
                return;
            }

            var frame = EncyclopediaProjectilePreviews.GetFrame(kind, _timer, image.color);
            image.sprite = frame;
            image.enabled = frame != null;
        }
    }
}
