using System.IO.Compression;
using System.Text;
using System.Text.Json;
using DepthConsulting.Core.Messaging;
using DepthConsulting.Core.Services.Persistence.EventSource;

namespace PRDC2022.CustomerApi.Persistence
{
    public static class EventDescriptorExtensions
    {
        public static EventDescriptorEntity ToEventDescriptorEntity(this EventDescriptor obj)
        {
           var eventData = JsonSerializer.Serialize(obj.EventData, obj.EventData.GetType());

            return new EventDescriptorEntity
            {
                MessageId = obj.MessageId,
                CorrelationId = obj.CorrelationId,
                CausationId = obj.CausationId,
                AggregateId = obj.AggregateId,
                Version = obj.Version,
                ReceivedOn = obj.ReceivedOn,
                EventType = obj.EventData.GetType().AssemblyQualifiedName ?? string.Empty,
                EventData = eventData,
                AggregateType = obj.AggregateType
            };
        }

        public static SnapshotEntity ToSnapshotEntity(this EventDescriptor obj)
        {
            var eventData =JsonSerializer.Serialize(obj.EventData, obj.EventData.GetType());
            var compressedEventData = Compress(eventData);

            return new SnapshotEntity
            {
                MessageId = obj.MessageId,
                CorrelationId = obj.CorrelationId,
                CausationId = obj.CausationId,
                AggregateId = obj.AggregateId,
                Version = obj.Version,
                ReceivedOn = obj.ReceivedOn,
                EventType = obj.EventData.GetType().AssemblyQualifiedName ?? string.Empty,
                EventData = compressedEventData,
                AggregateType = obj.AggregateType
            };
        }

        public static EventDescriptor ToDescriptor(this BaseEventEntity obj)
        {
            var t = Type.GetType(obj.EventType);
            if (t == null)
            {
                throw new Exception("Unsupported type in serialized data");
            }

            var isEventDataCompressed = !obj.EventData.StartsWith("{");
            EventBase? eventData;
            if (isEventDataCompressed)
            {
                var decompressedString = Decompress(obj.EventData);
                eventData = JsonSerializer.Deserialize(decompressedString, t) as EventBase;
            }
            else
            {
                eventData =JsonSerializer.Deserialize(obj.EventData, t) as EventBase;
            }

            return new EventDescriptor(obj.AggregateId, obj.AggregateType, eventData, obj.Version, obj.ReceivedOn,
                obj.MessageId, obj.CorrelationId, obj.CausationId);
        }

        private static string Compress(string uncompressedString)
        {
            byte[] compressedBytes;
            using (var uncompressedStream = new MemoryStream(Encoding.UTF8.GetBytes(uncompressedString)))
            using (var compressedStream = new MemoryStream())
            {
                // setting the leaveOpen parameter to true to ensure that compressedStream will not be closed when compressorStream is disposed
                // this allows compressorStream to close and flush its buffers to compressedStream and guarantees that compressedStream.ToArray() can be called afterward
                // although MSDN documentation states that ToArray() can be called on a closed MemoryStream, I don't want to rely on that very odd behavior should it ever change
                using (var compressorStream = new DeflateStream(compressedStream, CompressionLevel.Fastest, true))
                {
                    uncompressedStream.CopyTo(compressorStream);
                }
                // call compressedStream.ToArray() after the enclosing DeflateStream has closed and flushed its buffer to compressedStream
                compressedBytes = compressedStream.ToArray();
            }
            return Convert.ToBase64String(compressedBytes);
        }

        /// <summary>
        /// Decompresses a deflate compressed, Base64 encoded string and returns an uncompressed string.
        /// </summary>
        /// <param name="compressedString">String to decompress.</param>
        private static string Decompress(string compressedString)
        {
            byte[] decompressedBytes;
            var compressedStream = new MemoryStream(Convert.FromBase64String(compressedString));
            using (var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
            using (var decompressedStream = new MemoryStream())
            {
                deflateStream.CopyTo(decompressedStream);
                decompressedBytes = decompressedStream.ToArray();
            }
            return Encoding.UTF8.GetString(decompressedBytes);
        }
    }

    public static class TypeConverter
    {
        private static Dictionary<string, Type>? _types;
        private static readonly object Lock = new();

        public static Type? FromString(string typeName)
        {
            if (_types == null) CacheTypes();

            if (_types != null && _types.ContainsKey(typeName))
                return _types[typeName];
            return null;
        }

        private static void CacheTypes()
        {
            lock (Lock)
            {
                if (_types == null)
                {
                    _types = new Dictionary<string, Type>();
                    // Initialize the myTypes list.
                    var baseType = typeof(CommandBase);
                    var proTransAssemblies = AppDomain.CurrentDomain
                        .GetAssemblies()
                        .Where(assembly => assembly.FullName != null && assembly.FullName.Contains("ProTrans"));
                    foreach (var ass in proTransAssemblies)
                    {
                        var types = ass.GetTypes().Where(t =>
                            (t.IsInterface || t.IsClass) &&
                            baseType.IsAssignableFrom(t) &&
                            baseType != t);
                        foreach (var type in types) _types.Add(type.Name, type);
                    }
                }
            }
        }
    }
}