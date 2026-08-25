using System;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HowToFish.BossHpText
{
    /// <summary>
    /// Draws "current/total" inside the boss health bar.
    ///
    /// The game hands us everything we need in BossUI.UpdateBossHp(int curHp): the current HP
    /// arrives as the argument, and the maximum is the public static BossManager.BossMaxHp.
    /// That method is only called when the value actually changes, so a postfix is both the
    /// cheapest hook and a natural change-detector - no per-frame polling, no per-frame garbage.
    /// </summary>
    [HarmonyPatch(typeof(BossUI))]
    internal static class BossHpLabel
    {
        private const string LabelName = "BossHpTextLabel";

        // BossUI keeps these private. We need the bar to parent onto and the name label to
        // copy the font from, so the mod's typography matches the game's automatically.
        private static readonly AccessTools.FieldRef<BossUI, Image> BossHealthRef =
            AccessTools.FieldRefAccess<BossUI, Image>("_bossHealth");

        private static readonly AccessTools.FieldRef<BossUI, TextMeshProUGUI> BossNameRef =
            AccessTools.FieldRefAccess<BossUI, TextMeshProUGUI>("_bossNameText");

        private static TextMeshProUGUI _label;
        private static int _lastCur = int.MinValue;
        private static int _lastMax = int.MinValue;

        [HarmonyPatch(nameof(BossUI.UpdateBossHp))]
        [HarmonyPostfix]
        private static void ShowHpNumbers(BossUI __instance, int curHp)
        {
            // Never let an exception escape into the game's UI path: this runs on every hit,
            // and Unity would re-throw it every time for the rest of the session.
            try
            {
                if (!Plugin.Enabled.Value)
                {
                    // Comparing a destroyed Unity object to null is the supported check; the
                    // null-conditional operator would bypass Unity's == overload entirely.
                    if (_label != null)
                        _label.gameObject.SetActive(false);
                    return;
                }

                if (_label == null && !TryCreateLabel(__instance))
                    return;

                if (!_label.gameObject.activeSelf)
                    _label.gameObject.SetActive(true);

                int max = BossManager.BossMaxHp;
                if (curHp == _lastCur && max == _lastMax)
                    return;

                _lastCur = curHp;
                _lastMax = max;
                _label.text = SafeFormat(curHp, max);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Failed to update the boss HP label: " + e);
            }
        }

        private static bool TryCreateLabel(BossUI bossUi)
        {
            Image bar = BossHealthRef(bossUi);
            TextMeshProUGUI donor = BossNameRef(bossUi);

            if (bar == null || donor == null)
            {
                Plugin.Log.LogWarning("Boss bar or boss name label not found; skipping this boss.");
                return false;
            }

            // Parent to the fill image itself. Its rect spans the whole bar regardless of
            // fillAmount (that only drives the shader), so stretching to it centres the text
            // in the bar - and because the fill sits under the game's shake-and-spring
            // transform, the numbers inherit the damage animation for free.
            var go = new GameObject(LabelName, typeof(RectTransform));
            go.transform.SetParent(bar.rectTransform, false);

            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;

            _label = go.AddComponent<TextMeshProUGUI>();
            _label.font = donor.font;
            _label.fontSize = Plugin.FontSize.Value > 0f ? Plugin.FontSize.Value : donor.fontSize;
            _label.alignment = TextAlignmentOptions.Center;
            _label.overflowMode = TextOverflowModes.Overflow;
            _label.color = ParseColor(Plugin.TextColor.Value, Color.white);
            _label.raycastTarget = false;

            ApplyOutline(_label);

            // UI draws in sibling order, not by depth - last sibling wins.
            go.transform.SetAsLastSibling();

            _lastCur = int.MinValue;
            _lastMax = int.MinValue;

            Plugin.Log.LogInfo("Boss HP label attached to " + bar.name + ".");
            return true;
        }

        private static void ApplyOutline(TextMeshProUGUI label)
        {
            float width = Mathf.Clamp01(Plugin.OutlineWidth.Value);
            if (width <= 0f)
                return;

            // Reading fontMaterial gives this label its own material instance, so the outline
            // does not leak onto every other piece of text sharing the font asset.
            Material mat = label.fontMaterial;
            if (mat == null)
                return;

            mat.EnableKeyword(ShaderUtilities.Keyword_Outline);
            label.outlineColor = ParseColor(Plugin.OutlineColor.Value, Color.black);
            label.outlineWidth = width;
        }

        private static string SafeFormat(int cur, int max)
        {
            try
            {
                return string.Format(Plugin.Format.Value, cur, max);
            }
            catch (FormatException)
            {
                // A user typo in the config should not blank the label for the whole run.
                return cur + "/" + max;
            }
        }

        private static Color ParseColor(string hex, Color fallback)
        {
            return ColorUtility.TryParseHtmlString(hex, out Color parsed) ? parsed : fallback;
        }
    }
}
