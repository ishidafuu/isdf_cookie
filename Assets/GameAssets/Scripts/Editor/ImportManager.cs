﻿using System.Collections;
using UnityEditor;
using UnityEngine;

public class ImportProcess : AssetPostprocessor
{
    //画像のインポート時、インポート設定変更時に実行される関数
    void OnPreprocessTexture()
    {
        //assetImporterにインポートするオブジェクトが入る。それをテクスチャ型にキャスト
        TextureImporter importer = assetImporter as TextureImporter;
        importer.textureCompression = TextureImporterCompression.Compressed;
        importer.filterMode = FilterMode.Point;
        importer.spritePixelsPerUnit = 1;

        importer.textureCompression = TextureImporterCompression.Uncompressed;

        TextureImporterSettings textureSettings = new TextureImporterSettings();
        if (textureSettings.spriteMeshType != SpriteMeshType.FullRect)
        {
            textureSettings.spriteMeshType = SpriteMeshType.FullRect;
            // importer.SetTextureSettings(textureSettings);
            // importer.SaveAndReimport();
        }
        importer.ReadTextureSettings(textureSettings);

        // //ここからはインポートするテクスチャがどのフォルダ内にあるかで処理を変える
        // if (importer.assetPath.Contains("Character"))
        // {

        // }
    }
}
