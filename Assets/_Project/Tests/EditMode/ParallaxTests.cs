using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace ChromeDino.Tests
{
    public sealed class ParallaxTests
    {
        [Test]
        public void LargeOvershootAndManyWrapsRetainContinuousCoverageAndResetExactly()
        {
            GameObject root = new GameObject("Parallax test");
            try
            {
                Camera camera = root.AddComponent<Camera>();
                camera.orthographic = true;
                camera.orthographicSize = 5f;
                camera.aspect = 16f / 9f;
                ParallaxLayer layer = root.AddComponent<ParallaxLayer>();
                Transform[] tiles = new Transform[3];
                for (int i = 0; i < tiles.Length; i++)
                {
                    tiles[i] = new GameObject("Tile " + i).transform;
                    tiles[i].SetParent(root.transform);
                    tiles[i].position = new Vector3(-10f + 20f * i, 2f, 0f);
                }

                typeof(ParallaxLayer).GetField("_tiles", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(layer, tiles);
                Assert.That(layer.Initialize(camera), Is.True);
                float[] positions = new float[3];
                for (int step = 0; step < 1000; step++)
                {
                    layer.Scroll(step == 0 ? 1234.567f : 0.37f);
                    for (int i = 0; i < tiles.Length; i++) positions[i] = tiles[i].position.x;
                    System.Array.Sort(positions);
                    Assert.That(positions[1] - positions[0], Is.EqualTo(20f).Within(0.0001f));
                    Assert.That(positions[2] - positions[1], Is.EqualTo(20f).Within(0.0001f));
                    Assert.That(positions[0], Is.LessThanOrEqualTo(-camera.orthographicSize * camera.aspect));
                    Assert.That(positions[2] + 20f, Is.GreaterThanOrEqualTo(camera.orthographicSize * camera.aspect));
                }

                layer.ResetRun();
                for (int i = 0; i < tiles.Length; i++)
                    Assert.That(tiles[i].position, Is.EqualTo(new Vector3(-10f + 20f * i, 2f, 0f)));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
