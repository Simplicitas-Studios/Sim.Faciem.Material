using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sim.Faciem.Material.Editor
{
    [CustomPropertyDrawer(typeof(Background))]
    public sealed class BackgroundPropertyDrawer : PropertyDrawer
    {
        private const string TextureOption = "Texture";
        private const string SpriteOption = "Sprite";
        private const string RenderTextureOption = "RenderTexture";
        private const string VectorImageOption = "Vector Image";

        private static readonly List<string> s_options = new()
        {
            TextureOption,
            SpriteOption,
            RenderTextureOption,
            VectorImageOption,
        };

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            root.style.flexDirection = FlexDirection.Column;

            var label = new Label(property.displayName);
            label.style.marginBottom = 2;
            root.Add(label);

            var sourceType = GetInitialSourceType((Background)property.boxedValue);

            var dropdown = new DropdownField("Source", s_options, sourceType);
            root.Add(dropdown);

            var objectField = new ObjectField("Asset");
            objectField.allowSceneObjects = false;
            root.Add(objectField);

            void RefreshObjectField(bool notify)
            {
                var background = (Background)property.boxedValue;
                var currentSource = dropdown.value;
                objectField.objectType = GetObjectType(currentSource);
                var asset = GetAsset(background, currentSource);

                if (notify)
                {
                    objectField.value = asset;
                }
                else
                {
                    objectField.SetValueWithoutNotify(asset);
                }
            }

            dropdown.RegisterValueChangedCallback(evt =>
            {
                var background = (Background)property.boxedValue;
                var next = CreateBackground(evt.newValue, GetAsset(background, evt.newValue));
                property.boxedValue = next;
                property.serializedObject.ApplyModifiedProperties();
                RefreshObjectField(false);
            });

            objectField.RegisterValueChangedCallback(evt =>
            {
                var next = CreateBackground(dropdown.value, evt.newValue);
                property.boxedValue = next;
                property.serializedObject.ApplyModifiedProperties();
            });

            RefreshObjectField(false);
            return root;
        }

        private static string GetInitialSourceType(Background background)
        {
            if (background.texture != null)
            {
                return TextureOption;
            }

            if (background.sprite != null)
            {
                return SpriteOption;
            }

            if (background.renderTexture != null)
            {
                return RenderTextureOption;
            }

            if (background.vectorImage != null)
            {
                return VectorImageOption;
            }

            return VectorImageOption;
        }

        private static Type GetObjectType(string sourceType)
        {
            return sourceType switch
            {
                TextureOption => typeof(Texture2D),
                SpriteOption => typeof(Sprite),
                RenderTextureOption => typeof(RenderTexture),
                VectorImageOption => typeof(VectorImage),
                _ => typeof(VectorImage),
            };
        }

        private static UnityEngine.Object GetAsset(Background background, string sourceType)
        {
            return sourceType switch
            {
                TextureOption => background.texture,
                SpriteOption => background.sprite,
                RenderTextureOption => background.renderTexture,
                VectorImageOption => background.vectorImage,
                _ => background.vectorImage,
            };
        }

        private static Background CreateBackground(string sourceType, UnityEngine.Object asset)
        {
            return sourceType switch
            {
                TextureOption => new Background { texture = asset as Texture2D },
                SpriteOption => new Background { sprite = asset as Sprite },
                RenderTextureOption => new Background { renderTexture = asset as RenderTexture },
                VectorImageOption => new Background { vectorImage = asset as VectorImage },
                _ => new Background { vectorImage = asset as VectorImage },
            };
        }
    }
}
