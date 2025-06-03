using UnityEngine;

namespace Base_Scripts
{
    public static class PerlinNoise
    {
        /// <summary>
        /// Génère un bruit Perlin 1D (résultat dans [0,1]).
        /// </summary>
        public static float GenerateNoise(
            float x,
            float scale = 1f,
            float offset = 0f
        )
        {
            // On projette en 2D sur l’axe X, Y = 0
            return Mathf.PerlinNoise(x * scale + offset, 0f);
        }

        /// <summary>
        /// Génère un bruit Perlin 2D (résultat dans [0,1]).
        /// </summary>
        public static float GenerateNoise(
            Vector2 position,
            float scale = 1f,
            Vector2 offset = default
        )
        {
            return Mathf.PerlinNoise(
                position.x * scale + offset.x,
                position.y * scale + offset.y
            );
        }

        /// <summary>
        /// Génère un bruit Perlin « 3D » en combinant trois projections 2D.
        /// La moyenne des trois plans XY, YZ, ZX limite la répétition.
        /// </summary>
        public static float GenerateNoise(
            Vector3 position,
            float scale = 1f,
            Vector3 offset = default
        )
        {
            float xy = Mathf.PerlinNoise(
                position.x * scale + offset.x,
                position.y * scale + offset.y
            );
            float yz = Mathf.PerlinNoise(
                position.y * scale + offset.y,
                position.z * scale + offset.z
            );
            float zx = Mathf.PerlinNoise(
                position.z * scale + offset.z,
                position.x * scale + offset.x
            );
            return (xy + yz + zx) / 3f;
        }

        /// <summary>
        /// Génère un Perlin noise fractal (octaves).
        /// </summary>
        public static float GenerateFractalNoise(
            Vector3 position,
            float scale = 1f,
            Vector3 offset = default,
            int octaves = 3,
            float lacunarity = 2f,
            float gain = 0.5f
        )
        {
            float amplitude = 1f;
            float frequency = scale;
            float sum = 0f;
            float max = 0f;

            for (int i = 0; i < octaves; i++)
            {
                float n = GenerateNoise(
                    position * frequency,
                    1f,
                    offset * frequency
                );
                sum += n * amplitude;
                max += amplitude;
                frequency *= lacunarity;
                amplitude *= gain;
            }

            return sum / max;
        }
    }
}
