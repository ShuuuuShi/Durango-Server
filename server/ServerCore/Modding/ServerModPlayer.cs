using DurangoServer.Modding;
using Messages;

namespace DurangoServer.Core;

/// <summary>ตัวห่อ ServerPlayer จริงให้ mod มองเห็นผ่าน IModPlayer เท่านั้น — ไม่ให้ mod แตะ
/// ServerPlayer ตรง ๆ (กัน mod พังตอนโครงสร้างภายในเซิร์ฟเปลี่ยน)</summary>
internal sealed class ServerModPlayer : IModPlayer
{
    private readonly ServerPlayer _player;

    public ServerModPlayer(ServerPlayer player)
    {
        _player = player;
    }

    public string EntityId => _player.EntityId;
    public string Name => _player.Name;
    public int Level => _player.Level;
    public bool IsDead => _player.Dead;
    public int TileX => (int)(_player.CurrentPosition.x / 200f);
    public int TileY => (int)(_player.CurrentPosition.y / 200f);

    public void SendMessage(string text) => _player.Send(new Info { Text = text });

    public void Teleport(int tileX, int tileY) => _player.ControlTeleport(tileX, tileY);
}
