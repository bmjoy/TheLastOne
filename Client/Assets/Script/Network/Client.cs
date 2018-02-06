// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace Game.Hyun
{

    using global::System;
    using global::FlatBuffers;

    public struct Client : IFlatbufferObject
    {
        private Table __p;
        public ByteBuffer ByteBuffer { get { return __p.bb; } }
        public static Client GetRootAsClient(ByteBuffer _bb) { return GetRootAsClient(_bb, new Client()); }
        public static Client GetRootAsClient(ByteBuffer _bb, Client obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
        public void __init(int _i, ByteBuffer _bb) { __p.bb_pos = _i; __p.bb = _bb; }
        public Client __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

        public Vec3? Pos { get { int o = __p.__offset(4); return o != 0 ? (Vec3?)(new Vec3()).__assign(o + __p.bb_pos, __p.bb) : null; } }
        public Vec3? Rotation { get { int o = __p.__offset(6); return o != 0 ? (Vec3?)(new Vec3()).__assign(o + __p.bb_pos, __p.bb) : null; } }
        public Vec3? View { get { int o = __p.__offset(8); return o != 0 ? (Vec3?)(new Vec3()).__assign(o + __p.bb_pos, __p.bb) : null; } }

        public static void StartClient(FlatBufferBuilder builder) { builder.StartObject(3); }
        public static void AddPos(FlatBufferBuilder builder, Offset<Vec3> posOffset) { builder.AddStruct(0, posOffset.Value, 0); }
        public static void AddRotation(FlatBufferBuilder builder, Offset<Vec3> rotationOffset) { builder.AddStruct(1, rotationOffset.Value, 0); }
        public static void AddView(FlatBufferBuilder builder, Offset<Vec3> viewOffset) { builder.AddStruct(2, viewOffset.Value, 0); }
        public static Offset<Client> EndClient(FlatBufferBuilder builder)
        {
            int o = builder.EndObject();
            return new Offset<Client>(o);
        }
    };


}