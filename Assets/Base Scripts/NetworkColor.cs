namespace Base_Scripts
{
    using Unity.Netcode;
    using UnityEngine;

    public struct NetworkColor : INetworkSerializable
    {
        // Les données que nous voulons synchroniser
        public float r, g, b, a;

        // L'implémentation de l'interface
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            // Sérialise chaque composante float
            serializer.SerializeValue(ref r);
            serializer.SerializeValue(ref g);
            serializer.SerializeValue(ref b);
            serializer.SerializeValue(ref a);
        }

        // --- Astuces pour une utilisation facile ---

        // Permet de convertir une NetworkColor en Color (lecture)
        public static implicit operator Color(NetworkColor nc)
        {
            return new Color(nc.r, nc.g, nc.b, nc.a);
        }

        // Permet de convertir une Color en NetworkColor (écriture)
        public static implicit operator NetworkColor(Color c)
        {
            return new NetworkColor { r = c.r, g = c.g, b = c.b, a = c.a };
        }
    }
}