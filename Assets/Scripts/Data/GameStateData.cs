using System;
using System.Collections.Generic;

[Serializable]
public class GameStateData
{
    public int saveVersion = 1;
    public string lastSavedIso;

    public List<int> completedTaskIds = new List<int>();
    public List<int> startedTaskIds = new List<int>();

    [Serializable]
    public class NpcQueueEntry { public string npcGuid; public List<int> taskIds = new List<int>(); }

    public List<NpcQueueEntry> giverQueues = new List<NpcQueueEntry>();
    public List<NpcQueueEntry> receiverQueues = new List<NpcQueueEntry>();

    public SerializableVector3 playerPosition = new SerializableVector3(0, 0, 0);

    [Serializable]
    public struct SerializableVector3 { public float x, y, z; public SerializableVector3(float X, float Y, float Z) { x = X; y = Y; z = Z; } }
}
