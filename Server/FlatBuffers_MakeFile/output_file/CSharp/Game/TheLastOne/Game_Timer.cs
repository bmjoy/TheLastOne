// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace Game.TheLastOne
{

using global::System;
using global::FlatBuffers;

public struct Game_Timer : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static Game_Timer GetRootAsGame_Timer(ByteBuffer _bb) { return GetRootAsGame_Timer(_bb, new Game_Timer()); }
  public static Game_Timer GetRootAsGame_Timer(ByteBuffer _bb, Game_Timer obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p.bb_pos = _i; __p.bb = _bb; }
  public Game_Timer __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public int Kind { get { int o = __p.__offset(4); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
  public int Time { get { int o = __p.__offset(6); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }

  public static Offset<Game_Timer> CreateGame_Timer(FlatBufferBuilder builder,
      int kind = 0,
      int time = 0) {
    builder.StartObject(2);
    Game_Timer.AddTime(builder, time);
    Game_Timer.AddKind(builder, kind);
    return Game_Timer.EndGame_Timer(builder);
  }

  public static void StartGame_Timer(FlatBufferBuilder builder) { builder.StartObject(2); }
  public static void AddKind(FlatBufferBuilder builder, int kind) { builder.AddInt(0, kind, 0); }
  public static void AddTime(FlatBufferBuilder builder, int time) { builder.AddInt(1, time, 0); }
  public static Offset<Game_Timer> EndGame_Timer(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<Game_Timer>(o);
  }
};


}