using System;
using System.Collections.Generic;
using Unity.Netcode;

[Serializable]
public struct PlayerDeathEntry : INetworkSerializable
{
    public ulong ClientId;
    public ushort Round;

    public PlayerDeathEntry(ulong clientId, ushort round)
    {
        ClientId = clientId;
        Round = round;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref Round);
    }
}

[Serializable]
public struct EndGameReport : INetworkSerializable
{
    public ulong WinnerClientId;
    public List<PlayerDeathEntry> Timeline;
    
    public EndGameReport(ulong winnerClientId, List<PlayerDeathEntry> timeline) { WinnerClientId = winnerClientId; Timeline = timeline; }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref WinnerClientId);

        int timelineCount = serializer.IsReader ? 0 : Timeline.Count;
        serializer.SerializeValue(ref timelineCount);
        if (serializer.IsReader) Timeline = new List<PlayerDeathEntry>(timelineCount);
        for(int i = 0; i < timelineCount; i++)
        {
            var entry = serializer.IsReader ? default : Timeline[i];
            entry.NetworkSerialize(serializer);
            if(serializer.IsReader) Timeline.Add(entry);
        }
    }
}